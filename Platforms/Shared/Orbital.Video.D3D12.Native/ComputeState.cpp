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

		// add read/write buffer heaps
		if (desc->readWriteBufferCount != 0)
		{
			handle->readWriteBufferCount = desc->readWriteBufferCount;
			UINT size = sizeof(intptr_t) * desc->readWriteBufferCount;
			handle->readWriteBuffers = (intptr_t*)malloc(size);
			memcpy(handle->readWriteBuffers, desc->readWriteBuffers, size);

			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NumDescriptors = handle->readWriteBufferCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->readWriteBufferHeap)))) return 0;
			handle->readWriteBufferGPUDescHandle = handle->readWriteBufferHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuReadWriteBufferHeap = handle->readWriteBufferHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

			for (int i = 0; i != desc->readWriteBufferCount; ++i)
			{
				if (desc->readWriteTypes[i] == ReadWriteBufferType::ReadWriteBufferType_Texture)
				{
					Texture* texture = (Texture*)desc->readWriteBuffers[i];
					D3D12_CPU_DESCRIPTOR_HANDLE heap = texture->readWriteResourceHeap->GetCPUDescriptorHandleForHeapStart();
					handle->device->device->CopyDescriptorsSimple(1, cpuReadWriteBufferHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
					cpuReadWriteBufferHeap.ptr += heapSize;
				}
				else
				{
					return 0;
				}
			}

			size_t readWriteBufferSize = desc->readWriteBufferCount * sizeof(ReadWriteBufferType);
			handle->readWriteTypes = (ReadWriteBufferType*)malloc(readWriteBufferSize);
			memcpy(handle->readWriteTypes, desc->readWriteTypes, readWriteBufferSize);
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

		if (handle->readWriteBuffers != NULL)
		{
			free(handle->readWriteBuffers);
			handle->readWriteBuffers = NULL;
		}

		if (handle->readWriteTypes != NULL)
		{
			free(handle->readWriteTypes);
			handle->readWriteTypes = NULL;
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

		if (handle->readWriteBufferHeap != NULL)
		{
			handle->readWriteBufferHeap->Release();
			handle->readWriteBufferHeap = NULL;
		}

		if (handle->state != NULL)
		{
			handle->state->Release();
			handle->state = NULL;
		}

		free(handle);
	}
}