#pragma once
#include "Utils.h"

bool GetNative_SurfaceFormat(SurfaceFormat format, DXGI_FORMAT* nativeFormat)
{
	switch (format)
	{
		case SurfaceFormat::SurfaceFormat_Default:
		case SurfaceFormat::SurfaceFormat_B8G8R8A8:
			(*nativeFormat) = DXGI_FORMAT::DXGI_FORMAT_B8G8R8A8_UNORM;
			return true;

		case SurfaceFormat::SurfaceFormat_DefaultHDR:
		case SurfaceFormat::SurfaceFormat_R10G10B10A2:
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

bool GetNative_Topology(Topology topology, D3D12_PRIMITIVE_TOPOLOGY_TYPE* nativeTopology)
{
	switch (topology)
	{
		case Topology::Topology_Point:
			(*nativeTopology) = D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_POINT;
			return true;

		case Topology::Topology_Line:
			(*nativeTopology) = D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_LINE;
			return true;

		case Topology::Topology_Triangle:
			(*nativeTopology) = D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
			return true;
	}
	return false;
}