#include "Device.h"
//#include "CommandBuffer.h"
#include <malloc.h>

char FeatureLevelToNative(FeatureLevel featureLevel, uint32_t* nativeMinFeatureLevel)
{
	switch (featureLevel)
	{
		case FeatureLevel_Level_1_0: *nativeMinFeatureLevel = VK_API_VERSION_1_0; break;
		case FeatureLevel_Level_1_1: *nativeMinFeatureLevel = VK_API_VERSION_1_1; break;
		default: return 0;
	}
	return 1;
}

/*ORBITAL_EXPORT int Orbital_Video_Vulkan_Device_QuerySupportedAdapters(FeatureLevel minimumFeatureLevel, int allowSoftwareAdapters, WCHAR** adapterNames, UINT* adapterNameCount, UINT adapterNameMaxLength)
{
		
	return 1;
}*/

ORBITAL_EXPORT Device* Orbital_Video_Vulkan_Device_Create()
{
	return (Device*)calloc(1, sizeof(Device));
}

VKAPI_ATTR VkBool32 VKAPI_CALL debug_messenger_callback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT *pCallbackData, void *pUserData)
{
	// TODO: log message
	return 1;
}

ORBITAL_EXPORT int Orbital_Video_Vulkan_Device_Init(Device* handle, int adapterIndex, FeatureLevel minimumFeatureLevel, int softwareRasterizer)
{
	// -1 adapter defaults to 0
	if (adapterIndex == -1) adapterIndex = 0;

	// get native feature level
	FeatureLevel nativeMinFeatureLevel;
	if (!FeatureLevelToNative(minimumFeatureLevel, &nativeMinFeatureLevel)) return 0;
	
	// set max feature level
	FeatureLevel nativeMaxFeatureLevel = nativeMinFeatureLevel;
	RE_INIT_WITH_FEATURE_MAX:;// if the device we

	// setup info objects
	VkApplicationInfo appInfo = {0};
	appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
	appInfo.pNext = NULL;
	appInfo.pApplicationName = "Orbital";
	appInfo.applicationVersion = 0;
	appInfo.pEngineName = "Orbital.Video.Vulkan";
	appInfo.engineVersion = 0;
	appInfo.apiVersion = nativeMaxFeatureLevel;

	uint32_t extCount = 0;
	char* extension_names[1] = {0};
	//extension_names[0] = VK_KHR_SURFACE_EXTENSION_NAME;

	VkInstanceCreateInfo createInfo = {0};
	createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
	createInfo.pNext = NULL;
	createInfo.pApplicationInfo = &appInfo;
	createInfo.enabledLayerCount = 0;
	createInfo.ppEnabledLayerNames = NULL;
	createInfo.enabledExtensionCount = extCount;
	createInfo.ppEnabledExtensionNames = extension_names;

	// enable debugging
    #ifdef _DEBUG
	VkDebugUtilsMessengerCreateInfoEXT dbg_messenger_create_info = {0};
    dbg_messenger_create_info.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
    dbg_messenger_create_info.pNext = NULL;
    dbg_messenger_create_info.flags = 0;
    dbg_messenger_create_info.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
    dbg_messenger_create_info.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
    dbg_messenger_create_info.pfnUserCallback = debug_messenger_callback;
    //dbg_messenger_create_info.pUserData = demo;
    createInfo.pNext = &dbg_messenger_create_info;
    #endif

	// init
    VkResult result = vkCreateInstance(&createInfo, NULL, &handle->instance);
    if (result == VK_ERROR_INCOMPATIBLE_DRIVER) return 0;
	else if (result == VK_ERROR_EXTENSION_NOT_PRESENT) return 0;
	else if (result != VK_SUCCESS) return 0;

	if (nativeMaxFeatureLevel >= VK_API_VERSION_1_1)
	{
		uint32_t adapterCount;
		result = vkEnumeratePhysicalDeviceGroups(handle->instance, &adapterCount, NULL);
		if (result != VK_SUCCESS) return 0;
		if (adapterIndex >= adapterCount) return 0;

		VkPhysicalDeviceGroupProperties* adapters = alloca(sizeof(VkPhysicalDeviceGroupProperties) * adapterCount);
        result = vkEnumeratePhysicalDeviceGroups(handle->instance, &adapterCount, adapters);
		if (result != VK_SUCCESS) return 0;
		if (adapters[adapterIndex].physicalDeviceCount <= 0) return 0;
		handle->adapter = adapters[adapterIndex];
		handle->device = handle->adapter.physicalDevices[0];
	}
	else
	{
		uint32_t deviceCount;
		result = vkEnumeratePhysicalDevices(handle->instance, &deviceCount, NULL);
		if (result != VK_SUCCESS) return 0;
		if (adapterIndex >= deviceCount) return 0;

		VkPhysicalDevice* devices = alloca(sizeof(VkPhysicalDevice) * deviceCount);
        result = vkEnumeratePhysicalDevices(handle->instance, &deviceCount, devices);
		if (result != VK_SUCCESS) return 0;
		handle->device = devices[adapterIndex];
	}



	return 1;
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_Device_Dispose(Device* handle)
{
		

	free(handle);
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_Device_BeginFrame(Device* handle)
{
		
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_Device_EndFrame(Device* handle)
{
		
}

/*ORBITAL_EXPORT void Orbital_Video_Vulkan_Device_ExecuteCommandBuffer(Device* handle, CommandBuffer* commandBuffer)
{
		
}*/