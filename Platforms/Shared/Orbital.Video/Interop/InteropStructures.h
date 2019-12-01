#pragma once
#include <stdint.h>

typedef enum SurfaceFormat
{
	SurfaceFormat_Default,
	SurfaceFormat_DefaultHDR,
	SurfaceFormat_B8G8R8A8,
	SurfaceFormat_R10G10B10A2

}SurfaceFormat;

typedef enum DepthStencilFormat
{
	DepthStencilFormat_Default,
	DepthStencilFormat_D24S8
}DepthStencilFormat;

typedef enum Topology
{
	Topology_Point,
	Topology_Line,
	Topology_Triangle
}Topology;

typedef struct RenderPassDesc
{
	char clearColor, clearDepthStencil;
	float clearColorValue[4];
	float depthValue, stencilValue;
}RenderPassDesc;

typedef enum BufferLayoutElementType
{
	BufferLayoutElementType_Float,
	BufferLayoutElementType_Float2,
	BufferLayoutElementType_Float3,
	BufferLayoutElementType_Float4,
	BufferLayoutElementType_RGBAx8
}BufferLayoutElementType;

typedef enum BufferLayoutElementUsage
{
	BufferLayoutElementUsage_Position,
	BufferLayoutElementUsage_Color,
	BufferLayoutElementUsage_UV,
	BufferLayoutElementUsage_Normal,
	BufferLayoutElementUsage_Tangent,
	BufferLayoutElementUsage_Binormal,
	BufferLayoutElementUsage_Index,
	BufferLayoutElementUsage_Weight
}BufferLayoutElementUsage;

typedef struct BufferLayoutElement
{
	BufferLayoutElementType type;
	BufferLayoutElementUsage usage;
	uint32_t streamIndex, usageIndex, byteOffset;
}BufferLayoutElement;

typedef struct BufferLayout
{
	uint32_t elementCount;
	BufferLayoutElement elements[32];
}BufferLayout;

#pragma region Shaders
typedef enum ShaderType
{
	ShaderType_VS,
	ShaderType_PS,
	ShaderType_HS,
	ShaderType_DS,
	ShaderType_GS
}ShaderType;

typedef struct ShaderEffectResource
{
	int registerIndex;
	int usedInTypesCount;
	ShaderType* usedInTypes;
}ShaderEffectResource;

typedef enum ShaderEffectSampleFilter
{
	ShaderEffectSampleFilter_Default,
	ShaderEffectSampleFilter_Point,
	ShaderEffectSampleFilter_Bilinear,
	ShaderEffectSampleFilter_Trilinear
}ShaderEffectSampleFilter;

typedef enum ShaderEffectSampleAddress
{
	ShaderEffectSampleAddress_Wrap,
	ShaderEffectSampleAddress_Clamp
}ShaderEffectSampleAddress;

typedef enum ShaderEffectSamplerAnisotropy
{
	ShaderEffectSamplerAnisotropy_Default = 0,
	ShaderEffectSamplerAnisotropy_X1 = 1,
	ShaderEffectSamplerAnisotropy_X2 = 2,
	ShaderEffectSamplerAnisotropy_X4 = 4,
	ShaderEffectSamplerAnisotropy_X8 = 8,
	ShaderEffectSamplerAnisotropy_X16 = 16
}ShaderEffectSamplerAnisotropy;

typedef struct ShaderEffectSampler
{
	int registerIndex;
	ShaderEffectSampleFilter filter;
	ShaderEffectSampleAddress addressU, addressV, addressW;
	ShaderEffectSamplerAnisotropy anisotropy;
}ShaderEffectSampler;

typedef struct ShaderEffectDesc
{
	int resourcesCount, samplersCount;
	ShaderEffectResource* resources;
	ShaderEffectSampler* samplers;
}ShaderEffectDesc;
#pragma endregion