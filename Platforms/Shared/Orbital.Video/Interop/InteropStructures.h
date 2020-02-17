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
	TextureMode_GPUOptimized
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
	TextureFormat_R10G10B10A2,
	TextureFormat_R16G16B16A16,
	TextureFormat_R32G32B32A32,
}TextureFormat;
#pragma endregion

#pragma region Depth Stencil
typedef enum DepthStencilMode
{
	DepthStencilMode_GPUOptimized
}DepthStencilMode;

typedef enum DepthStencilFormat
{
	DepthStencilFormat_DefaultDepth,
	DepthStencilFormat_DefaultDepthStencil,
	DepthStencilFormat_D32,
	DepthStencilFormat_D32S8,
	DepthStencilFormat_D24S8,
	DepthStencilFormat_D16
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

typedef enum IndexBufferMode
{
	IndexBufferMode_GPUOptimized,
	IndexBufferMode_Write,
	IndexBufferMode_Read
}IndexBufferMode;

typedef enum VertexBufferTopology
{
	VertexBufferTopology_Point,
	VertexBufferTopology_Line,
	VertexBufferTopology_Triangle
}VertexBufferTopology;

typedef enum VertexBufferStreamType
{
	VertexBufferStreamType_VertexData,
	VertexBufferStreamType_InstanceData
}VertexBufferStreamType;

typedef struct VertexBufferStreamDesc
{
	intptr_t vertexBuffer;
	VertexBufferStreamType type;
}VertexBufferStreamDesc;

typedef enum VertexBufferStreamElementType
{
	VertexBufferStreamElementType_Float,
	VertexBufferStreamElementType_Float2,
	VertexBufferStreamElementType_Float3,
	VertexBufferStreamElementType_Float4,
	VertexBufferStreamElementType_RGBAx8
}VertexBufferStreamElementType;

typedef enum VertexBufferStreamElementUsage
{
	VertexBufferStreamElementUsage_Position,
	VertexBufferStreamElementUsage_Color,
	VertexBufferStreamElementUsage_UV,
	VertexBufferStreamElementUsage_Normal,
	VertexBufferStreamElementUsage_Tangent,
	VertexBufferStreamElementUsage_Binormal,
	VertexBufferStreamElementUsage_Index,
	VertexBufferStreamElementUsage_Weight
}VertexBufferStreamElementUsage;

typedef struct VertexBufferStreamElement
{
	int index;
	VertexBufferStreamElementType type;
	VertexBufferStreamElementUsage usage;
	int usageIndex;
	int offset;
}VertexBufferStreamElement;

typedef struct VertexBufferStreamLayout
{
	int descCount;
	VertexBufferStreamDesc* descs;
	int elementCount;
	VertexBufferStreamElement* elements;
}VertexBufferStreamLayout;
#pragma endregion

#pragma region Render State
typedef enum MSAALevel
{
	MSAALevel_Disabled = 0,
	MSAALevel_X2 = 2,
	MSAALevel_X4 = 4,
	MSAALevel_X8 = 8,
	MSAALevel_X16 = 16
}MSAALevel;

typedef struct RenderStateDesc
{
	intptr_t renderPass;
	intptr_t shaderEffect;
	int constantBufferCount;
	intptr_t* constantBuffers;
	int textureCount;
	intptr_t* textures;
	VertexBufferTopology vertexBufferTopology;
	intptr_t vertexBufferStreamer;
	intptr_t indexBuffer;
	char depthEnable, stencilEnable;
	MSAALevel msaaLevel;
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