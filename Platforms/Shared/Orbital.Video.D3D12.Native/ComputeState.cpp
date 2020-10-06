#include "ComputeState.h"
#include "RenderPass.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT ComputeState* Orbital_Video_D3D12_ComputeState_Create(Device* device)
	{
		ComputeState* handle = (ComputeState*)calloc(1, sizeof(ComputeState));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ComputeState_Init(ComputeState* handle, ComputeStateDesc* desc)
	{
		D3D12_COMPUTE_PIPELINE_STATE_DESC pipelineDesc = {};

		// shaders
		ComputeShader* computeShader = (ComputeShader*)desc->computeShader;
		handle->computeShader = computeShader;
		pipelineDesc.CS = computeShader->bytecode;
		pipelineDesc.pRootSignature = computeShader->signature.signature;

		// add constant buffer heaps
		if (desc->constantBufferCount != 0)
		{
			handle->constantBufferCount = desc->constantBufferCount;
			UINT size = sizeof(ConstantBuffer*) * handle->constantBufferCount;
			handle->constantBuffers = (ConstantBuffer**)malloc(size);
			memcpy(handle->constantBuffers, desc->constantBuffers, size);

			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NumDescriptors = desc->constantBufferCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->constantBufferHeap)))) return 0;
			handle->constantBufferGPUDescHandle = handle->constantBufferHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuComputerBufferHeap = handle->constantBufferHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			for (int i = 0; i != desc->constantBufferCount; ++i)
			{
				ConstantBuffer* constantBuffer = (ConstantBuffer*)desc->constantBuffers[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = constantBuffer->resourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuComputerBufferHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuComputerBufferHeap.ptr += heapSize;
			}
		}

		// add texture heaps
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

			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NumDescriptors = handle->textureCount + handle->textureDepthStencilCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->textureHeap)))) return 0;
			handle->textureGPUDescHandle = handle->textureHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuTextureHeap = handle->textureHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

			for (int i = 0; i != desc->textureCount; ++i)
			{
				Texture* texture = (Texture*)desc->textures[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = texture->shaderResourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuTextureHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuTextureHeap.ptr += heapSize;
			}

			for (int i = 0; i != desc->textureDepthStencilCount; ++i)
			{
				DepthStencil* depthStencil = (DepthStencil*)desc->textureDepthStencils[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = depthStencil->shaderResourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuTextureHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuTextureHeap.ptr += heapSize;
			}
		}

		// add random-access buffer heaps
		if (desc->randomAccessBufferCount != 0)
		{
			handle->randomAccessBufferCount = desc->randomAccessBufferCount;
			UINT size = sizeof(intptr_t) * desc->randomAccessBufferCount;
			handle->randomAccessBuffers = (intptr_t*)malloc(size);
			memcpy(handle->randomAccessBuffers, desc->randomAccessBuffers, size);

			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NumDescriptors = handle->randomAccessBufferCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->randomAccessBufferHeap)))) return 0;
			handle->randomAccessBufferGPUDescHandle = handle->randomAccessBufferHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuRandomAccessBufferHeap = handle->randomAccessBufferHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

			for (int i = 0; i != desc->randomAccessBufferCount; ++i)
			{
				if (desc->randomAccessTypes[i] == RandomAccessBufferType::RandomAccessBufferType_Texture)
				{
					Texture* texture = (Texture*)desc->randomAccessBuffers[i];
					D3D12_CPU_DESCRIPTOR_HANDLE heap = texture->randomAccessResourceHeap->GetCPUDescriptorHandleForHeapStart();
					handle->device->device->CopyDescriptorsSimple(1, cpuRandomAccessBufferHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
					cpuRandomAccessBufferHeap.ptr += heapSize;
				}
				else
				{
					return 0;
				}
			}

			size_t randomAccessBufferSize = desc->randomAccessBufferCount * sizeof(RandomAccessBufferType);
			handle->randomAccessTypes = (RandomAccessBufferType*)malloc(randomAccessBufferSize);
			memcpy(handle->randomAccessTypes, desc->randomAccessTypes, randomAccessBufferSize);
		}

		// create pipeline state
		if (FAILED(handle->device->device->CreateComputePipelineState(&pipelineDesc, IID_PPV_ARGS(&handle->state)))) return 0;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ComputeState_Dispose(ComputeState* handle)
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

		if (handle->constantBufferHeap != NULL)
		{
			handle->constantBufferHeap->Release();
			handle->constantBufferHeap = NULL;
		}

		if (handle->textureHeap != NULL)
		{
			handle->textureHeap->Release();
			handle->textureHeap = NULL;
		}

		if (handle->randomAccessBufferHeap != NULL)
		{
			handle->randomAccessBufferHeap->Release();
			handle->randomAccessBufferHeap = NULL;
		}

		if (handle->state != NULL)
		{
			handle->state->Release();
			handle->state = NULL;
		}

		free(handle);
	}
}