#pragma once
#include <stdint.h>

// ensures enums compile as 32-bit regardess of C compiler
#define ENUM_BIT INT_MAX

typedef struct AdapterInfo
{
	bool isPrimary;
	int index;
	byte* name;
	uint32_t vendorID;
	int nodeCount;
	uintptr_t dedicatedGPUMemory;
	uintptr_t deticatedSystemMemory;
	uintptr_t sharedSystemMemory;
}AdapterInfo;

typedef enum DeviceType
{
	DeviceType_Presentation,
	DeviceType_Background,
	DeviceType_BIT = ENUM_BIT
}DeviceType;

typedef enum MultiGPUNodeResourceVisibility
{
	MultiGPUNodeResourceVisibility_Self,
	MultiGPUNodeResourceVisibility_All
}MultiGPUNodeResourceVisibility;

typedef struct RenderPassRenderTargetDesc
{
	byte clearColor;
	float clearColorValue[4];
}RenderPassRenderTargetDesc;

typedef struct RenderPassDepthStencilDesc
{
	byte clearDepth;
	byte clearStencil;
	float depthValue;
	float stencilValue;
}RenderPassDepthStencilDesc;

typedef struct RenderPassDesc
{
	int renderTargetDescCount;
	RenderPassRenderTargetDesc* renderTargetDescs;
	RenderPassDepthStencilDesc depthStencilDesc;
}RenderPassDesc;

typedef enum RandomAccessBufferType
{
	RandomAccessBufferType_Texture,
	RandomAccessBufferType_BIT = ENUM_BIT
}RandomAccessBufferType;

typedef enum SwapChainType
{
	SwapChainType_SingleGPU_Standard,
	SwapChainType_MultiGPU_AFR,
	SwapChainType_BIT = ENUM_BIT
}SwapChainType;

typedef enum SwapChainVSyncMode
{
	SwapChainVSyncMode_VSyncOn,
	SwapChainVSyncMode_VSyncOff
}SwapChainVSyncMode;

#pragma region Texture / Surface
typedef enum TextureMode
{
	TextureMode_GPUOptimized,
	TextureMode_BIT = ENUM_BIT
}TextureMode;

typedef enum TextureType
{
	TextureType_1D,
	TextureType_2D,
	TextureType_3D,
	TextureType_Cube,
	TextureType_BIT = ENUM_BIT
}TextureType;

typedef enum TextureFormat
{
	TextureFormat_Default,
	TextureFormat_DefaultHDR,
	TextureFormat_B8G8R8A8,
	TextureFormat_R10G10B10A2,
	TextureFormat_R16G16B16A16,
	TextureFormat_R32G32B32A32,
	TextureFormat_BIT = ENUM_BIT
}TextureFormat;

typedef enum RenderTextureUsage
{
	RenderTextureUsage_Discard,
	RenderTextureUsage_Preserve,
	RenderTextureUsage_BIT = ENUM_BIT
}RenderTextureUsage;

typedef enum SwapChainFormat
{
	SwapChainFormat_Default,
	SwapChainFormat_DefaultHDR,
	SwapChainFormat_B8G8R8A8,
	SwapChainFormat_R10G10B10A2,
	SwapChainFormat_BIT = ENUM_BIT
}SwapChainFormat;
#pragma endregion

#pragma region Depth Stencil
typedef enum DepthStencilMode
{
	DepthStencilMode_GPUOptimized,
	DepthStencilMode_BIT = ENUM_BIT
}DepthStencilMode;

typedef enum DepthStencilFormat
{
	DepthStencilFormat_DefaultDepth,
	DepthStencilFormat_DefaultDepthStencil,
	DepthStencilFormat_D32,
	DepthStencilFormat_D32S8,
	DepthStencilFormat_D24S8,
	DepthStencilFormat_D16,
	DepthStencilFormat_BIT = ENUM_BIT
}DepthStencilFormat;

typedef enum StencilUsage
{
	StencilUsage_Discard,
	StencilUsage_Preserve,
	StencilUsage_BIT = ENUM_BIT
}StencilUsage;
#pragma endregion

#pragma region Constant Buffer
typedef enum ConstantBufferMode
{
	ConstantBufferMode_GPUOptimized,
	ConstantBufferMode_Write,
	ConstantBufferMode_Read,
	ConstantBufferMode_BIT = ENUM_BIT
}ConstantBufferMode;
#pragma endregion

