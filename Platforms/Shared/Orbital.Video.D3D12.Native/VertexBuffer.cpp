#include "VertexBuffer.h"

extern "C"
{
	ORBITAL_EXPORT VertexBuffer* Orbital_Video_D3D12_VertexBuffer_Create(Device* device, VertexBufferMode mode)
	{
		VertexBuffer* handle = (VertexBuffer*)calloc(1, sizeof(VertexBuffer));
		handle->device = device;
		handle->mode = mode;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_VertexBuffer_Init(VertexBuffer* handle, void* vertices, uint32_t vertexCount, uint32_t vertexSize)
	{
		uint64_t bufferSize = vertexSize * vertexCount;

		// create buffer
		D3D12_HEAP_PROPERTIES heapProperties = {};
		if (handle->mode == VertexBufferMode_GPUOptimized) heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
		else if (handle->mode == VertexBufferMode_Write) heapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;
		else if (handle->mode == VertexBufferMode_Read) heapProperties.Type = D3D12_HEAP_TYPE_READBACK;
		else return 0;
        heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
        heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
        heapProperties.CreationNodeMask = 1;// TODO: multi-gpu setup
        heapProperties.VisibleNodeMask = 1;

		D3D12_RESOURCE_DESC resourceDesc = {};
		resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
		resourceDesc.Alignment = 0;
		resourceDesc.Width = bufferSize;
		resourceDesc.Height = 1;
		resourceDesc.DepthOrArraySize = 1;
		resourceDesc.MipLevels = 1, 
        resourceDesc.Format = DXGI_FORMAT_UNKNOWN;
		resourceDesc.SampleDesc.Count = 1;
		resourceDesc.SampleDesc.Quality = 0;
		resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
		resourceDesc.Flags = D3D12_RESOURCE_FLAG_NONE;

		handle->resourceState = D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER;
		if (vertices != NULL && handle->mode == VertexBufferMode_GPUOptimized) handle->resourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for gpu copy
		else if (handle->mode == VertexBufferMode_Read) handle->resourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for CPU read
		else if (handle->mode == VertexBufferMode_Write) handle->resourceState = D3D12_RESOURCE_STATE_GENERIC_READ;// init for frequent cpu writes
		if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, handle->resourceState, nullptr, IID_PPV_ARGS(&handle->resource)))) return 0;

		// upload cpu buffer to gpu
		if (vertices != NULL)
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
			memcpy(gpuDataPtr, vertices, bufferSize);
			uploadResource->Unmap(0, nullptr);

			// copy upload buffer to default buffer
			if (useUploadBuffer)
			{
				// reset command list and copy resource
				handle->device->internalMutex->lock();
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

		// create view
		handle->resourceView.BufferLocation = handle->resource->GetGPUVirtualAddress();
        handle->resourceView.StrideInBytes = vertexSize;
        handle->resourceView.SizeInBytes = bufferSize;

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_VertexBuffer_Dispose(VertexBuffer* handle)
	{
		if (handle->resource != NULL)
		{
			handle->resource->Release();
			handle->resource = NULL;
		}

		free(handle);
	}
}

void Orbital_Video_D3D12_VertexBuffer_ChangeState(VertexBuffer* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList)
{
	if (handle->resourceState == state) return;
	if (handle->mode == VertexBufferMode_Read || handle->mode == VertexBufferMode_Write) return;
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