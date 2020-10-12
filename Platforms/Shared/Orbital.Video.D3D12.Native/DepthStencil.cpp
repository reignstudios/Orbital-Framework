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

	ORBITAL_EXPORT int Orbital_Video_D3D12_DepthStencil_Init(DepthStencil* handle, DepthStencilFormat format, UINT32 width, UINT32 height, MSAALevel msaaLevel)
	{
		if (!GetNative_DepthStencilFormat(format, &handle->format)) return 0;

		// create nodes
		handle->nodes = (DepthStencilNode*)calloc(handle->device->nodeCount, sizeof(DepthStencilNode));
		for (UINT n = 0; n != handle->device->nodeCount; ++n)
		{
			// create resource
			D3D12_HEAP_PROPERTIES heapProperties = {};
			if (handle->mode == DepthStencilMode_GPUOptimized) heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
			else return 0;
			heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
			heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
			heapProperties.CreationNodeMask = handle->device->nodes[n].mask;
			heapProperties.VisibleNodeMask = handle->device->nodes[n].mask;

			D3D12_RESOURCE_DESC resourceDesc = {};
			resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
			resourceDesc.Alignment = 0;
			resourceDesc.Width = width;
			resourceDesc.Height = height;
			resourceDesc.DepthOrArraySize = 1;
			resourceDesc.MipLevels = 1;
			resourceDesc.Format = handle->format;
			resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
			resourceDesc.Flags = D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;

			if (msaaLevel != MSAALevel::MSAALevel_Disabled)
			{
				resourceDesc.SampleDesc.Count = (UINT)msaaLevel;
				resourceDesc.SampleDesc.Quality = DXGI_STANDARD_MULTISAMPLE_QUALITY_PATTERN;
			}
			else
			{
				resourceDesc.SampleDesc.Count = 1;
				resourceDesc.SampleDesc.Quality = 0;
			}

			handle->nodes[n].resourceState = D3D12_RESOURCE_STATE_DEPTH_READ;
			if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, handle->nodes[n].resourceState, NULL, IID_PPV_ARGS(&handle->nodes[n].resource)))) return 0;

			// create depth-stencil resource heap
			D3D12_DESCRIPTOR_HEAP_DESC depthStencilHeapDesc = {};
			depthStencilHeapDesc.NodeMask = handle->device->nodes[n].mask;
			depthStencilHeapDesc.NumDescriptors = 1;
			depthStencilHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_DSV;
			depthStencilHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
			if (FAILED(handle->device->device->CreateDescriptorHeap(&depthStencilHeapDesc, IID_PPV_ARGS(&handle->nodes[n].depthStencilResourceHeap)))) return 0;
			handle->nodes[n].depthStencilResourceCPUHeapHandle = handle->nodes[n].depthStencilResourceHeap->GetCPUDescriptorHandleForHeapStart();

			// create depth-stencil view
			D3D12_DEPTH_STENCIL_VIEW_DESC dsDesc = {};
			dsDesc.Format = handle->format;
			dsDesc.ViewDimension = (msaaLevel == MSAALevel_Disabled) ? D3D12_DSV_DIMENSION_TEXTURE2D : D3D12_DSV_DIMENSION_TEXTURE2DMS;
			dsDesc.Texture2D.MipSlice = 0;
			handle->device->device->CreateDepthStencilView(handle->nodes[n].resource, &dsDesc, handle->nodes[n].depthStencilResourceCPUHeapHandle);

			// create shader resource heap
			D3D12_DESCRIPTOR_HEAP_DESC shaderHeapDesc = {};
			shaderHeapDesc.NodeMask = handle->device->nodes[n].mask;
			shaderHeapDesc.NumDescriptors = 1;
			shaderHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			shaderHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
			if (FAILED(handle->device->device->CreateDescriptorHeap(&shaderHeapDesc, IID_PPV_ARGS(&handle->nodes[n].shaderResourceHeap)))) return 0;

			// create shader resource view
			D3D12_SHADER_RESOURCE_VIEW_DESC shaderResourceDesc = {};
			shaderResourceDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
			if (!GetNative_DepthStencilShaderResourceFormat(format, &shaderResourceDesc.Format)) return 0;
			shaderResourceDesc.ViewDimension = (msaaLevel == MSAALevel_Disabled) ? D3D12_SRV_DIMENSION_TEXTURE2D : D3D12_SRV_DIMENSION_TEXTURE2DMS;
			shaderResourceDesc.Texture2D.MipLevels = 1;
			handle->device->device->CreateShaderResourceView(handle->nodes[n].resource, &shaderResourceDesc, handle->nodes[n].shaderResourceHeap->GetCPUDescriptorHandleForHeapStart());
		}

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_DepthStencil_Dispose(DepthStencil* handle)
	{
		if (handle->nodes != NULL)
		{
			for (UINT n = 0; n != handle->device->nodeCount; ++n)
			{
				if (handle->nodes[n].shaderResourceHeap != NULL)
				{
					handle->nodes[n].shaderResourceHeap->Release();
					handle->nodes[n].shaderResourceHeap = NULL;
				}

				if (handle->nodes[n].depthStencilResourceHeap != NULL)
				{
					handle->nodes[n].depthStencilResourceHeap->Release();
					handle->nodes[n].depthStencilResourceHeap = NULL;
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
}

void Orbital_Video_D3D12_DepthStencil_ChangeState(DepthStencil* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList)
{
	DepthStencilNode* activeNode = &handle->nodes[nodeIndex];
	if (activeNode->resourceState == state) return;
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