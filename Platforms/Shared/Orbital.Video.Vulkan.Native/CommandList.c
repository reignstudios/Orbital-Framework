#include "CommandList.h"
#include "SwapChain.h"

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

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_EnableSwapChainRenderTarget(CommandList* handle, SwapChain* swapChain)
{
	VkClearValue clearValues[1] = {0};
	clearValues[0].color.float32[0] = 1;
	clearValues[0].color.float32[1] = 0;
	clearValues[0].color.float32[2] = 0;
	clearValues[0].color.float32[3] = 0;
	//clearValues[1].depthStencil.depth = 1.0f;
	//clearValues[1].depthStencil.stencil = 0.0f;

	VkRenderPassBeginInfo renderPassBeginInfo = {0};
	renderPassBeginInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
	renderPassBeginInfo.renderPass = swapChain->renderPass;
	renderPassBeginInfo.renderArea.offset.x = 0;
	renderPassBeginInfo.renderArea.offset.y = 0;
	renderPassBeginInfo.renderArea.extent.width = swapChain->width;
	renderPassBeginInfo.renderArea.extent.height = swapChain->height;
	renderPassBeginInfo.clearValueCount = 1;
	renderPassBeginInfo.pClearValues = clearValues;
	renderPassBeginInfo.framebuffer = swapChain->frameBuffers[swapChain->currentRenderTargetIndex];
	vkCmdBeginRenderPass(handle->commandBuffer, &renderPassBeginInfo, VK_SUBPASS_CONTENTS_INLINE);

	//vkCmdClearAttachments()
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_EnableSwapChainPresent(CommandList* handle, SwapChain* swapChain)
{
	vkCmdEndRenderPass(handle->commandBuffer);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_ClearSwapChainRenderTarget(CommandList* handle, SwapChain* swapChain, float r, float g, float b, float a)
{
	/*VkClearColorValue rgba;
	rgba.float32[0] = r;
	rgba.float32[1] = g;
	rgba.float32[2] = b;
	rgba.float32[3] = a;
	vkCmdClearColorImage(handle->commandBuffer, swapChain->images[swapChain->currentRenderTargetIndex], VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, &rgba, 1, &swapChain->subresourceRange);*/
}