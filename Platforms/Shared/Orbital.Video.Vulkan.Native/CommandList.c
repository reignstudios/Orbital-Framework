#include "CommandList.h"
#include "SwapChain.h"
#include "RenderPass.h"

ORBITAL_EXPORT CommandList* Orbital_Video_Vulkan_CommandList_Create(Device* device)
{
	CommandList* handle = (CommandList*)calloc(1, sizeof(CommandList));
	handle->device = device;
	return handle;
}

ORBITAL_EXPORT int Orbital_Video_Vulkan_CommandList_Init(CommandList* handle)
{
	VkCommandBufferAllocateInfo allocInfo = {0};
    allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocInfo.commandPool = handle->device->commandPool;
    allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    allocInfo.commandBufferCount = 1;
    if (vkAllocateCommandBuffers(handle->device->device, &allocInfo, &handle->commandBuffer) != VK_SUCCESS) return 0;

	return 1;
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_Dispose(CommandList* handle)
{
	if (handle->commandBuffer != NULL)
	{
		vkFreeCommandBuffers(handle->device->device, handle->device->commandPool, 1, &handle->commandBuffer);
		handle->commandBuffer = NULL;
	}
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_Start(CommandList* handle, Device* device)
{
	vkResetCommandBuffer(handle->commandBuffer, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);

	VkCommandBufferBeginInfo beginInfo = {0};
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	vkBeginCommandBuffer(handle->commandBuffer, &beginInfo);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_Finish(CommandList* handle)
{
	vkEndCommandBuffer(handle->commandBuffer);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_BeginRenderPass(CommandList* handle, RenderPass* renderPass)
{
	VkRenderPassBeginInfo renderPassBeginInfo = {0};
	renderPassBeginInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
	renderPassBeginInfo.renderPass = renderPass->renderPass;
	renderPassBeginInfo.renderArea.offset.x = 0;
	renderPassBeginInfo.renderArea.offset.y = 0;
	renderPassBeginInfo.renderArea.extent.width = renderPass->width;
	renderPassBeginInfo.renderArea.extent.height = renderPass->height;
	if (renderPass->clearColor)
	{
		VkClearValue clearValues[1] = {0};
		memcpy(clearValues[0].color.float32, renderPass->clearColorValue, sizeof(float) * 4);
		renderPassBeginInfo.clearValueCount = 1;
		renderPassBeginInfo.pClearValues = clearValues;
	}
	//clearValues[1].depthStencil.depth = 1.0f;
	//clearValues[1].depthStencil.stencil = 0.0f;
	if (renderPass->swapChain != NULL) renderPassBeginInfo.framebuffer = renderPass->frameBuffers[renderPass->swapChain->currentRenderTargetIndex];
	else renderPassBeginInfo.framebuffer = renderPass->frameBuffers[0];
	vkCmdBeginRenderPass(handle->commandBuffer, &renderPassBeginInfo, VK_SUBPASS_CONTENTS_INLINE);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_EndRenderPass(CommandList* handle)
{
	vkCmdEndRenderPass(handle->commandBuffer);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_ClearSwapChainRenderTarget(CommandList* handle, SwapChain* swapChain, float r, float g, float b, float a)
{
	VkClearColorValue rgba;
	rgba.float32[0] = r;
	rgba.float32[1] = g;
	rgba.float32[2] = b;
	rgba.float32[3] = a;
	vkCmdClearColorImage(handle->commandBuffer, swapChain->images[swapChain->currentRenderTargetIndex], VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, &rgba, 1, &swapChain->subresourceRange);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_Execute(CommandList* handle)
{
	VkPipelineStageFlags pipeStageFlags = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
	VkSubmitInfo submitInfo = {0};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.waitSemaphoreCount = 0;
    submitInfo.pWaitSemaphores = VK_NULL_HANDLE;
    submitInfo.pWaitDstStageMask = &pipeStageFlags;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = &handle->commandBuffer;
    submitInfo.signalSemaphoreCount = 0;
    submitInfo.pSignalSemaphores = NULL;
	vkQueueSubmit(handle->device->queue, 1, &submitInfo, handle->device->fence);
}