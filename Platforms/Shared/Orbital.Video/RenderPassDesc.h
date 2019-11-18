#pragma once

typedef struct RenderPassDesc
{
	char clearColor, clearDepthStencil;
	float clearColorValue[4];
	float depthValue, stencilValue;
}RenderPassDesc;