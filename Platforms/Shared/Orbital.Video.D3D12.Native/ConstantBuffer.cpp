#include "ConstantBuffer.h"

extern "C"
{
	ORBITAL_EXPORT ConstantBuffer* Orbital_Video_D3D12_ConstantBuffer_Create(Device* device, ConstantBufferMode mode)
	{
		ConstantBuffer* handle = (ConstantBuffer*)calloc(1, sizeof(ConstantBuffer));
		handle->device = device;
		handle->mode = mode;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ConstantBuffer_Init(ConstantBuffer* handle, UINT32 size, int* alignedSize, void* initialData)
	{
		const UINT32 alignment = D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT - 1;
		*alignedSize = (size + alignment) & ~alignment;// size is required to be aligned
		
		// create resource
		D3D12_HEAP_PROPERTIES heapProperties = {};
		if (handle->mode == ConstantBufferMode_GPUOptimized) heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
		else if (handle->mode == ConstantBufferMode_Write) heapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;
		else if (handle->mode == ConstantBufferMode_Read) heapProperties.Type = D3D12_HEAP_TYPE_READBACK;
		else return 0;
        heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
        heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
        heapProperties.CreationNodeMask = 1;// TODO: multi-gpu setup
        heapProperties.VisibleNodeMask = 1;

		D3D12_RESOURCE_DESC resourceDesc = {};
		resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
        resourceDesc.Alignment = 0;
        resourceDesc.Width = *alignedSize;
        resourceDesc.Height = 1;
        resourceDesc.DepthOrArraySize = 1;
        resourceDesc.MipLevels = 1;
        resourceDesc.Format = DXGI_FORMAT_UNKNOWN;
        resourceDesc.SampleDesc.Count = 1;
        resourceDesc.SampleDesc.Quality = 0;
        resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
        resourceDesc.Flags = D3D12_RESOURCE_FLAG_NONE;

		handle->resourceState = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
		if (initialData != NULL && handle->mode == ConstantBufferMode_GPUOptimized) handle->resourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for gpu copy
		else if (handle->mode == ConstantBufferMode_Read) handle->resourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for CPU read
		else if (handle->mode == ConstantBufferMode_Write) handle->resourceState = D3D12_RESOURCE_STATE_GENERIC_READ;// init for frequent cpu writes
		if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, handle->resourceState, NULL, IID_PPV_ARGS(&handle->resource)))) return 0;

		// create resource heap
		D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
        heapDesc.NumDescriptors = 1;
        heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
        heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
        if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->resourceHeap)))) return 0;
		handle->resourceHeapHandle = handle->resourceHeap->GetGPUDescriptorHandleForHeapStart();

		// create resource view
		D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
        cbvDesc.BufferLocation = handle->resource->GetGPUVirtualAddress();
        cbvDesc.SizeInBytes = *alignedSize;
        handle->device->device->CreateConstantBufferView(&cbvDesc, handle->resourceHeap->GetCPUDescriptorHandleForHeapStart());

		// upload initial data
		if (initialData != NULL)
		{
			// allocate gpu upload buffer if needed
			bool useUploadBuffer = false;
			ID3D12Resource* uploadResource = handle->resource;
			if (heapProperties.Type != D3D12_HEAP_TYPE_UPLOAD)
			{
				useUploadBuffer = true;
				uploadResource = NULL;
				heapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;
				if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, D3D12_RESOURCE_STATE_GENERIC_READ, NULL, IID_PPV_ARGS(&uploadResource)))) return 0;
			}

			// copy CPU memory to GPU
			UINT8* gpuDataPtr;
			D3D12_RANGE readRange = {};
			if (FAILED(uploadResource->Map(0, &readRange, reinterpret_cast<void**>(&gpuDataPtr))))
			{
				if (useUploadBuffer) uploadResource->Release();
				return 0;
			}
			memcpy(gpuDataPtr, initialData, size);
			uploadResource->Unmap(0, nullptr);

			// copy upload buffer to default buffer
			if (useUploadBuffer)
			{
				handle->device->internalMutex->lock();
				// reset command list and copy resource
				handle->device->internalCommandList->Reset(handle->device->commandAllocator, NULL);
				handle->device->internalCommandList->CopyResource(handle->resource, uploadResource);

				// close command list
				handle->device->internalCommandList->Close();

				// execute operations
				ID3D12CommandList* commandLists[1] = { handle->device->internalCommandList };
				handle->device->commandQueue->ExecuteCommandLists(1, commandLists);
				WaitForFence(handle->device, handle->device->internalFence, handle->device->internalFenceEvent, handle->device->internalFenceValue);

				// release temp resource
				uploadResource->Release();
				handle->device->internalMutex->unlock();
			}
		}

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_Dispose(ConstantBuffer* handle)
	{
		if (handle->resourceHeap != NULL)
		{
			handle->resourceHeap->Release();
			handle->resourceHeap = NULL;
		}

		if (handle->resource != NULL)
		{
			handle->resource->Release();
			handle->resource = NULL;
		}

		free(handle);
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ConstantBuffer_BeginUpdate(ConstantBuffer* handle)
	{
		D3D12_RANGE readRange = {};
		if (FAILED(handle->resource->Map(0, &readRange, reinterpret_cast<void**>(&handle->updateDataPtr)))) return 0;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_EndUpdate(ConstantBuffer* handle)
	{
		handle->resource->Unmap(0, nullptr);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_Update(ConstantBuffer* handle, void* data, UINT dataSize, UINT offset)
	{
		memcpy(handle->updateDataPtr + offset, data, dataSize);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_UpdateArray(ConstantBuffer* handle, void* data, UINT dataElementSize, UINT dataElementCount, UINT offset, UINT srcStride, UINT dstStride)
	{
		UINT dataSize = dataElementSize * dataElementCount;
		if (srcStride == dstStride)
		{
			memcpy(handle->updateDataPtr + offset, data, dataSize);
		}
		else
		{
			byte* srcDataPtr = (byte*)data;
			UINT dstOffset = offset;
			UINT dataRead = 0;
			while (dataRead < dataSize)
			{
				memcpy(handle->updateDataPtr + dstOffset, srcDataPtr, srcStride);
				dataRead += srcStride;
				srcDataPtr += srcStride;
				dstOffset += dstStride;
			}
		}
	}
}

void Orbital_Video_D3D12_ConstantBuffer_ChangeState(ConstantBuffer* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList)
{
	if (handle->resourceState == state) return;
	if (handle->mode == ConstantBufferMode_Read || handle->mode == ConstantBufferMode_Write) return;
	D3D12_RESOURCE_BARRIER barrier = {};
	barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
	barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
	barrier.Transition.pResource = handle->resource;
	barrier.Transition.StateBefore = handle->resourceState;
	barrier.Transition.StateAfter = state;
	barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
	commandList->ResourceBarrier(1, &barrier);
	handle->resourceState = state;
}