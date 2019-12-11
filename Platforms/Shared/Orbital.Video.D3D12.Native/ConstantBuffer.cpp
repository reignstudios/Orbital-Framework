#include "ConstantBuffer.h"

extern "C"
{
	ORBITAL_EXPORT ConstantBuffer* Orbital_Video_D3D12_ConstantBuffer_Create(Device* device)
	{
		ConstantBuffer* handle = (ConstantBuffer*)calloc(1, sizeof(ConstantBuffer));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ConstantBuffer_Init(ConstantBuffer* handle, UINT32 size, void* initialData)
	{
		int alignedSize = (size + 255) & ~255;// size is required to be 256-byte aligned

		// create resource
		D3D12_HEAP_PROPERTIES properties = {};
		properties.Type = D3D12_HEAP_TYPE_UPLOAD;
        properties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
        properties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
        properties.CreationNodeMask = 1;
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

		if (FAILED(handle->device->device->CreateCommittedResource(&properties, D3D12_HEAP_FLAG_NONE, &desc, D3D12_RESOURCE_STATE_GENERIC_READ, NULL, IID_PPV_ARGS(&handle->resource)))) return 0;

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
			UINT8* gpuDataPtr;
			D3D12_RANGE readRange = {};
			if (FAILED(handle->resource->Map(0, &readRange, reinterpret_cast<void**>(&gpuDataPtr)))) return 0;
			memcpy(gpuDataPtr, initialData, size);
			handle->resource->Unmap(0, nullptr);
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