#pragma once
#include "Common.h"

bool GetNative_SurfaceFormat(SurfaceFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_DepthStencilFormat(DepthStencilFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_Topology(Topology topology, D3D12_PRIMITIVE_TOPOLOGY_TYPE* nativeTopology);