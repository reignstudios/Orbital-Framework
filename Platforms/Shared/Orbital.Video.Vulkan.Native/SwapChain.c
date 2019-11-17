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

	VkSwapchainCreateInfoKHR swapchain_ci = {0};
    swapchain_ci.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
	#ifdef __ANDROID__
    swapchain_ci.clipped = VK_FALSE;
	#else
    swapchain_ci.clipped = VK_TRUE;
	#endif
    swapchain_ci.surface = handle->surface;
    swapchain_ci.minImageCount = bufferCount;
    swapchain_ci.imageFormat = VK_FORMAT_R8G8B8A8_UNORM;
    swapchain_ci.imageExtent.width = swapchainExtent.width;
    swapchain_ci.imageExtent.height = swapchainExtent.height;
    swapchain_ci.preTransform = surfaceCapabilities.currentTransform;// VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR
    swapchain_ci.compositeAlpha = compositeAlpha;
    swapchain_ci.imageArrayLayers = 1;
    swapchain_ci.presentMode = VK_PRESENT_MODE_FIFO_KHR;
    swapchain_ci.oldSwapchain = VK_NULL_HANDLE;
    swapchain_ci.imageColorSpace = VK_COLORSPACE_SRGB_NONLINEAR_KHR;
    swapchain_ci.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
    swapchain_ci.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
    swapchain_ci.queueFamilyIndexCount = 0;
    swapchain_ci.pQueueFamilyIndices = NULL;
    if (vkCreateSwapchainKHR(handle->device->device, &swapchain_ci, NULL, &handle->swapChain) != VK_SUCCESS) return 0;

	// create swap-chain image views
	if (vkGetSwapchainImagesKHR(handle->device->device, handle->swapChain, &handle->imageCount, NULL) != VK_SUCCESS) return 0;

    handle->images = malloc(sizeof(VkImage) * handle->imageCount);
    if (vkGetSwapchainImagesKHR(handle->device->device, handle->swapChain, &handle->imageCount, handle->images) != VK_SUCCESS) return 0;

	handle->imageViews = malloc(sizeof(VkImageView) * handle->imageCount);
	for (uint32_t i = 0; i != handle->imageCount; ++i)
	{
		VkImageViewCreateInfo imageViewCreateInfo = {0};
        imageViewCreateInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
        imageViewCreateInfo.format = VK_FORMAT_R8G8B8A8_UNORM;
        imageViewCreateInfo.components.r = VK_COMPONENT_SWIZZLE_R;
        imageViewCreateInfo.components.g = VK_COMPONENT_SWIZZLE_G;
        imageViewCreateInfo.components.b = VK_COMPONENT_SWIZZLE_B;
        imageViewCreateInfo.components.a = VK_COMPONENT_SWIZZLE_A;
        imageViewCreateInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
        imageViewCreateInfo.subresourceRange.baseMipLevel = 0;
        imageViewCreateInfo.subresourceRange.levelCount = 1;
        imageViewCreateInfo.subresourceRange.baseArrayLayer = 0;
        imageViewCreateInfo.subresourceRange.layerCount = 1;
        imageViewCreateInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
        imageViewCreateInfo.flags = 0;
        imageViewCreateInfo.image = handle->images[i];
		if (vkCreateImageView(handle->device->device, &imageViewCreateInfo, NULL, &handle->imageViews[i]) != VK_SUCCESS) return 0;
	}

	return 1;
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_SwapChain_Dispose(SwapChain* handle)
{
	if (handle->imageViews != NULL)
	{
		for (uint32_t i = 0; i != handle->imageCount; ++i)
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
	vkAcquireNextImageKHR(handle->device->device, handle->swapChain, UINT_MAX, handle->device->semaphore, handle->device->fence, &handle->currentRenderTargetIndex);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_SwapChain_Present(SwapChain* handle)
{
	VkPresentInfoKHR present;
    present.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
    present.pNext = NULL;
    present.swapchainCount = 1;
    present.pSwapchains = &handle->swapChain;
    present.pImageIndices = &handle->currentRenderTargetIndex;
    present.pWaitSemaphores = NULL;
    present.waitSemaphoreCount = 0;
    present.pResults = NULL;
    vkQueuePresentKHR(handle->device->queue, &present);
}