#pragma region Command List
typedef enum CommandListType
{
	CommandListType_Rasterize,
	CommandListType_Compute,
	CommandListType_BIT = ENUM_BIT
}CommandListType;
#pragma endregion

#pragma region Vertex Buffer
typedef enum VertexBufferMode
{
	VertexBufferMode_GPUOptimized,
	VertexBufferMode_Write,
	VertexBufferMode_Read,
	VertexBufferMode_BIT = ENUM_BIT
}VertexBufferMode;

typedef enum IndexBufferMode
{
	IndexBufferMode_GPUOptimized,
	IndexBufferMode_Write,
	IndexBufferMode_Read,
	IndexBufferMode_BIT = ENUM_BIT
}IndexBufferMode;

typedef enum VertexBufferTopology
{
	VertexBufferTopology_Point,
	VertexBufferTopology_Line,
	VertexBufferTopology_Triangle,
	VertexBufferTopology_BIT = ENUM_BIT
}VertexBufferTopology;

typedef enum VertexBufferStreamType
{
	VertexBufferStreamType_VertexData,
	VertexBufferStreamType_InstanceData,
	VertexBufferStreamType_BIT = ENUM_BIT
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
	VertexBufferStreamElementType_RGBAx8,
	VertexBufferStreamElementType_BIT = ENUM_BIT
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
	VertexBufferStreamElementUsage_Weight,
	VertexBufferStreamElementUsage_BIT = ENUM_BIT
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
	MSAALevel_X16 = 16,
	MSAALevel_BIT = ENUM_BIT
}MSAALevel;

typedef enum TriangleCulling
{
	TriangleCulling_None,
	TriangleCulling_Back,
	TriangleCulling_Front,
	TriangleCulling_BIT = ENUM_BIT
}TriangleCulling;

typedef enum TriangleFillMode
{
	TriangleFillMode_Solid,
	TriangleFillMode_Wireframe,
	TriangleFillMode_BIT = ENUM_BIT
}TriangleFillMode;

typedef enum BlendFactor
{
	BlendFactor_Zero,
	BlendFactor_One,
	BlendFactor_SourceColor,
	BlendFactor_SourceColorInverse,
	BlendFactor_SourceAlpha,
	BlendFactor_SourceAlphaInverse,
	BlendFactor_DestinationColor,
	BlendFactor_DestinationColorInverse,
	BlendFactor_DestinationAlpha,
	BlendFactor_DestinationAlphaInverse,
	BlendFactor_SourceAlphaSaturate,
	BlendFactor_SourceColor2,
	BlendFactor_SourceColorInverse2,
	BlendFactor_SourceAlpha2,
	BlendFactor_SourceAlphaInverse2,
	BlendFactor_BIT = ENUM_BIT
}BlendFactor;

typedef enum BlendOperation
{
	BlendOperation_Add,
	BlendOperation_Subtract,
	BlendOperation_SubtractReversed,
	BlendOperation_Minimum,
	BlendOperation_Maximum,
	BlendOperation_BIT = ENUM_BIT
}BlendOperation;

typedef enum LogicalBlendOperation
{
	LogicalBlendOperation_NoOperation,
	LogicalBlendOperation_Clear,
	LogicalBlendOperation_Set,
	LogicalBlendOperation_Copy,
	LogicalBlendOperation_CopyInverted,
	LogicalBlendOperation_Invert,
	LogicalBlendOperation_AND,
	LogicalBlendOperation_NAND,
	LogicalBlendOperation_OR,
	LogicalBlendOperation_NOR,
	LogicalBlendOperation_XOR,
	LogicalBlendOperation_Equivalent,
	LogicalBlendOperation_AND_Reverse,
	LogicalBlendOperation_AND_Inverted,
	LogicalBlendOperation_OR_Reverse,
	LogicalBlendOperation_OR_Inverted,
	LogicalBlendOperation_BIT = ENUM_BIT
}LogicalBlendOperation;

typedef enum BlendWriteMask
{
	BlendWriteMask_Red = 1,
	BlendWriteMask_Green = 2,
	BlendWriteMask_Blue = 3,
	BlendWriteMask_Alpha = 4,
	BlendWriteMask_All = BlendWriteMask_Red | BlendWriteMask_Green | BlendWriteMask_Blue | BlendWriteMask_Alpha,
	BlendWriteMask_BIT = ENUM_BIT
}BlendWriteMask;

