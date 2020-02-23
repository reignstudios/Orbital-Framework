#pragma once
#include "Utils.h"

bool GetNative_SwapChainFormat(SwapChainFormat format, DXGI_FORMAT* nativeFormat)
{
	switch (format)
	{
		case SwapChainFormat::SwapChainFormat_Default:
		case SwapChainFormat::SwapChainFormat_B8G8R8A8:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_B8G8R8A8_UNORM;
			break;

		case SwapChainFormat::SwapChainFormat_DefaultHDR:
		case SwapChainFormat::SwapChainFormat_R10G10B10A2:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UNORM;
			break;
		default: return false;
	}
	return true;
}

bool GetNative_TextureFormat(TextureFormat format, DXGI_FORMAT* nativeFormat)
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

		case TextureFormat::TextureFormat_R16G16B16A16: *nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R16G16B16A16_FLOAT; break;
		case TextureFormat::TextureFormat_R32G32B32A32: *nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R32G32B32A32_FLOAT; break;
		default: return false;
	}
	return true;
}

bool GetNative_DepthStencilFormat(DepthStencilFormat format, DXGI_FORMAT* nativeFormat)
{
	switch (format)
	{
		case DepthStencilFormat::DepthStencilFormat_DefaultDepth:
		case DepthStencilFormat::DepthStencilFormat_D32:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_D32_FLOAT;
			break;

		case DepthStencilFormat::DepthStencilFormat_DefaultDepthStencil:
		case DepthStencilFormat::DepthStencilFormat_D32S8:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_D32_FLOAT_S8X24_UINT;
			break;

		case DepthStencilFormat::DepthStencilFormat_D24S8: *nativeFormat = DXGI_FORMAT::DXGI_FORMAT_D24_UNORM_S8_UINT; break;
		case DepthStencilFormat::DepthStencilFormat_D16: *nativeFormat = DXGI_FORMAT::DXGI_FORMAT_D16_UNORM; break;
		default: return false;
	}
	return true;;
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