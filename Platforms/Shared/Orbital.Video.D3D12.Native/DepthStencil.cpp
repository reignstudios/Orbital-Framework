#include "DepthStencil.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT DepthStencil* Orbital_Video_D3D12_DepthStencil_Create(Device* device, DepthStencilMode mode)
	{
		DepthStencil* handle = (DepthStencil*)calloc(1, sizeof(DepthStencil));
		handle->device = device;
		handle->mode = mode;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_DepthStencil_Init(DepthStencil* handle, DepthStencilFormat format, UINT32 width, UINT32 height)
	{
		if (!GetNative_DepthStencilFormat(format, &handle->format)) return 0;

		// create resource
		D3D12_HEAP_PROPERTIES heapProperties = {};
		if (handle->mode == DepthStencilMode_GPUOptimized) heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
		else return 0;
        heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
        heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
        heapProperties.CreationNodeMask = 1;// TODO: multi-gpu setup
        heapProperties.VisibleNodeMask = 1;

		D3D12_RESOURCE_DESC resourceDesc = {};
		resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
        resourceDesc.Alignment = 0;
        resourceDesc.Width = width;
        resourceDesc.Height = height;
        resourceDesc.DepthOrArraySize = 1;
        resourceDesc.MipLevels = 1;
        resourceDesc.Format = handle->format;
        resourceDesc.SampleDesc.Count = 1;
        resourceDesc.SampleDesc.Quality = 0;
        resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
        resourceDesc.Flags = D3D12_RESOURCE_FLAG_NONE;

		handle->resourceState = D3D12_RESOURCE_STATE_DEPTH_WRITE;
		if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, handle->resourceState, NULL, IID_PPV_ARGS(&handle->resource)))) return 0;

		// create resource heap
		D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
        heapDesc.NumDescriptors = 1;
        heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_DSV;
        heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
        if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->resourceHeap)))) return 0;
		handle->resourceHeapHandle = handle->resourceHeap->GetGPUDescriptorHandleForHeapStart();

		// create resource view
		D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
		srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
		srvDesc.Format = handle->format;
		srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;
		srvDesc.Texture2D.MipLevels = 1;
		handle->device->device->CreateShaderResourceView(handle->resource, &srvDesc, handle->resourceHeap->GetCPUDescriptorHandleForHeapStart());

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_DepthStencil_Dispose(DepthStencil* handle)
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

void Orbital_Video_D3D12_DepthStencil_ChangeState(DepthStencil* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList)
{
	if (handle->resourceState == state) return;
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