#include "VertexBuffer.h"

extern "C"
{
	ORBITAL_EXPORT VertexBuffer* Orbital_Video_D3D12_VertexBuffer_Create(Device* device)
	{
		VertexBuffer* handle = (VertexBuffer*)calloc(1, sizeof(VertexBuffer));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_VertexBuffer_Init(VertexBuffer* handle, void* vertices, uint32_t vertexCount, uint32_t vertexSize)
	{
		uint64_t bufferSize = vertexSize * vertexCount;

		// create buffer
		D3D12_HEAP_PROPERTIES heapProperties = {};
		heapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;
        heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
        heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
        heapProperties.CreationNodeMask = 1;
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

		if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, D3D12_RESOURCE_STATE_GENERIC_READ, nullptr, IID_PPV_ARGS(&handle->vertexBuffer)))) return 0;

		// upload cpu buffer to gpu
		UINT8* gpuDataPtr;
        D3D12_RANGE readRange = {};
        if (FAILED(handle->vertexBuffer->Map(0, &readRange, reinterpret_cast<void**>(&gpuDataPtr)))) return 0;
        memcpy(gpuDataPtr, vertices, bufferSize);
        handle->vertexBuffer->Unmap(0, nullptr);

		// create view
		handle->vertexBufferView.BufferLocation = handle->vertexBuffer->GetGPUVirtualAddress();
        handle->vertexBufferView.StrideInBytes = vertexSize;
        handle->vertexBufferView.SizeInBytes = bufferSize;

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_VertexBuffer_Dispose(VertexBuffer* handle)
	{
		if (handle->vertexBuffer != NULL)
		{
			handle->vertexBuffer->Release();
			handle->vertexBuffer = NULL;
		}

		free(handle);
	}
}