typedef struct RenderTargetBlendDesc
{
	byte blendingEnabled;
	byte logicOperationEnabled;
	byte alphaBlendingSeparated;
	BlendFactor sourceFactor;
	BlendFactor destinationFactor;
	BlendOperation operation;
	BlendFactor sourceAlphaFactor;
	BlendFactor destinationAlphaFactor;
	BlendOperation alphaOperation;
	LogicalBlendOperation logicalOperation;
	BlendWriteMask writeMask;
}RenderTargetBlendDesc;

typedef struct BlendDesc
{
	byte alphaToCoverageEnable;
	byte independentBlendEnable;
	int renderTargetBlendDescCount;
	RenderTargetBlendDesc* renderTargetBlendDescs;
}BlendDesc;

typedef enum DepthStencilTestFunction
{
	DepthStencilTestFunction_Always,
	DepthStencilTestFunction_Never,
	DepthStencilTestFunction_Equal,
	DepthStencilTestFunction_NotEqual,
	DepthStencilTestFunction_LessThan,
	DepthStencilTestFunction_LessThanOrEqual,
	DepthStencilTestFunction_GreaterThan,
	DepthStencilTestFunction_GreaterThanOrEqual,
	DepthStencilTestFunction_BIT = ENUM_BIT
}DepthStencilTestFunction;

typedef enum StencilOperation
{
	StencilOperation_Keep,
	StencilOperation_Zero,
	StencilOperation_Invert,
	StencilOperation_IncrementWrap,
	StencilOperation_DecrementWrap,
	StencilOperation_IncrementClamp,
	StencilOperation_DecrementClamp,
	StencilOperation_BIT = ENUM_BIT
}StencilOperation;

typedef struct StencilTestOperationDesc
{
	DepthStencilTestFunction stencilTestFunction;
	StencilOperation stencilPassOperation;
	StencilOperation stencilFailOperation;
	StencilOperation stencilDepthFailOperation;
}StencilTestOperationDesc;

typedef struct DepthStencilDesc
{
	byte depthTestEnable;
	byte depthWriteEnable;
	DepthStencilTestFunction depthTestFunction;
	byte stencilTestEnable;
	byte stencilWriteEnable;
	StencilTestOperationDesc stencilFrontFacingDesc;
	StencilTestOperationDesc stencilBackFacingDesc;
}DepthStencilDesc;

typedef struct RenderStateDesc
{
	intptr_t renderPass;
	intptr_t shaderEffect;
	int constantBufferCount;
	intptr_t* constantBuffers;
	int textureCount;
	intptr_t* textures;
	int textureDepthStencilCount;
	intptr_t* textureDepthStencils;
	int randomAccessBufferCount;
	intptr_t* randomAccessBuffers;
	RandomAccessBufferType* randomAccessTypes;
	VertexBufferTopology vertexBufferTopology;
	intptr_t vertexBufferStreamer;
	intptr_t indexBuffer;
	TriangleCulling triangleCulling;
	TriangleFillMode triangleFillMode;
	BlendDesc blendDesc;
	DepthStencilDesc depthStencilDesc;
}RenderStateDesc;
#pragma endregion

#pragma region Compute State
typedef struct ComputeStateDesc
{
	intptr_t computeShader;
	int constantBufferCount;
	intptr_t* constantBuffers;
	int textureCount;
	intptr_t* textures;
	int textureDepthStencilCount;
	intptr_t* textureDepthStencils;
	int randomAccessBufferCount;
	intptr_t* randomAccessBuffers;
	RandomAccessBufferType* randomAccessTypes;
}ComputeStateDesc;
#pragma endregion

#pragma region Shaders
typedef enum ShaderType
{
	ShaderType_VS,
	ShaderType_PS,
	ShaderType_HS,
	ShaderType_DS,
	ShaderType_GS,
	ShaderType_CS,
	ShaderType_BIT = ENUM_BIT
}ShaderType;

