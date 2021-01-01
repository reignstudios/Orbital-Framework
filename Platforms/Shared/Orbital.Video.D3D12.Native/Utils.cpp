#pragma once
#include "Utils.h"

bool GetNative_SwapChainFormat(SwapChainFormat format, DXGI_FORMAT* nativeFormat)
{
	switch (format)
	{
		case SwapChainFormat::SwapChainFormat_Default:
		case SwapChainFormat::SwapChainFormat_R8G8B8A8:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R8G8B8A8_UNORM;
			break;

		case SwapChainFormat::SwapChainFormat_DefaultHDR:
		case SwapChainFormat::SwapChainFormat_R10G10B10A2:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UNORM;
			break;

		case SwapChainFormat::SwapChainFormat_R16G16B16A16:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R16G16B16A16_FLOAT;
			break;
		default: return false;
	}
	return true;
}

bool GetNative_TextureFormat(TextureFormat format, DXGI_FORMAT* nativeFormat, bool isRenderTexture)
{
	switch (format)
	{
		case TextureFormat::TextureFormat_Default:
		case TextureFormat::TextureFormat_R8G8B8A8:
			*nativeFormat = DXGI_FORMAT_R8G8B8A8_UNORM;//isRenderTexture ? DXGI_FORMAT::DXGI_FORMAT_R8G8B8A8_UNORM : DXGI_FORMAT::DXGI_FORMAT_R8G8B8A8_UINT;
			break;

		case TextureFormat::TextureFormat_DefaultHDR:
		case TextureFormat::TextureFormat_R10G10B10A2:
			*nativeFormat = DXGI_FORMAT_R10G10B10A2_UNORM;//isRenderTexture ? DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UNORM : DXGI_FORMAT::DXGI_FORMAT_R10G10B10A2_UINT;
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
		case DepthStencilFormat::DepthStencilFormat_D32:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_D32_FLOAT;
			break;

		case DepthStencilFormat::DepthStencilFormat_D32S8:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_D32_FLOAT_S8X24_UINT;
			break;

		case DepthStencilFormat::DepthStencilFormat_DefaultDepth:
		case DepthStencilFormat::DepthStencilFormat_DefaultDepthStencil:
		case DepthStencilFormat::DepthStencilFormat_D24S8:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_D24_UNORM_S8_UINT;
			break;

		case DepthStencilFormat::DepthStencilFormat_D16:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_D16_UNORM;
			break;
		default: return false;
	}
	return true;;
}

bool GetNative_DepthStencilShaderResourceFormat(DepthStencilFormat format, DXGI_FORMAT* nativeFormat)
{
	switch (format)
	{
		case DepthStencilFormat::DepthStencilFormat_D32:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R32_FLOAT;
			break;

		case DepthStencilFormat::DepthStencilFormat_D32S8:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS;
			break;

		case DepthStencilFormat::DepthStencilFormat_DefaultDepth:
		case DepthStencilFormat::DepthStencilFormat_DefaultDepthStencil:
		case DepthStencilFormat::DepthStencilFormat_D24S8:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
			break;

		case DepthStencilFormat::DepthStencilFormat_D16:
			*nativeFormat = DXGI_FORMAT::DXGI_FORMAT_R16_UNORM;
			break;
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

bool GetNative_ShaderSamplerAddress(ShaderSamplerAddress address, D3D12_TEXTURE_ADDRESS_MODE* nativeAddress)
{
	switch (address)
	{
		case ShaderSamplerAddress::ShaderSamplerAddress_Wrap:
			*nativeAddress = D3D12_TEXTURE_ADDRESS_MODE::D3D12_TEXTURE_ADDRESS_MODE_WRAP;
			break;

		case ShaderSamplerAddress::ShaderSamplerAddress_Clamp:
			*nativeAddress = D3D12_TEXTURE_ADDRESS_MODE::D3D12_TEXTURE_ADDRESS_MODE_CLAMP;
			break;
		default: return false;
	}
	return true;
}

bool GetNative_ShaderSamplerAnisotropy(ShaderSamplerAnisotropy anisotropy, UINT* nativeAnisotropy)
{
	if (anisotropy == ShaderSamplerAnisotropy::ShaderSamplerAnisotropy_Default) *nativeAnisotropy = 16;
	else *nativeAnisotropy = (UINT)anisotropy;
	return *nativeAnisotropy >= 1 && *nativeAnisotropy <= 16;
}

bool GetNative_ShaderSamplerFilter(ShaderSamplerFilter filter, D3D12_FILTER* nativeFilter, bool useComparison)
{
	switch (filter)
	{
		case ShaderSamplerFilter::ShaderSamplerFilter_Point:
			*nativeFilter = useComparison ? D3D12_FILTER::D3D12_FILTER_COMPARISON_MIN_MAG_MIP_POINT : D3D12_FILTER::D3D12_FILTER_MIN_MAG_MIP_POINT;
			break;

		case ShaderSamplerFilter::ShaderSamplerFilter_Bilinear:
			*nativeFilter = useComparison ? D3D12_FILTER::D3D12_FILTER_COMPARISON_MIN_MAG_LINEAR_MIP_POINT : D3D12_FILTER::D3D12_FILTER_MIN_MAG_LINEAR_MIP_POINT;
			break;

		case ShaderSamplerFilter::ShaderSamplerFilter_Default:
		case ShaderSamplerFilter::ShaderSamplerFilter_Trilinear:
			*nativeFilter = useComparison ? D3D12_FILTER::D3D12_FILTER_COMPARISON_MIN_MAG_MIP_LINEAR : D3D12_FILTER::D3D12_FILTER_MIN_MAG_MIP_LINEAR;
			break;
		default: return false;
	}
	return true;
}

bool GetNative_ShaderComparisonFunction(ShaderComparisonFunction function, D3D12_COMPARISON_FUNC* nativeFunction)
{
	switch (function)
	{
		case ShaderComparisonFunction::ShaderComparisonFunction_Never: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_NEVER; break;
		case ShaderComparisonFunction::ShaderComparisonFunction_Always: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_ALWAYS; break;
		case ShaderComparisonFunction::ShaderComparisonFunction_Equal: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_EQUAL; break;
		case ShaderComparisonFunction::ShaderComparisonFunction_NotEqual: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_NOT_EQUAL; break;
		case ShaderComparisonFunction::ShaderComparisonFunction_LessThan: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_LESS; break;
		case ShaderComparisonFunction::ShaderComparisonFunction_LessThanOrEqual: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_LESS_EQUAL; break;
		case ShaderComparisonFunction::ShaderComparisonFunction_GreaterThan: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_GREATER; break;
		case ShaderComparisonFunction::ShaderComparisonFunction_GreaterThanOrEqual: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_GREATER_EQUAL; break;
		default: return false;
	}
	return true;
}