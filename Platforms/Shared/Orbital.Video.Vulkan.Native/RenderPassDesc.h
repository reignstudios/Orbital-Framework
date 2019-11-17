#pragma once
#include "Common.h"

typedef struct RenderPassDesc
{
	uint32_t attachmentCount;
	VkAttachmentDescription* attachments;
	uint32_t subpassDependencyCount;
	VkSubpassDependency* subpassDependencies;
	VkSubpassDescription subpass;
} RenderPassDesc;