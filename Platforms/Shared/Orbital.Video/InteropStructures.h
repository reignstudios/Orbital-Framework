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

typedef enum SamplerState
{
	SamplerState_Default,
	SamplerState_Point,
	SamplerState_Bilinear,
	SamplerState_Trilinear
}SamplerState;

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