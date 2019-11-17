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
	VkCommandBufferResetFlags resetInfo = {0};
	vkResetCommandBuffer(handle->commandBuffer, &resetInfo);

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
	
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_EnableSwapChainPresent(CommandList* handle, SwapChain* swapChain)
{
	
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_CommandList_ClearSwapChainRenderTarget(CommandList* handle, SwapChain* swapChain, float r, float g, float b, float a)
{
	VkClearColorValue rgba;
	rgba.float32[0] = r;
	rgba.float32[1] = g;
	rgba.float32[2] = b;
	rgba.float32[3] = a;
	vkCmdClearColorImage(handle->commandBuffer, swapChain->images[swapChain->currentRenderTargetIndex], VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, &rgba, 0, NULL);
}