typedef enum ShaderSamplerFilter
{
	ShaderSamplerFilter_Default,
	ShaderSamplerFilter_Point,
	ShaderSamplerFilter_Bilinear,
	ShaderSamplerFilter_Trilinear,
	ShaderSamplerFilter_BIT = ENUM_BIT
}ShaderSamplerFilter;

typedef enum ShaderSamplerAddress
{
	ShaderSamplerAddress_Wrap,
	ShaderSamplerAddress_Clamp,
	ShaderSamplerAddress_BIT = ENUM_BIT
}ShaderSamplerAddress;

typedef enum ShaderSamplerAnisotropy
{
	ShaderSamplerAnisotropy_Default = 0,
	ShaderSamplerAnisotropy_X1 = 1,
	ShaderSamplerAnisotropy_X2 = 2,
	ShaderSamplerAnisotropy_X4 = 4,
	ShaderSamplerAnisotropy_X8 = 8,
	ShaderSamplerAnisotropy_X16 = 16,
	ShaderSamplerAnisotropy_BIT = ENUM_BIT
}ShaderSamplerAnisotropy;

typedef enum ShaderComparisonFunction
{
	ShaderComparisonFunction_Never,
	ShaderComparisonFunction_Always,
	ShaderComparisonFunction_Equal,
	ShaderComparisonFunction_NotEqual,
	ShaderComparisonFunction_LessThan,
	ShaderComparisonFunction_LessThanOrEqual,
	ShaderComparisonFunction_GreaterThan,
	ShaderComparisonFunction_GreaterThanOrEqual,
	ShaderComparisonFunction_BIT = ENUM_BIT
}ShaderComparisonFunction;
#pragma endregion

#pragma region ShaderEffect
typedef enum ShaderEffectResourceUsage
{
	ShaderEffectResourceUsage_VS = 1,
	ShaderEffectResourceUsage_PS = 2,
	ShaderEffectResourceUsage_HS = 4,
	ShaderEffectResourceUsage_DS = 8,
	ShaderEffectResourceUsage_GS = 16,
	ShaderEffectResourceUsage_All = ShaderEffectResourceUsage_VS | ShaderEffectResourceUsage_PS | ShaderEffectResourceUsage_HS | ShaderEffectResourceUsage_DS | ShaderEffectResourceUsage_GS,
	ShaderEffectResourceUsage_BIT = ENUM_BIT
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

typedef struct ShaderEffectSampler
{
	int registerIndex;
	ShaderSamplerFilter filter;
	ShaderSamplerAnisotropy anisotropy;
	ShaderSamplerAddress addressU, addressV, addressW;
	ShaderComparisonFunction comparisonFunction;
	ShaderEffectResourceUsage usage;
}ShaderEffectSampler;

typedef struct ShaderEffectRandomAccessBuffer
{
	int registerIndex;
	ShaderEffectResourceUsage usage;
}ShaderEffectRandomAccessBuffer;

typedef struct ShaderEffectDesc
{
	int constantBufferCount, textureCount, samplersCount, readWriteBufferCount;
	ShaderEffectConstantBuffer* constantBuffers;
	ShaderEffectTexture* textures;
	ShaderEffectSampler* samplers;
	ShaderEffectRandomAccessBuffer* randomAccessBuffers;
}ShaderEffectDesc;
#pragma endregion

#pragma region ComputeShader
typedef struct ComputeShaderConstantBuffer
{
	int registerIndex;
}ComputeShaderConstantBuffer;

typedef struct ComputeShaderTexture
{
	int registerIndex;
}ComputeShaderTexture;

typedef struct ComputeShaderSampler
{
	int registerIndex;
	ShaderSamplerFilter filter;
	ShaderSamplerAnisotropy anisotropy;
	ShaderSamplerAddress addressU, addressV, addressW;
	ShaderComparisonFunction comparisonFunction;
}ComputeShaderSampler;

typedef struct ComputeShaderRandomAccessBuffer
{
	int registerIndex;
}ComputeShaderRandomAccessBuffer;

typedef struct ComputeShaderDesc
{
	int constantBufferCount, textureCount, samplersCount, randomAccessBufferCount;
	ComputeShaderConstantBuffer* constantBuffers;
	ComputeShaderTexture* textures;
	ComputeShaderSampler* samplers;
	ComputeShaderRandomAccessBuffer* randomAccessBuffers;
}ComputeShaderDesc;
#pragma endregion