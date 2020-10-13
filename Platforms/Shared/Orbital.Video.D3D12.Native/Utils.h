#pragma once
#include "Common.h"

bool GetNative_SwapChainFormat(SwapChainFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_TextureFormat(TextureFormat format, DXGI_FORMAT* nativeFormat, bool isRenderTexture);
bool GetNative_DepthStencilFormat(DepthStencilFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_DepthStencilShaderResourceFormat(DepthStencilFormat format, DXGI_FORMAT* nativeFormat);
bool GetNative_VertexBufferTopology(VertexBufferTopology topology, D3D12_PRIMITIVE_TOPOLOGY_TYPE* nativeTopology);

bool GetNative_ShaderSamplerAddress(ShaderSamplerAddress address, D3D12_TEXTURE_ADDRESS_MODE* nativeAddress);
bool GetNative_ShaderSamplerAnisotropy(ShaderSamplerAnisotropy anisotropy, UINT* nativeAnisotropy);
bool GetNative_ShaderSamplerFilter(ShaderSamplerFilter filter, D3D12_FILTER* nativeFilter, bool useComparison);
bool GetNative_ShaderComparisonFunction(ShaderComparisonFunction function, D3D12_COMPARISON_FUNC* nativeFunction);