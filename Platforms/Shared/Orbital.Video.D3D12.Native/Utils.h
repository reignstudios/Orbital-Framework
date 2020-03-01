#pragma once
#include "Common.h"

bool GetNative_SwapChainFormat(SwapChainFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_TextureFormat(TextureFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_DepthStencilFormat(DepthStencilFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_DepthStencilShaderResourceFormat(DepthStencilFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_VertexBufferTopology(VertexBufferTopology topology, D3D12_PRIMITIVE_TOPOLOGY_TYPE* nativeTopology);