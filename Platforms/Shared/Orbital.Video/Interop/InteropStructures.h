#pragma once
#include <stdint.h>

typedef struct RenderPassDesc
{
	char clearColor, clearDepthStencil;
	float clearColorValue[4];
	float depthValue, stencilValue;
}RenderPassDesc;

typedef enum TextureFormat
{
	TextureFormat_Default,
	TextureFormat_DefaultHDR,
	TextureFormat_B8G8R8A8,
	TextureFormat_R10G10B10A2
}TextureFormat;

typedef enum DepthStencilFormat
{
	DepthStencilFormat_Default,
	DepthStencilFormat_D24S8
}DepthStencilFormat;

#pragma region Vertex Buffer
typedef enum VertexBufferTopology
{
	VertexBufferTopology_Point,
	VertexBufferTopology_Line,
	VertexBufferTopology_Triangle
}VertexBufferTopology;

typedef enum VertexBufferLayoutElementType
{
	VertexBufferLayoutElementType_Float,
	VertexBufferLayoutElementType_Float2,
	VertexBufferLayoutElementType_Float3,
	VertexBufferLayoutElementType_Float4,
	VertexBufferLayoutElementType_RGBAx8
}VertexBufferLayoutElementType;

typedef enum VertexBufferLayoutElementUsage
{
	VertexBufferLayoutElementUsage_Position,
	VertexBufferLayoutElementUsage_Color,
	VertexBufferLayoutElementUsage_UV,
	VertexBufferLayoutElementUsage_Normal,
	VertexBufferLayoutElementUsage_Tangent,
	VertexBufferLayoutElementUsage_Binormal,
	VertexBufferLayoutElementUsage_Index,
	VertexBufferLayoutElementUsage_Weight
}VertexBufferLayoutElementUsage;

typedef struct VertexBufferLayoutElement
{
	VertexBufferLayoutElementType type;
	VertexBufferLayoutElementUsage usage;
	int streamIndex, usageIndex, byteOffset;
}VertexBufferLayoutElement;

typedef struct VertexBufferLayout
{
	int elementCount;
	VertexBufferLayoutElement* elements;
}VertexBufferLayout;
#pragma endregion

#pragma region Render State
typedef struct RenderStateDesc
{
	void* renderPass;
	void* shaderEffect;
	void* vertexBuffer;
	VertexBufferTopology vertexBufferTopology;
	char depthEnable, stencilEnable;
	int msaaLevel;
}RenderStateDesc;
#pragma endregion

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

typedef struct ShaderEffectConstantBuffer
{
	int registerIndex;
	int size;
}ShaderEffectConstantBuffer;

typedef struct ShaderEffectDesc
{
	int resourcesCount, samplersCount, constantBufferCount;
	ShaderEffectResource* resources;
	ShaderEffectSampler* samplers;
	ShaderEffectConstantBuffer* constantBuffers;
}ShaderEffectDesc;
#pragma endregion