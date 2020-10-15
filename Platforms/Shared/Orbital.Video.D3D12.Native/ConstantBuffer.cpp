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
		
		// create nodes
		handle->nodes = (ConstantBufferNode*)calloc(handle->device->nodeCount, sizeof(ConstantBufferNode));
		for (UINT n = 0; n != handle->device->nodeCount; ++n)
		{
			// create resource
			D3D12_HEAP_PROPERTIES heapProperties = {};
			if (handle->mode == ConstantBufferMode_GPUOptimized) heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
			else if (handle->mode == ConstantBufferMode_Write) heapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;
			else if (handle->mode == ConstantBufferMode_Read) heapProperties.Type = D3D12_HEAP_TYPE_READBACK;
			else return 0;
			heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
			heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
			heapProperties.CreationNodeMask = handle->device->nodes[n].mask;
			heapProperties.VisibleNodeMask = handle->device->nodes[n].mask;

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

			handle->nodes[n].resourceState = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			if (initialData != NULL && handle->mode == ConstantBufferMode_GPUOptimized) handle->nodes[n].resourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for gpu copy
			else if (handle->mode == ConstantBufferMode_Read) handle->nodes[n].resourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for CPU read
			else if (handle->mode == ConstantBufferMode_Write) handle->nodes[n].resourceState = D3D12_RESOURCE_STATE_GENERIC_READ;// init for frequent cpu writes
			if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, handle->nodes[n].resourceState, NULL, IID_PPV_ARGS(&handle->nodes[n].resource)))) return 0;

			// create resource heap
			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NodeMask = handle->device->nodes[n].mask;
			heapDesc.NumDescriptors = 1;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->nodes[n].resourceHeap)))) return 0;
			handle->nodes[n].resourceHeapHandle = handle->nodes[n].resourceHeap->GetGPUDescriptorHandleForHeapStart();

			// create resource view
			D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
			cbvDesc.BufferLocation = handle->nodes[n].resource->GetGPUVirtualAddress();
			cbvDesc.SizeInBytes = *alignedSize;
			handle->device->device->CreateConstantBufferView(&cbvDesc, handle->nodes[n].resourceHeap->GetCPUDescriptorHandleForHeapStart());

			// upload initial data
			if (initialData != NULL)
			{
				// allocate gpu upload buffer if needed
				bool useUploadBuffer = false;
				ID3D12Resource* uploadResource = handle->nodes[n].resource;
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
					// reset command list and copy resource
					handle->device->nodes[n].internalMutex->lock();
					handle->device->nodes[n].internalCommandAllocator->Reset();
					handle->device->nodes[n].internalCommandList->Reset(handle->device->nodes[n].internalCommandAllocator, NULL);
					handle->device->nodes[n].internalCommandList->CopyResource(handle->nodes[n].resource, uploadResource);

					// close command list
					handle->device->nodes[n].internalCommandList->Close();

					// execute operations
					ID3D12CommandList* commandLists[1] = { handle->device->nodes[n].internalCommandList };
					handle->device->nodes[n].commandQueue->ExecuteCommandLists(1, commandLists);
					WaitForFence(handle->device, n, handle->device->nodes[n].internalFence, handle->device->nodes[n].internalFenceEvent, handle->device->nodes[n].internalFenceValue);

					// release temp resource
					uploadResource->Release();
					handle->device->nodes[n].internalMutex->unlock();
				}
			}
		}

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_Dispose(ConstantBuffer* handle)
	{
		if (handle->nodes != NULL)
		{
			for (UINT n = 0; n != handle->device->nodeCount; ++n)
			{
				if (handle->nodes[n].resourceHeap != NULL)
				{
					handle->nodes[n].resourceHeap->Release();
					handle->nodes[n].resourceHeap = NULL;
				}

				if (handle->nodes[n].resource != NULL)
				{
					handle->nodes[n].resource->Release();
					handle->nodes[n].resource = NULL;
				}
			}

			free(handle->nodes);
			handle->nodes = NULL;
		}

		free(handle);
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ConstantBuffer_BeginUpdate(ConstantBuffer* handle, int nodeIndex)
	{
		D3D12_RANGE readRange = {};
		ConstantBufferNode* activeNode = &handle->nodes[nodeIndex];
		if (FAILED(activeNode->resource->Map(0, &readRange, reinterpret_cast<void**>(&activeNode->updateDataPtr)))) return 0;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_EndUpdate(ConstantBuffer* handle, int nodeIndex)
	{
		D3D12_RANGE readRange = {};
		ConstantBufferNode* activeNode = &handle->nodes[nodeIndex];
		activeNode->resource->Unmap(0, &readRange);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_Update(ConstantBuffer* handle, void* data, UINT dataSize, UINT offset, int nodeIndex)
	{
		ConstantBufferNode* activeNode = &handle->nodes[nodeIndex];
		memcpy(activeNode->updateDataPtr + offset, data, dataSize);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_UpdateArray(ConstantBuffer* handle, void* data, UINT dataElementSize, UINT dataElementCount, UINT offset, UINT srcStride, UINT dstStride, int nodeIndex)
	{
		ConstantBufferNode* activeNode = &handle->nodes[nodeIndex];
		UINT dataSize = dataElementSize * dataElementCount;
		if (srcStride == dstStride)
		{
			memcpy(activeNode->updateDataPtr + offset, data, dataSize);
		}
		else
		{
			byte* srcDataPtr = (byte*)data;
			UINT dstOffset = offset;
			UINT dataRead = 0;
			while (dataRead < dataSize)
			{
				memcpy(activeNode->updateDataPtr + dstOffset, srcDataPtr, srcStride);
				dataRead += srcStride;
				srcDataPtr += srcStride;
				dstOffset += dstStride;
			}
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_UpdateColorArray(ConstantBuffer* handle, void* data, UINT dataElementSize, UINT dataElementCount, UINT offset, UINT srcStride, UINT dstStride, int nodeIndex)
	{
		ConstantBufferNode* activeNode = &handle->nodes[nodeIndex];
		UINT dataSize = dataElementSize * dataElementCount;
		byte* srcDataPtr = (byte*)data;
		UINT dstOffset = offset;
		UINT dataRead = 0;
		while (dataRead < dataSize)
		{
			float srcDataVec[4];
			srcDataVec[0] = srcDataPtr[0] / 255.0f;
			srcDataVec[1] = srcDataPtr[1] / 255.0f;
			srcDataVec[2] = srcDataPtr[2] / 255.0f;
			if (srcStride >= 4) srcDataVec[3] = srcDataPtr[3] / 255.0f;
			memcpy(activeNode->updateDataPtr + dstOffset, srcDataVec, srcStride);
			dataRead += srcStride;
			srcDataPtr += srcStride;
			dstOffset += dstStride;
		}
	}
}

void Orbital_Video_D3D12_ConstantBuffer_ChangeState(ConstantBuffer* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList)
{
	ConstantBufferNode* activeNode = &handle->nodes[nodeIndex];
	if (activeNode->resourceState == state) return;
	if (handle->mode == ConstantBufferMode_Read || handle->mode == ConstantBufferMode_Write) return;
	D3D12_RESOURCE_BARRIER barrier = {};
	barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
	barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
	barrier.Transition.pResource = activeNode->resource;
	barrier.Transition.StateBefore = activeNode->resourceState;
	barrier.Transition.StateAfter = state;
	barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
	commandList->ResourceBarrier(1, &barrier);
	activeNode->resourceState = state;
}