#pragma once
#include <stdint.h>

typedef struct RenderPassDesc
{
	char clearColor, clearDepthStencil;
	float clearColorValue[4];
	float depthValue, stencilValue;
}RenderPassDesc;

#pragma region Texture
typedef enum TextureMode
{
	TextureMode_GPUOptimized,
	TextureMode_Write,
	TextureMode_Read
}TextureMode;

typedef enum TextureType
{
	TextureType_1D,
	TextureType_2D,
	TextureType_3D,
	TextureType_Cube
}TextureType;

typedef enum TextureFormat
{
	TextureFormat_Default,
	TextureFormat_DefaultHDR,
	TextureFormat_B8G8R8A8,
	TextureFormat_R10G10B10A2
}TextureFormat;
#pragma endregion

#pragma region Depth Stencil
typedef enum DepthStencilFormat
{
	DepthStencilFormat_Default,
	DepthStencilFormat_D24S8
}DepthStencilFormat;
#pragma endregion

#pragma region Command Buffer
typedef enum ConstantBufferMode
{
	ConstantBufferMode_GPUOptimized,
	ConstantBufferMode_Write,
	ConstantBufferMode_Read
}ConstantBufferMode;
#pragma endregion

#pragma region Vertex Buffer
typedef enum VertexBufferMode
{
	VertexBufferMode_GPUOptimized,
	VertexBufferMode_Write,
	VertexBufferMode_Read
}VertexBufferMode;

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
	int constantBufferCount;
	intptr_t* constantBuffers;
	int textureCount;
	intptr_t* textures;
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

typedef enum ShaderEffectResourceUsage
{
	ShaderEffectResourceUsage_VS = 1,
	ShaderEffectResourceUsage_PS = 2,
	ShaderEffectResourceUsage_HS = 4,
	ShaderEffectResourceUsage_DS = 8,
	ShaderEffectResourceUsage_GS = 16,
	ShaderEffectResourceUsage_All = ShaderEffectResourceUsage_VS | ShaderEffectResourceUsage_PS | ShaderEffectResourceUsage_HS | ShaderEffectResourceUsage_DS | ShaderEffectResourceUsage_GS,
}ShaderEffectResourceUsage;

typedef struct ShaderEffectConstantBuffer
{
	int registerIndex;
	ShaderEffectResourceUsage usage;
}ShaderEffectConstantBuffer;

typedef struct ShaderEffectTexture
{
	int registerIndex;
	ShaderEffectResourceUsage usage;
}ShaderEffectTexture;

typedef enum ShaderEffectSamplerFilter
{
	ShaderEffectSamplerFilter_Default,
	ShaderEffectSamplerFilter_Point,
	ShaderEffectSamplerFilter_Bilinear,
	ShaderEffectSamplerFilter_Trilinear
}ShaderEffectSamplerFilter;

typedef enum ShaderEffectSamplerAddress
{
	ShaderEffectSamplerAddress_Wrap,
	ShaderEffectSamplerAddress_Clamp
}ShaderEffectSamplerAddress;

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
	ShaderEffectSamplerFilter filter;
	ShaderEffectSamplerAnisotropy anisotropy;
	ShaderEffectSamplerAddress addressU, addressV, addressW;
}ShaderEffectSampler;

typedef struct ShaderEffectDesc
{
	int constantBufferCount, textureCount, samplersCount;
	ShaderEffectConstantBuffer* constantBuffers;
	ShaderEffectTexture* textures;
	ShaderEffectSampler* samplers;
}ShaderEffectDesc;
#pragma endregion