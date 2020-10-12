#include "PipelineStateResources.h"

int Orbital_Video_D3D12_PipelineStateResources_Init(PipelineStateResources* handle, Device* device, PipelineStateResourcesDesc* desc)
{
	handle->device = device;

	// copy constant references
	if (desc->constantBufferCount != 0)
	{
		handle->constantBufferCount = desc->constantBufferCount;
		UINT size = sizeof(ConstantBuffer*) * handle->constantBufferCount;
		handle->constantBuffers = (ConstantBuffer**)malloc(size);
		memcpy(handle->constantBuffers, desc->constantBuffers, size);
	}

	// copy texture & depth-stencil references
	if (desc->textureCount != 0 || desc->textureDepthStencilCount != 0)
	{
		handle->textureCount = desc->textureCount;
		UINT size = sizeof(Texture*) * desc->textureCount;
		handle->textures = (Texture**)malloc(size);
		memcpy(handle->textures, desc->textures, size);

		handle->textureDepthStencilCount = desc->textureDepthStencilCount;
		size = sizeof(DepthStencil*) * desc->textureDepthStencilCount;
		handle->textureDepthStencils = (DepthStencil**)malloc(size);
		memcpy(handle->textureDepthStencils, desc->textureDepthStencils, size);
	}

	// copy random-access references
	if (desc->randomAccessBufferCount != 0)
	{
		handle->randomAccessBufferCount = desc->randomAccessBufferCount;
		UINT size = sizeof(intptr_t) * desc->randomAccessBufferCount;
		handle->randomAccessBuffers = (intptr_t*)malloc(size);
		memcpy(handle->randomAccessBuffers, desc->randomAccessBuffers, size);

		size = desc->randomAccessBufferCount * sizeof(RandomAccessBufferType);
		handle->randomAccessTypes = (RandomAccessBufferType*)malloc(size);
		memcpy(handle->randomAccessTypes, desc->randomAccessTypes, size);
	}

	// create GPU heaps
	handle->nodes = (PipelineStateResourcesNode*)calloc(handle->device->nodeCount, sizeof(PipelineStateResourcesNode));
	for (UINT n = 0; n != handle->device->nodeCount; ++n)
	{
		// add constant buffer heaps
		if (desc->constantBufferCount != 0)
		{
			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NodeMask = handle->device->nodes[n].mask;
			heapDesc.NumDescriptors = desc->constantBufferCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->nodes[n].constantBufferHeap)))) return 0;
			handle->nodes[n].constantBufferGPUDescHandle = handle->nodes[n].constantBufferHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuComputerBufferHeap = handle->nodes[n].constantBufferHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			for (int i = 0; i != desc->constantBufferCount; ++i)
			{
				ConstantBuffer* constantBuffer = (ConstantBuffer*)desc->constantBuffers[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = constantBuffer->nodes[n].resourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuComputerBufferHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuComputerBufferHeap.ptr += heapSize;
			}
		}

		// add texture heaps
		if (desc->textureCount != 0 || desc->textureDepthStencilCount != 0)
		{
			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NodeMask = handle->device->nodes[n].mask;
			heapDesc.NumDescriptors = handle->textureCount + handle->textureDepthStencilCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->nodes[n].textureHeap)))) return 0;
			handle->nodes[n].textureGPUDescHandle = handle->nodes[n].textureHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuTextureHeap = handle->nodes[n].textureHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

			for (int i = 0; i != desc->textureCount; ++i)
			{
				Texture* texture = (Texture*)desc->textures[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = texture->nodes[n].shaderResourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuTextureHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuTextureHeap.ptr += heapSize;
			}

			for (int i = 0; i != desc->textureDepthStencilCount; ++i)
			{
				DepthStencil* depthStencil = (DepthStencil*)desc->textureDepthStencils[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = depthStencil->nodes[n].shaderResourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuTextureHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuTextureHeap.ptr += heapSize;
			}
		}

		// add random-access buffer heaps
		if (desc->randomAccessBufferCount != 0)
		{
			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NodeMask = handle->device->nodes[n].mask;
			heapDesc.NumDescriptors = handle->randomAccessBufferCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->nodes[n].randomAccessBufferHeap)))) return 0;
			handle->nodes[n].randomAccessBufferGPUDescHandle = handle->nodes[n].randomAccessBufferHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuRandomAccessBufferHeap = handle->nodes[n].randomAccessBufferHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

			for (int i = 0; i != desc->randomAccessBufferCount; ++i)
			{
				if (desc->randomAccessTypes[i] == RandomAccessBufferType::RandomAccessBufferType_Texture)
				{
					Texture* texture = (Texture*)desc->randomAccessBuffers[i];
					D3D12_CPU_DESCRIPTOR_HANDLE heap = texture->nodes[n].randomAccessResourceHeap->GetCPUDescriptorHandleForHeapStart();
					handle->device->device->CopyDescriptorsSimple(1, cpuRandomAccessBufferHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
					cpuRandomAccessBufferHeap.ptr += heapSize;
				}
				else
				{
					return 0;
				}
			}
		}
	}

	return 1;
}

void Orbital_Video_D3D12_PipelineStateResources_Dispose(PipelineStateResources* handle)
{
	if (handle->constantBuffers != NULL)
	{
		free(handle->constantBuffers);
		handle->constantBuffers = NULL;
	}

	if (handle->textures != NULL)
	{
		free(handle->textures);
		handle->textures = NULL;
	}

	if (handle->randomAccessBuffers != NULL)
	{
		free(handle->randomAccessBuffers);
		handle->randomAccessBuffers = NULL;
	}

	if (handle->randomAccessTypes != NULL)
	{
		free(handle->randomAccessTypes);
		handle->randomAccessTypes = NULL;
	}

	if (handle->nodes != NULL)
	{
		for (UINT n = 0; n != handle->device->nodeCount; ++n)
		{
			if (handle->nodes[n].constantBufferHeap != NULL)
			{
				handle->nodes[n].constantBufferHeap->Release();
				handle->nodes[n].constantBufferHeap = NULL;
			}

			if (handle->nodes[n].textureHeap != NULL)
			{
				handle->nodes[n].textureHeap->Release();
				handle->nodes[n].textureHeap = NULL;
			}

			if (handle->nodes[n].randomAccessBufferHeap != NULL)
			{
				handle->nodes[n].randomAccessBufferHeap->Release();
				handle->nodes[n].randomAccessBufferHeap = NULL;
			}
		}

		free(handle->nodes);
		handle->nodes = NULL;
	}
}