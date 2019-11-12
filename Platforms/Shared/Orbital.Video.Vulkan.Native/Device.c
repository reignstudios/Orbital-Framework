#include "Device.h"
#include <malloc.h>

ORBITAL_EXPORT Device* Orbital_Video_Vulkan_Device_Create(Instance* instance)
{
	return (Device*)calloc(1, sizeof(Device));
}

ORBITAL_EXPORT int Orbital_Video_Vulkan_Device_Init(Device* handle, int adapterIndex)
{
	// -1 adapter defaults to 0
	if (adapterIndex == -1) adapterIndex = 0;

	// create device at max feature/sdk level
	if (handle->instance->nativeMaxFeatureLevel >= VK_API_VERSION_1_1)
	{
		uint32_t deviceGroupCount;
		if (vkEnumeratePhysicalDeviceGroups(handle->instance->instance, &deviceGroupCount, NULL) != VK_SUCCESS) return 0;
		if (adapterIndex >= deviceGroupCount) return 0;

		VkPhysicalDeviceGroupProperties* deviceGroups = alloca(sizeof(VkPhysicalDeviceGroupProperties) * deviceGroupCount);
		if (vkEnumeratePhysicalDeviceGroups(handle->instance->instance, &deviceGroupCount, deviceGroups) != VK_SUCCESS) return 0;
		if (deviceGroups[adapterIndex].physicalDeviceCount <= 0) return 0;
		handle->physicalDeviceGroup = deviceGroups[adapterIndex];
		handle->physicalDevice = handle->physicalDeviceGroup.physicalDevices[0];
	}
	else
	{
		uint32_t deviceCount;
		if (vkEnumeratePhysicalDevices(handle->instance->instance, &deviceCount, NULL) != VK_SUCCESS) return 0;
		if (adapterIndex >= deviceCount) return 0;

		VkPhysicalDevice* devices = alloca(sizeof(VkPhysicalDevice) * deviceCount);
		if (vkEnumeratePhysicalDevices(handle->instance->instance, &deviceCount, devices) != VK_SUCCESS) return 0;
		handle->physicalDevice = devices[adapterIndex];
	}

	// get max feature level
	VkPhysicalDeviceProperties deviceProperties = {0};
	vkGetPhysicalDeviceProperties(handle->physicalDevice, &deviceProperties);
	handle->nativeFeatureLevel = deviceProperties.apiVersion;

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
    float queue_priorities[1] = {0};
    VkDeviceQueueCreateInfo queueCreateInfo[2] = {0};
    queueCreateInfo[0].sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
    queueCreateInfo[0].pNext = NULL;
    queueCreateInfo[0].queueFamilyIndex = 0;//demo->graphics_queue_family_index;
    queueCreateInfo[0].queueCount = 1;
    queueCreateInfo[0].pQueuePriorities = queue_priorities;
    queueCreateInfo[0].flags = 0;

    VkDeviceCreateInfo deviceInfo = {0};
    deviceInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
    deviceInfo.pNext = NULL;
    deviceInfo.queueCreateInfoCount = 1;
    deviceInfo.pQueueCreateInfos = queueCreateInfo;
    deviceInfo.enabledLayerCount = 0;
    deviceInfo.ppEnabledLayerNames = NULL;
    deviceInfo.enabledExtensionCount = 0;//demo->enabled_extension_count,
    deviceInfo.ppEnabledExtensionNames = NULL;//(const char *const *)demo->extension_names,
    deviceInfo.pEnabledFeatures = NULL;// If specific features are required, pass them in here

    /*if (demo->separate_present_queue)
	{
        queueCreateInfo[1].sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
        queueCreateInfo[1].pNext = NULL;
        queueCreateInfo[1].queueFamilyIndex = demo->present_queue_family_index;
        queueCreateInfo[1].queueCount = 1;
        queueCreateInfo[1].pQueuePriorities = queue_priorities;
        queueCreateInfo[1].flags = 0;
        device.queueCreateInfoCount = 2;
    }*/
	if (vkCreateDevice(handle->physicalDevice, &deviceInfo, NULL, &handle->device) != VK_SUCCESS) return 0;

	return 1;
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_Device_Dispose(Device* handle)
{
	if (handle->device != NULL)
	{
		vkDeviceWaitIdle(handle->device);
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
		
}