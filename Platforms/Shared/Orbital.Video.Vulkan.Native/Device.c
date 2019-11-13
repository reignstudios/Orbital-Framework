#include "Device.h"
#include <malloc.h>

ORBITAL_EXPORT Device* Orbital_Video_Vulkan_Device_Create(Instance* instance)
{
	Device* handle = (Device*)calloc(1, sizeof(Device));
	handle->instance = instance;
	return handle;
}

ORBITAL_EXPORT int Orbital_Video_Vulkan_Device_Init(Device* handle, int adapterIndex)
{
	// -1 adapter defaults to 0
	if (adapterIndex == -1) adapterIndex = 0;

	// get physical device
	uint32_t deviceCount;
	if (vkEnumeratePhysicalDevices(handle->instance->instance, &deviceCount, NULL) != VK_SUCCESS) return 0;
	if (adapterIndex >= deviceCount) return 0;

	VkPhysicalDevice* physicalDevices = alloca(sizeof(VkPhysicalDevice) * deviceCount);
	if (vkEnumeratePhysicalDevices(handle->instance->instance, &deviceCount, physicalDevices) != VK_SUCCESS) return 0;
	handle->physicalDevice = physicalDevices[adapterIndex];

	// get physical device group if possible
	if (handle->instance->nativeMaxFeatureLevel >= VK_API_VERSION_1_1)
	{
		uint32_t deviceGroupCount;
		if (vkEnumeratePhysicalDeviceGroups(handle->instance->instance, &deviceGroupCount, NULL) != VK_SUCCESS) return 0;
		if (deviceGroupCount != 0)
		{
			VkPhysicalDeviceGroupProperties* physicalDeviceGroups = alloca(sizeof(VkPhysicalDeviceGroupProperties) * deviceGroupCount);
			if (vkEnumeratePhysicalDeviceGroups(handle->instance->instance, &deviceGroupCount, physicalDeviceGroups) != VK_SUCCESS) return 0;
			for (uint32_t i = 0; i != deviceGroupCount; ++i)
			{
				if (physicalDeviceGroups[i].physicalDeviceCount <= 0) continue;
				for (uint32_t j = 0; j != physicalDeviceGroups[i].physicalDeviceCount; ++j)
				{
					if (physicalDeviceGroups[i].physicalDevices[j] != handle->physicalDevice) continue;
					handle->physicalDeviceGroup = physicalDeviceGroups[j];
				}
			}
		}
	}

	// get max feature level
	VkPhysicalDeviceProperties physicalDeviceProperties = {0};
	vkGetPhysicalDeviceProperties(handle->physicalDevice, &physicalDeviceProperties);
	handle->nativeFeatureLevel = physicalDeviceProperties.apiVersion;

	// validate max isn't less than min
	if (handle->nativeFeatureLevel < handle->instance->nativeMinFeatureLevel) return 0;
	for (int i = 0; i != handle->physicalDeviceGroup.physicalDeviceCount; ++i)
	{
		VkPhysicalDeviceProperties deviceProperties = {0};
		vkGetPhysicalDeviceProperties(handle->physicalDeviceGroup.physicalDevices[i], &deviceProperties);
		if (deviceProperties.apiVersion < handle->instance->nativeMinFeatureLevel) return 0;
		if (deviceProperties.apiVersion != handle->nativeFeatureLevel) return 0;// make sure all devices support the same api version
	}

	// TODO?: check extensions
	//vkEnumerateDeviceExtensionProperties(demo->gpu, NULL, &device_extension_count, NULL);

	// TODO?: check device features
	//VkPhysicalDeviceFeatures physDevFeatures;
    //vkGetPhysicalDeviceFeatures(demo->gpu, &physDevFeatures);

	// create device
    float queuePriorities[1] = {0};
    VkDeviceQueueCreateInfo queueCreateInfo[1] = {0};
    queueCreateInfo[0].sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
    queueCreateInfo[0].pNext = NULL;
    queueCreateInfo[0].queueFamilyIndex = 0;
    queueCreateInfo[0].queueCount = 1;
    queueCreateInfo[0].pQueuePriorities = queuePriorities;
    queueCreateInfo[0].flags = 0;

    VkDeviceCreateInfo deviceInfo = {0};
    deviceInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
    deviceInfo.pNext = NULL;
    deviceInfo.queueCreateInfoCount = 1;
    deviceInfo.pQueueCreateInfos = queueCreateInfo;
    deviceInfo.enabledLayerCount = 0;
    deviceInfo.ppEnabledLayerNames = NULL;
    deviceInfo.enabledExtensionCount = 0;
    deviceInfo.ppEnabledExtensionNames = NULL;
    deviceInfo.pEnabledFeatures = NULL;

	if (vkCreateDevice(handle->physicalDevice, &deviceInfo, NULL, &handle->device) != VK_SUCCESS) return 0;

	// create command pool
	VkCommandPoolCreateInfo poolCreateInfo = {0};
    poolCreateInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
    poolCreateInfo.pNext = NULL;
    poolCreateInfo.queueFamilyIndex = 0;
    poolCreateInfo.flags = 0;
	if (vkCreateCommandPool(handle->device, &poolCreateInfo, NULL, &handle->commandPool) != VK_SUCCESS) return 0;

	return 1;
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_Device_Dispose(Device* handle)
{
	if (handle->commandPool != NULL)
	{
		vkDestroyCommandPool(handle->device, handle->commandPool, NULL);
		handle->commandPool = NULL;
	}

	if (handle->device != NULL)
	{
		vkDestroyDevice(handle->device, NULL);
		handle->device = NULL;
	}
	
	free(handle);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_Device_BeginFrame(Device* handle)
{
	
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_Device_EndFrame(Device* handle)
{
	vkDeviceWaitIdle(handle->device);
}