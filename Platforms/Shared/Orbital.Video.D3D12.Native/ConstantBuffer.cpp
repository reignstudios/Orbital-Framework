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

	ORBITAL_EXPORT int Orbital_Video_D3D12_ConstantBuffer_Init(ConstantBuffer* handle, UINT32 size, void* initialData)
	{
		int alignedSize = (size + 255) & ~255;// size is required to be 256-byte aligned

		// create resource
		D3D12_HEAP_PROPERTIES properties = {};
		properties.Type = handle->mode == ConstantBufferMode::Static ? D3D12_HEAP_TYPE_DEFAULT : D3D12_HEAP_TYPE_UPLOAD;
        properties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
        properties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
        properties.CreationNodeMask = 1;// TODO: multi-gpu setup
        properties.VisibleNodeMask = 1;

		D3D12_RESOURCE_DESC desc = {};
		desc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
        desc.Alignment = 0;
        desc.Width = alignedSize;
        desc.Height = 1;
        desc.DepthOrArraySize = 1;
        desc.MipLevels = 1;
        desc.Format = DXGI_FORMAT_UNKNOWN;
        desc.SampleDesc.Count = 1;
        desc.SampleDesc.Quality = 0;
        desc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
        desc.Flags = D3D12_RESOURCE_FLAG_NONE;

		D3D12_RESOURCE_STATES initialResourceState = D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER;
		if (initialData != NULL)
		{
			if (handle->mode == ConstantBufferMode::Update) initialResourceState = D3D12_RESOURCE_STATE_GENERIC_READ;// init for frequent gpu updates
			else if (handle->mode == ConstantBufferMode::Static) initialResourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for gpu copy
			else return 0;
		}
		if (FAILED(handle->device->device->CreateCommittedResource(&properties, D3D12_HEAP_FLAG_NONE, &desc, initialResourceState, NULL, IID_PPV_ARGS(&handle->resource)))) return 0;

		// create resource heap
		D3D12_DESCRIPTOR_HEAP_DESC cbvHeapDesc = {};
        cbvHeapDesc.NumDescriptors = 1;
        cbvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
        cbvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
        if (FAILED(handle->device->device->CreateDescriptorHeap(&cbvHeapDesc, IID_PPV_ARGS(&handle->resourceHeap)))) return 0;

		// create resource view
		D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
        cbvDesc.BufferLocation = handle->resource->GetGPUVirtualAddress();
        cbvDesc.SizeInBytes = alignedSize;
		D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = handle->resourceHeap->GetCPUDescriptorHandleForHeapStart();
        handle->device->device->CreateConstantBufferView(&cbvDesc, cpuHandle);
		handle->resourceHeapHandle = handle->resourceHeap->GetGPUDescriptorHandleForHeapStart();

		// upload initial data
		if (initialData != NULL)
		{
			// allocate gpu upload buffer if needed
			bool useUploadBuffer = false;
			ID3D12Resource* uploadResource = handle->resource;
			if (properties.Type != D3D12_HEAP_TYPE_UPLOAD)
			{
				useUploadBuffer = true;
				uploadResource = NULL;
				properties.Type = D3D12_HEAP_TYPE_UPLOAD;
				if (FAILED(handle->device->device->CreateCommittedResource(&properties, D3D12_HEAP_FLAG_NONE, &desc, D3D12_RESOURCE_STATE_GENERIC_READ, NULL, IID_PPV_ARGS(&uploadResource)))) return 0;
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
				handle->device->internalCommandList->Reset(handle->device->commandAllocator, NULL);
				handle->device->internalCommandList->CopyResource(handle->resource, uploadResource);

				// change resource to function as constant buffer
				D3D12_RESOURCE_BARRIER barrier = {};
				barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				barrier.Transition.pResource = handle->resource;
				barrier.Transition.StateBefore = initialResourceState;
				barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER;
				barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
				handle->device->internalCommandList->ResourceBarrier(1, &barrier);
				handle->device->internalCommandList->Close();

				// execute operations
				ID3D12CommandList* commandLists[1] = { handle->device->internalCommandList };
				handle->device->commandQueue->ExecuteCommandLists(1, commandLists);
				WaitForFence(handle->device, handle->device->internalFence, handle->device->internalFenceEvent, handle->device->internalFenceValue);

				// release temp resource
				uploadResource->Release();
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
}