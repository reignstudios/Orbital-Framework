#include "VertexBufferStreamer.h"
#include "VertexBuffer.h"

extern "C"
{
	ORBITAL_EXPORT VertexBufferStreamer* Orbital_Video_D3D12_VertexBufferStreamer_Create()
	{
		return (VertexBufferStreamer*)calloc(1, sizeof(VertexBufferStreamer));
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_VertexBufferStreamer_Init(VertexBufferStreamer* handle, VertexBufferStreamLayout* layout)
	{
		// track vertex buffer resources
		handle->vertexBufferCount = layout->descCount;
		handle->vertexBuffers = (VertexBuffer**)calloc(layout->descCount, sizeof(VertexBuffer*));
		handle->vertexBufferViews = (D3D12_VERTEX_BUFFER_VIEW*)calloc(layout->descCount, sizeof(D3D12_VERTEX_BUFFER_VIEW));
		for (int i = 0; i != layout->descCount; ++i)
		{
			handle->vertexBuffers[i] = (VertexBuffer*)layout->descs[i].vertexBuffer;
			handle->vertexBufferViews[i] = handle->vertexBuffers[i]->vertexBufferView;
		}

		// create elements
		handle->elementCount = layout->elementCount;
		handle->elements = (D3D12_INPUT_ELEMENT_DESC*)calloc(layout->elementCount, sizeof(D3D12_INPUT_ELEMENT_DESC));
		for (int i = 0; i != layout->elementCount; ++i)
		{
			VertexBufferStreamElement element = layout->elements[i];
			VertexBufferStreamDesc vertexBufferDesc = layout->descs[element.index];
			VertexBuffer* vertexBuffer = (VertexBuffer*)vertexBufferDesc.vertexBuffer;
			D3D12_INPUT_ELEMENT_DESC elementDesc = {};

			// set stream input specifics
			elementDesc.InputSlot = element.index;
			if (vertexBufferDesc.type == VertexBufferStreamType::VertexBufferStreamType_VertexData)
			{
				elementDesc.InputSlotClass = D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA;
				elementDesc.InstanceDataStepRate = 0;
			}
			else if (vertexBufferDesc.type == VertexBufferStreamType::VertexBufferStreamType_InstanceData)
			{
				elementDesc.InputSlotClass = D3D12_INPUT_CLASSIFICATION_PER_INSTANCE_DATA;
				elementDesc.InstanceDataStepRate = 1;
			}
			else
			{
				return 0;
			}

			// set data type
			switch (element.type)
			{
				case VertexBufferStreamElementType::VertexBufferStreamElementType_Float: elementDesc.Format = DXGI_FORMAT_R32_FLOAT; break;
				case VertexBufferStreamElementType::VertexBufferStreamElementType_Float2: elementDesc.Format = DXGI_FORMAT_R32G32_FLOAT; break;
				case VertexBufferStreamElementType::VertexBufferStreamElementType_Float3: elementDesc.Format = DXGI_FORMAT_R32G32B32_FLOAT; break;
				case VertexBufferStreamElementType::VertexBufferStreamElementType_Float4: elementDesc.Format = DXGI_FORMAT_R32G32B32A32_FLOAT; break;
				case VertexBufferStreamElementType::VertexBufferStreamElementType_RGBAx8: elementDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM; break;
				default: return 0;
			}

			// set shader usage
			elementDesc.SemanticIndex = element.usageIndex;
			switch (element.usage)
			{
				case VertexBufferStreamElementUsage::VertexBufferStreamElementUsage_Position: elementDesc.SemanticName = "POSITION"; break;
				case VertexBufferStreamElementUsage::VertexBufferStreamElementUsage_Color: elementDesc.SemanticName = "COLOR"; break;
				case VertexBufferStreamElementUsage::VertexBufferStreamElementUsage_UV: elementDesc.SemanticName = "TEXCOORD"; break;
				case VertexBufferStreamElementUsage::VertexBufferStreamElementUsage_Normal: elementDesc.SemanticName = "NORMAL"; break;
				case VertexBufferStreamElementUsage::VertexBufferStreamElementUsage_Tangent: elementDesc.SemanticName = "TANGENT"; break;
				case VertexBufferStreamElementUsage::VertexBufferStreamElementUsage_Binormal: elementDesc.SemanticName = "BINORMAL"; break;
				case VertexBufferStreamElementUsage::VertexBufferStreamElementUsage_Index: elementDesc.SemanticName = "BLENDINDICES"; break;
				case VertexBufferStreamElementUsage::VertexBufferStreamElementUsage_Weight: elementDesc.SemanticName = "BLENDWEIGHT"; break;
				default: return 0;
			}

			// set element buffer offset
			elementDesc.AlignedByteOffset = element.offset;

			memcpy(&handle->elements[i], &elementDesc, sizeof(D3D12_INPUT_ELEMENT_DESC));
		}

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_VertexBufferStreamer_Dispose(VertexBufferStreamer* handle)
	{
		if (handle->vertexBuffers != NULL)
		{
			free(handle->vertexBuffers);
			handle->vertexBuffers = NULL;
		}

		if (handle->vertexBufferViews != NULL)
		{
			free(handle->vertexBufferViews);
			handle->vertexBufferViews = NULL;
		}

		if (handle->elements != NULL)
		{
			free(handle->elements);
			handle->elements = NULL;
		}

		free(handle);
	}
}