#include "SwapChain.h"

ORBITAL_EXPORT SwapChain* Orbital_Video_Vulkan_SwapChain_Create(Device* device)
{
	SwapChain* handle = (SwapChain*)calloc(1, sizeof(SwapChain));
	handle->device = device;
	return handle;
}

#ifdef _WIN32
ORBITAL_EXPORT int Orbital_Video_Vulkan_SwapChain_Init(SwapChain* handle, HWND hWnd, UINT* width, UINT* height, int* sizeEnforced, UINT bufferCount, int fullscreen)
#endif
{
	#ifdef _WIN32
	VkWin32SurfaceCreateInfoKHR createSurfaceInfo = {0};
    createSurfaceInfo.sType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR;
    createSurfaceInfo.hwnd = hWnd;
    if (IsWindowUnicode(hWnd) == 0) createSurfaceInfo.hinstance = (HINSTANCE)GetWindowLongPtrA(hWnd, GWLP_HINSTANCE);
	else createSurfaceInfo.hinstance = (HINSTANCE)GetWindowLongPtrW(hWnd, GWLP_HINSTANCE);
    if (vkCreateWin32SurfaceKHR(handle->device->instance->instance, &createSurfaceInfo, NULL, &handle->surface) != VK_SUCCESS) return 0;
	#endif
	
	VkBool32 supportsPresent = 0;
	if (vkGetPhysicalDeviceSurfaceSupportKHR(handle->device->physicalDevice, handle->device->queueFamilyIndex, handle->surface, &supportsPresent) != VK_SUCCESS) return 0;
	if (supportsPresent != VK_TRUE) return 0;// if device doesn't support present with this surface exit

	// get surface capabilities
	VkSurfaceCapabilitiesKHR surfaceCapabilities = {0};
    if (vkGetPhysicalDeviceSurfaceCapabilitiesKHR(handle->device->physicalDevice, handle->surface, &surfaceCapabilities) != VK_SUCCESS) return 0;

	// make sure swap-chain buffer count isn't less than whats avaliable
	if (bufferCount > surfaceCapabilities.minImageCount) return 0;

	// get surface present modes
	uint32_t presentModeCount;
    if (vkGetPhysicalDeviceSurfacePresentModesKHR(handle->device->physicalDevice, handle->surface, &presentModeCount, NULL) != VK_SUCCESS) return 0;

    VkPresentModeKHR *presentModes = alloca(sizeof(VkPresentModeKHR) * presentModeCount);
    if (vkGetPhysicalDeviceSurfacePresentModesKHR(handle->device->physicalDevice, handle->surface, &presentModeCount, presentModes) != VK_SUCCESS) return 0;

	// set swapchain width / height
	VkExtent2D swapchainExtent = {0};
	if (surfaceCapabilities.currentExtent.width == 0xFFFFFFFF && surfaceCapabilities.currentExtent.height == 0xFFFFFFFF)// check if width/height are defined
	{
		// manually set width/height but adjust if its out of range
		if ((*width) < surfaceCapabilities.minImageExtent.width) (*width) = surfaceCapabilities.minImageExtent.width;
		if ((*width) > surfaceCapabilities.maxImageExtent.width) (*width) = surfaceCapabilities.maxImageExtent.width;
		if ((*height) < surfaceCapabilities.minImageExtent.height) (*height) = surfaceCapabilities.minImageExtent.height;
		if ((*height) > surfaceCapabilities.maxImageExtent.height) (*height) = surfaceCapabilities.maxImageExtent.height;
		swapchainExtent.width = (*width);
		swapchainExtent.height = (*height);
		(*sizeEnforced) = 0;
	}
	else
	{
		swapchainExtent = surfaceCapabilities.currentExtent;// if width/height are defined, swap-chain must match
		(*width) = swapchainExtent.width;
		(*height) = swapchainExtent.height;
		(*sizeEnforced) = 1;
	}

	handle->width = swapchainExtent.width;
	handle->height = swapchainExtent.height;

	// get supported alpha mode
	VkCompositeAlphaFlagBitsKHR compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
	if ((surfaceCapabilities.supportedCompositeAlpha & VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR) == 0)
	{
		VkCompositeAlphaFlagBitsKHR compositeAlphaFlags[4] =
		{
			VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR,
			VK_COMPOSITE_ALPHA_PRE_MULTIPLIED_BIT_KHR,
			VK_COMPOSITE_ALPHA_POST_MULTIPLIED_BIT_KHR,
			VK_COMPOSITE_ALPHA_INHERIT_BIT_KHR,
		};
		for (uint32_t i = 0; i != 4; ++i)
		{
			if ((surfaceCapabilities.supportedCompositeAlpha & compositeAlphaFlags[i]) != 0)
			{
				compositeAlpha = compositeAlphaFlags[i];
				break;
			}
		}
	}

	// get buffer format and color space
	uint32_t count = 0;
	if (vkGetPhysicalDeviceSurfaceFormatsKHR(handle->device->physicalDevice, handle->surface, &count, NULL) != VK_SUCCESS) return 0;
	if (count == 0) return 0;

	VkSurfaceFormatKHR* surfaceFormats = alloca(sizeof(VkSurfaceFormatKHR) * count);
	if (vkGetPhysicalDeviceSurfaceFormatsKHR(handle->device->physicalDevice, handle->surface, &count, surfaceFormats) != VK_SUCCESS) return 0;

	handle->format = surfaceFormats[0].format;
	handle->colorSpace = surfaceFormats[0].colorSpace;

	// create swap chain
	VkSwapchainCreateInfoKHR swapChainCreateInfo = {0};
    swapChainCreateInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
	#ifdef __ANDROID__
    swapChainCreateInfo.clipped = VK_FALSE;
	#else
    swapChainCreateInfo.clipped = VK_TRUE;
	#endif
    swapChainCreateInfo.surface = handle->surface;
    swapChainCreateInfo.minImageCount = bufferCount;
    swapChainCreateInfo.imageFormat = handle->format;
    swapChainCreateInfo.imageExtent.width = swapchainExtent.width;
    swapChainCreateInfo.imageExtent.height = swapchainExtent.height;
    swapChainCreateInfo.preTransform = surfaceCapabilities.currentTransform;
    swapChainCreateInfo.compositeAlpha = compositeAlpha;
    swapChainCreateInfo.imageArrayLayers = 1;
    swapChainCreateInfo.presentMode = VK_PRESENT_MODE_FIFO_KHR;
    swapChainCreateInfo.oldSwapchain = VK_NULL_HANDLE;
    swapChainCreateInfo.imageColorSpace = handle->colorSpace;
    swapChainCreateInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
    swapChainCreateInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
    swapChainCreateInfo.queueFamilyIndexCount = 0;
    swapChainCreateInfo.pQueueFamilyIndices = NULL;
    if (vkCreateSwapchainKHR(handle->device->device, &swapChainCreateInfo, NULL, &handle->swapChain) != VK_SUCCESS) return 0;

	// get images
	if (vkGetSwapchainImagesKHR(handle->device->device, handle->swapChain, &handle->bufferCount, NULL) != VK_SUCCESS) return 0;
	if (handle->bufferCount != bufferCount) return 0;// if image count doesn't match buffer count something is wrong

    handle->images = calloc(handle->bufferCount, sizeof(VkImage));
    if (vkGetSwapchainImagesKHR(handle->device->device, handle->swapChain, &handle->bufferCount, handle->images) != VK_SUCCESS) return 0;

	// create subresourceRange
	handle->subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	handle->subresourceRange.baseMipLevel = 0;
    handle->subresourceRange.levelCount = 1;
    handle->subresourceRange.baseArrayLayer = 0;
    handle->subresourceRange.layerCount = 1;

	// create image views
	handle->imageViews = calloc(handle->bufferCount, sizeof(VkImageView));
	for (uint32_t i = 0; i != handle->bufferCount; ++i)
	{
		VkImageViewCreateInfo imageViewCreateInfo = {0};
        imageViewCreateInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
        imageViewCreateInfo.format = handle->format;
        imageViewCreateInfo.components.r = VK_COMPONENT_SWIZZLE_R;
        imageViewCreateInfo.components.g = VK_COMPONENT_SWIZZLE_G;
        imageViewCreateInfo.components.b = VK_COMPONENT_SWIZZLE_B;
        imageViewCreateInfo.components.a = VK_COMPONENT_SWIZZLE_A;
        imageViewCreateInfo.subresourceRange = handle->subresourceRange;
        imageViewCreateInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
        imageViewCreateInfo.flags = 0;
        imageViewCreateInfo.image = handle->images[i];
		if (vkCreateImageView(handle->device->device, &imageViewCreateInfo, NULL, &handle->imageViews[i]) != VK_SUCCESS) return 0;
	}

	// create fence
	VkFenceCreateInfo fenceInfo = {0};
    fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
    fenceInfo.flags = 0;
    if (vkCreateFence(handle->device->device, &fenceInfo, NULL, &handle->fence) != VK_SUCCESS) return 0;

	return 1;
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_SwapChain_Dispose(SwapChain* handle)
{
	if (handle->fence != NULL)
	{
		vkDestroyFence(handle->device->device, handle->fence, NULL);
		handle->fence = NULL;
	}

	if (handle->imageViews != NULL)
	{
		for (uint32_t i = 0; i != handle->bufferCount; ++i)
		{
			if (handle->imageViews[i] != NULL) vkDestroyImageView(handle->device->device, handle->imageViews[i], NULL);
		}
		free(handle->imageViews);
		handle->imageViews = NULL;
	}

	if (handle->images != NULL)
	{
		free(handle->images);
		handle->images = NULL;
	}

	if (handle->swapChain != NULL)
	{
		vkDestroySwapchainKHR(handle->device->device, handle->swapChain, NULL);
		handle->swapChain = NULL;
	}

	if (handle->surface != NULL)
	{
		vkDestroySurfaceKHR(handle->device->instance->instance, handle->surface, NULL);
		handle->surface = NULL;
	}
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_SwapChain_BeginFrame(SwapChain* handle)
{
	vkResetFences(handle->device->device, 1, &handle->fence);
	vkAcquireNextImageKHR(handle->device->device, handle->swapChain, UINT64_MAX, VK_NULL_HANDLE, handle->fence, &handle->currentRenderTargetIndex);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_SwapChain_Present(SwapChain* handle)
{
	VkPresentInfoKHR present = {0};
    present.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
    present.swapchainCount = 1;
    present.pSwapchains = &handle->swapChain;
    present.pImageIndices = &handle->currentRenderTargetIndex;
    present.pWaitSemaphores = NULL;
    present.waitSemaphoreCount = 0;
    present.pResults = NULL;
    vkQueuePresentKHR(handle->device->queue, &present);
	vkWaitForFences(handle->device->device, 1, &handle->fence, VK_TRUE, UINT64_MAX);
}