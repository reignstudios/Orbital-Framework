#pragma once
#include "Utils.h"

bool GetNative_TextureFormat(TextureFormat format, DXGI_FORMAT* nativeFormat)
{
	switch (format)
	{
		case TextureFormat::TextureFormat_Default:
		case TextureFormat::TextureFormat_B8G8R8A8:
			(*nativeFormat) = DXGI_FORMAT::DXGI_FORMAT_B8G8R8A8_UNORM;
			return true;

		case TextureFormat::TextureFormat_DefaultHDR:
		case TextureFormat::TextureFormat_R10G10B10A2:
			(*nativeFormat) = DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UNORM;
			return true;
	}
	return false;
}

bool GetNative_DepthStencilFormat(DepthStencilFormat format, DXGI_FORMAT* nativeFormat)
{
	switch (format)
	{
		case DepthStencilFormat::DepthStencilFormat_Default:
		case DepthStencilFormat::DepthStencilFormat_D24S8:
			(*nativeFormat) = DXGI_FORMAT::DXGI_FORMAT_D24_UNORM_S8_UINT;
			return true;
	}
	return false;
}

bool GetNative_VertexBufferTopology(VertexBufferTopology topology, D3D12_PRIMITIVE_TOPOLOGY_TYPE* nativeTopology)
{
	switch (topology)
	{
		case VertexBufferTopology::VertexBufferTopology_Point:
			(*nativeTopology) = D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_POINT;
			return true;

		case VertexBufferTopology::VertexBufferTopology_Line:
			(*nativeTopology) = D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_LINE;
			return true;

		case VertexBufferTopology::VertexBufferTopology_Triangle:
			(*nativeTopology) = D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
			return true;
	}
	return false;
}