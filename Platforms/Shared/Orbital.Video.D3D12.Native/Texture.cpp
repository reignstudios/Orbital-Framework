#include "Texture.h"
#include "Utils.h"

UINT TextureFormatSizePerPixel(DXGI_FORMAT nativeFormat)
{
	switch (nativeFormat)
	{
		case DXGI_FORMAT::DXGI_FORMAT_R8G8B8A8_UINT:
		case DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UINT:
		case DXGI_FORMAT::DXGI_FORMAT_R8G8B8A8_UNORM:
		case DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UNORM:
			return 4;

		case DXGI_FORMAT::DXGI_FORMAT_R16G16B16A16_FLOAT: return 8;
		case DXGI_FORMAT::DXGI_FORMAT_R32G32B32A32_FLOAT: return 16;
		default: return 0;
	}
	return true;
}

extern "C"
{
	ORBITAL_EXPORT Texture* Orbital_Video_D3D12_Texture_Create(Device* device, TextureMode mode)
	{
		Texture* handle = (Texture*)calloc(1, sizeof(Texture));
		handle->device = device;
		handle->mode = mode;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Texture_Init(Texture* handle, TextureFormat format, TextureType type, UINT32 mipLevels, UINT32* width, UINT32* height, UINT32* depth, BYTE** data, int isRenderTexture, int allowRandomAccess, MSAALevel msaaLevel, MultiGPUNodeResourceVisibility nodeVisibility)
	{
		if (!GetNative_TextureFormat(format, &handle->format, isRenderTexture)) return 0;

		// create nodes
		handle->nodes = (TextureNode*)calloc(handle->device->nodeCount, sizeof(TextureNode));
		for (UINT n = 0; n != handle->device->nodeCount; ++n)
		{
			// create resource
			D3D12_HEAP_PROPERTIES heapProperties = {};
			if (handle->mode == TextureMode_GPUOptimized) heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
			else return 0;
			heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
			heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
			heapProperties.CreationNodeMask = handle->device->nodes[n].mask;
			heapProperties.VisibleNodeMask = (nodeVisibility == MultiGPUNodeResourceVisibility_Self) ? handle->device->nodes[n].mask : handle->device->fullNodeMask;

			D3D12_RESOURCE_DESC resourceDesc = {};
			if (type == TextureType::TextureType_1D) resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE1D;
			else if (type == TextureType::TextureType_2D) resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
			else if (type == TextureType::TextureType_3D) resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE3D;
			else if (type == TextureType::TextureType_Cube) resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
			else return 0;
			resourceDesc.Alignment = 0;
			resourceDesc.Width = *width;
			resourceDesc.Height = *height;
			resourceDesc.DepthOrArraySize = *depth;
			resourceDesc.MipLevels = mipLevels;
			resourceDesc.Format = handle->format;
			resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
			resourceDesc.Flags = isRenderTexture ? D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET : D3D12_RESOURCE_FLAG_NONE;
			if (allowRandomAccess) resourceDesc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;

			if (msaaLevel != MSAALevel::MSAALevel_Disabled)// get msaa quality levels
			{
				resourceDesc.SampleDesc.Count = (UINT)msaaLevel;
				resourceDesc.SampleDesc.Quality = DXGI_STANDARD_MULTISAMPLE_QUALITY_PATTERN;
			}
			else
			{
				resourceDesc.SampleDesc.Count = 1;
				resourceDesc.SampleDesc.Quality = 0;
			}

			handle->msaaSampleDesc = resourceDesc.SampleDesc;
			handle->nodes[n].resourceState = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			if (data != NULL && handle->mode == TextureMode_GPUOptimized) handle->nodes[n].resourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for gpu copy
			if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, handle->nodes[n].resourceState, NULL, IID_PPV_ARGS(&handle->nodes[n].resource)))) return 0;
			
			// create shader resource heap
			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NodeMask = handle->device->nodes[n].mask;
			heapDesc.NumDescriptors = 1;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->nodes[n].shaderResourceHeap)))) return 0;

			// create shader resource view
			D3D12_SHADER_RESOURCE_VIEW_DESC shaderResourceDesc = {};
			shaderResourceDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
			shaderResourceDesc.Format = handle->format;
			if (type == TextureType::TextureType_1D) shaderResourceDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE1D;
			else if (type == TextureType::TextureType_2D) shaderResourceDesc.ViewDimension = (msaaLevel == MSAALevel_Disabled) ? D3D12_SRV_DIMENSION_TEXTURE2D : D3D12_SRV_DIMENSION_TEXTURE2DMS;
			else if (type == TextureType::TextureType_3D) shaderResourceDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE3D;
			else if (type == TextureType::TextureType_Cube) shaderResourceDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURECUBE;
			else return 0;
			shaderResourceDesc.Texture2D.MipLevels = mipLevels;
			handle->device->device->CreateShaderResourceView(handle->nodes[n].resource, &shaderResourceDesc, handle->nodes[n].shaderResourceHeap->GetCPUDescriptorHandleForHeapStart());
		
			// create render-target resource view
			if (isRenderTexture)
			{
				D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
				heapDesc.NodeMask = handle->device->nodes[n].mask;
				heapDesc.NumDescriptors = 1;
				heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
				heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
				if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->nodes[n].renderTargetResourceHeap)))) return 0;

				D3D12_RENDER_TARGET_VIEW_DESC renderTargetDesc = {};
				renderTargetDesc.Format = handle->format;
				if (type == TextureType::TextureType_1D) renderTargetDesc.ViewDimension = D3D12_RTV_DIMENSION_TEXTURE1D;
				else if (type == TextureType::TextureType_2D) renderTargetDesc.ViewDimension = (msaaLevel == MSAALevel_Disabled) ? D3D12_RTV_DIMENSION_TEXTURE2D : D3D12_RTV_DIMENSION_TEXTURE2DMS;
				else if (type == TextureType::TextureType_3D) renderTargetDesc.ViewDimension = D3D12_RTV_DIMENSION_TEXTURE3D;
				else return 0;
				handle->nodes[n].renderTargetResourceDescCPUHandle = handle->nodes[n].renderTargetResourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CreateRenderTargetView(handle->nodes[n].resource, &renderTargetDesc, handle->nodes[n].renderTargetResourceDescCPUHandle);
			}

			if (allowRandomAccess)
			{
				D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
				heapDesc.NodeMask = handle->device->nodes[n].mask;
				heapDesc.NumDescriptors = 1;
				heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
				heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
				if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->nodes[n].randomAccessResourceHeap)))) return 0;

				D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
				uavDesc.Format = handle->format;
				if (type == TextureType::TextureType_1D) uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE1D;
				else if (type == TextureType::TextureType_2D) uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE2D;
				else if (type == TextureType::TextureType_3D) uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE3D;
				else return 0;
				D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = handle->nodes[n].randomAccessResourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CreateUnorderedAccessView(handle->nodes[n].resource, nullptr, &uavDesc, cpuHandle);
			}

			// upload initial data
			if (data != NULL)
			{
				// allocate gpu upload buffer if needed
				bool useUploadBuffer = false;
				ID3D12Resource* uploadResource = handle->nodes[n].resource;
				if (heapProperties.Type != D3D12_HEAP_TYPE_UPLOAD)
				{
					useUploadBuffer = true;
					uploadResource = NULL;
					heapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;

					UINT64 uploadBufferSize;
					handle->device->device->GetCopyableFootprints(&resourceDesc, 0, mipLevels, 0, nullptr, nullptr, nullptr, &uploadBufferSize);

					ZeroMemory(&resourceDesc, sizeof(D3D12_RESOURCE_DESC));
					resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
					resourceDesc.Alignment = 0;
					resourceDesc.Width = uploadBufferSize;
					resourceDesc.Height = 1;
					resourceDesc.DepthOrArraySize = 1;
					resourceDesc.MipLevels = 1;
					resourceDesc.Format = DXGI_FORMAT_UNKNOWN;
					resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
					resourceDesc.Flags = D3D12_RESOURCE_FLAG_NONE;
					resourceDesc.SampleDesc.Count = 1;
					resourceDesc.SampleDesc.Quality = 0;

					if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, D3D12_RESOURCE_STATE_GENERIC_READ, NULL, IID_PPV_ARGS(&uploadResource)))) return 0;
				}

				// copy CPU memory to GPU
				UINT8* gpuDataPtr;
				D3D12_RANGE readRange = {};
				for (UINT i = 0; i != mipLevels; ++i)
				{
					if (FAILED(uploadResource->Map(i, &readRange, reinterpret_cast<void**>(&gpuDataPtr))))
					{
						if (useUploadBuffer) uploadResource->Release();
						return 0;
					}
			
					UINT srcPitch = width[i] * TextureFormatSizePerPixel(handle->format);
					const UINT32 alignment = D3D12_TEXTURE_DATA_PITCH_ALIGNMENT - 1;
					UINT dstPitch = (srcPitch + alignment) & ~alignment;// row size is required to be aligned
					for (UINT y = 0; y != height[i]; ++y)
					{
						memcpy(gpuDataPtr + (dstPitch * y), data[i] + (srcPitch * y), srcPitch);// copy texture row
					}

					uploadResource->Unmap(i, nullptr);
				}

				// copy upload buffer to default buffer
				if (useUploadBuffer)
				{
					// reset command list and copy resource
					handle->device->nodes[n].internalMutex->lock();
					handle->device->nodes[n].internalCommandAllocator->Reset();
					handle->device->nodes[n].internalCommandList->Reset(handle->device->nodes[n].internalCommandAllocator, NULL);

					// copy all mip levels
					for (UINT i = 0; i != mipLevels; ++i)
					{
						D3D12_TEXTURE_COPY_LOCATION dstLoc = {};
						D3D12_TEXTURE_COPY_LOCATION srcLoc = {};
						dstLoc.pResource = handle->nodes[n].resource;
						srcLoc.pResource = uploadResource;
						dstLoc.SubresourceIndex = i;
						srcLoc.SubresourceIndex = i;
						dstLoc.Type = D3D12_TEXTURE_COPY_TYPE_SUBRESOURCE_INDEX;
						srcLoc.Type = D3D12_TEXTURE_COPY_TYPE_PLACED_FOOTPRINT;

						srcLoc.PlacedFootprint.Footprint.Width = width[i];
						srcLoc.PlacedFootprint.Footprint.Height = height[i];
						srcLoc.PlacedFootprint.Footprint.Depth = depth[i];
						srcLoc.PlacedFootprint.Footprint.Format = handle->format;
						UINT32 size = width[i] * TextureFormatSizePerPixel(handle->format);
						const UINT32 alignment = D3D12_TEXTURE_DATA_PITCH_ALIGNMENT - 1;
						srcLoc.PlacedFootprint.Footprint.RowPitch = (size + alignment) & ~alignment;// row size is required to be aligned

						handle->device->nodes[n].internalCommandList->CopyTextureRegion(&dstLoc, 0, 0, 0, &srcLoc, nullptr);
					}

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

	ORBITAL_EXPORT void Orbital_Video_D3D12_Texture_Dispose(Texture* handle)
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

				if (handle->nodes[n].renderTargetResourceHeap != NULL)
				{
					handle->nodes[n].renderTargetResourceHeap->Release();
					handle->nodes[n].renderTargetResourceHeap = NULL;
				}

				if (handle->nodes[n].randomAccessResourceHeap != NULL)
				{
					handle->nodes[n].randomAccessResourceHeap->Release();
					handle->nodes[n].randomAccessResourceHeap = NULL;
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

void Orbital_Video_D3D12_Texture_ChangeState(Texture* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList)
{
	TextureNode* activeNode = &handle->nodes[nodeIndex];
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