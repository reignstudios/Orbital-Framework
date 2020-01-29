#include "Texture.h"

extern "C"
{
	bool TextureFormatToNative(TextureFormat format, DXGI_FORMAT* nativeFormat)
	{
		switch (format)
		{
			case TextureFormat::TextureFormat_Default:
			case TextureFormat::TextureFormat_B8G8R8A8:
				*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_B8G8R8A8_UNORM;
				break;

			case TextureFormat::TextureFormat_DefaultHDR:
			case TextureFormat::TextureFormat_R10G10B10A2:
				*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UNORM;
				break;
			default: return false;
		}
		return true;
	}

	UINT TextureFormatSizePerPixel(DXGI_FORMAT nativeFormat)
	{
		switch (nativeFormat)
		{
			case DXGI_FORMAT::DXGI_FORMAT_B8G8R8A8_UNORM:
			case DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UNORM:
				return 4;

			default: return 0;
		}
		return true;
	}

	ORBITAL_EXPORT Texture* Orbital_Video_D3D12_Texture_Create(Device* device, TextureMode mode)
	{
		Texture* handle = (Texture*)calloc(1, sizeof(Texture));
		handle->device = device;
		handle->mode = mode;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Texture_Init(Texture* handle, TextureFormat format, TextureType type, UINT32 mipLevels, UINT32* width, UINT32* height, UINT32* depth, BYTE** data)
	{
		if (!TextureFormatToNative(format, &handle->format)) return 0;

		// create resource
		D3D12_HEAP_PROPERTIES heapProperties = {};
		if (handle->mode == TextureMode_GPUOptimized) heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
		else return 0;
        heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
        heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
        heapProperties.CreationNodeMask = 1;// TODO: multi-gpu setup
        heapProperties.VisibleNodeMask = 1;

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
        resourceDesc.SampleDesc.Count = 1;
        resourceDesc.SampleDesc.Quality = 0;
        resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
        resourceDesc.Flags = D3D12_RESOURCE_FLAG_NONE;

		handle->resourceState = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
		if (data != NULL && handle->mode == TextureMode_GPUOptimized) handle->resourceState = D3D12_RESOURCE_STATE_COPY_DEST;// init for gpu copy
		if (FAILED(handle->device->device->CreateCommittedResource(&heapProperties, D3D12_HEAP_FLAG_NONE, &resourceDesc, handle->resourceState, NULL, IID_PPV_ARGS(&handle->texture)))) return 0;

		// create resource heap
		D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
        heapDesc.NumDescriptors = 1;
        heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
        heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// set to none so it can be copied in RenderState
        if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->textureHeap)))) return 0;
		handle->textureHeapHandle = handle->textureHeap->GetGPUDescriptorHandleForHeapStart();

		// create resource view
		D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
		srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
		srvDesc.Format = handle->format;
		if (type == TextureType::TextureType_1D) srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE1D;
		else if (type == TextureType::TextureType_2D) srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;
		else if (type == TextureType::TextureType_3D) srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE3D;
		else if (type == TextureType::TextureType_Cube) srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURECUBE;
		else return 0;
		srvDesc.Texture2D.MipLevels = mipLevels;
		handle->device->device->CreateShaderResourceView(handle->texture, &srvDesc, handle->textureHeap->GetCPUDescriptorHandleForHeapStart());

		// upload initial data
		if (data != NULL)
		{
			// allocate gpu upload buffer if needed
			bool useUploadBuffer = false;
			ID3D12Resource* uploadResource = handle->texture;
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
				resourceDesc.SampleDesc.Count = 1;
				resourceDesc.SampleDesc.Quality = 0;
				resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
				resourceDesc.Flags = D3D12_RESOURCE_FLAG_NONE;

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
				handle->device->internalMutex->lock();
				// reset command list and copy resource
				handle->device->internalCommandList->Reset(handle->device->commandAllocator, NULL);

				// copy all mip levels
				for (UINT i = 0; i != mipLevels; ++i)
				{
					D3D12_TEXTURE_COPY_LOCATION dstLoc = {};
					D3D12_TEXTURE_COPY_LOCATION srcLoc = {};
					dstLoc.pResource = handle->texture;
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

					handle->device->internalCommandList->CopyTextureRegion(&dstLoc, 0, 0, 0, &srcLoc, nullptr);
				}

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

	ORBITAL_EXPORT void Orbital_Video_D3D12_Texture_Dispose(Texture* handle)
	{
		if (handle->textureHeap != NULL)
		{
			handle->textureHeap->Release();
			handle->textureHeap = NULL;
		}

		if (handle->texture != NULL)
		{
			handle->texture->Release();
			handle->texture = NULL;
		}

		free(handle);
	}
}

void Orbital_Video_D3D12_Texture_ChangeState(Texture* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList)
{
	if (handle->resourceState == state) return;
	D3D12_RESOURCE_BARRIER barrier = {};
	barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
	barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
	barrier.Transition.pResource = handle->texture;
	barrier.Transition.StateBefore = handle->resourceState;
	barrier.Transition.StateAfter = state;
	barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
	commandList->ResourceBarrier(1, &barrier);
	handle->resourceState = state;
}