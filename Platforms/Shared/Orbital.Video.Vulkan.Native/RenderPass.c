#include "RenderPass.h"
#include "RenderPassDesc.h"

ORBITAL_EXPORT RenderPass* Orbital_Video_Vulkan_RenderPass_Create(Device* device)
{
	RenderPass* handle = (RenderPass*)calloc(1, sizeof(RenderPass));
	handle->device = device;
	return handle;
}

ORBITAL_EXPORT int Orbital_Video_Vulkan_RenderPass_Init(RenderPass* handle, RenderPassDesc* desc)
{
	VkRenderPassCreateInfo renderPassInfo = {0};
	renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
	renderPassInfo.attachmentCount = desc->attachmentCount;
	renderPassInfo.pAttachments = desc->attachments;
	renderPassInfo.dependencyCount = desc->subpassDependencyCount;
	renderPassInfo.pDependencies = desc->subpassDependencies;
	renderPassInfo.subpassCount = 1;
	renderPassInfo.pSubpasses = &desc->subpass;
	if (vkCreateRenderPass(handle->device->device, &renderPassInfo, NULL, &handle->renderPass) != VK_SUCCESS) return 0;

	// create frame buffers
	/*VkFramebufferCreateInfo frameBufferInfo = {0};
    frameBufferInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
    frameBufferInfo.renderPass = handle->renderPass;
    frameBufferInfo.attachmentCount = 1;
    frameBufferInfo.pAttachments = &handle->imageViews[i];
    frameBufferInfo.width = (*width);
    frameBufferInfo.height = (*height);
    frameBufferInfo.layers = 1;
	if (vkCreateFramebuffer(handle->device->device, &frameBufferInfo, NULL, &handle->frameBuffer) != VK_SUCCESS) return 0;*/
}