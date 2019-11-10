#include "Device.h"
//#include "CommandBuffer.h"

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
	// get native feature level
	FeatureLevel nativeMinFeatureLevel;
	if (!FeatureLevelToNative(minimumFeatureLevel, &nativeMinFeatureLevel)) return 0;
	// TODO: get what max feature level actually is

	// setup info objects
	VkApplicationInfo appInfo =
	{
		.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO,
		.pNext = NULL,
		.pApplicationName = "Orbital",
		.applicationVersion = 0,
		.pEngineName = "Orbital.Video.Vulkan",
		.engineVersion = 0,
		.apiVersion = nativeMinFeatureLevel
	};

	uint32_t extCount = 0;
	char* extension_names[1] = {0};
	//extension_names[0] = VK_KHR_SURFACE_EXTENSION_NAME;

	VkInstanceCreateInfo createInfo =
	{
		.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
		.pNext = NULL,
		.pApplicationInfo = &appInfo,
		.enabledLayerCount = 0,
		.ppEnabledLayerNames = NULL,
		.enabledExtensionCount = extCount,
		.ppEnabledExtensionNames = extension_names,
	};

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
	uint32_t gpu_count;

    VkResult result = vkCreateInstance(&createInfo, NULL, &handle->instance);
    if (result == VK_ERROR_INCOMPATIBLE_DRIVER)
	{
		return 0;
        /*ERR_EXIT(
            "Cannot find a compatible Vulkan installable client driver (ICD).\n\n"
            "Please look at the Getting Started guide for additional information.\n",
            "vkCreateInstance Failure");*/
    }
	else if (result == VK_ERROR_EXTENSION_NOT_PRESENT)
	{
		return 0;
        /*ERR_EXIT(
            "Cannot find a specified extension library.\n"
            "Make sure your layers path is set appropriately.\n",
            "vkCreateInstance Failure");*/
    }
	else if (result)
	{
		return 0;
       /* ERR_EXIT(
            "vkCreateInstance failed.\n\n"
            "Do you have a compatible Vulkan installable client driver (ICD) installed?\n"
            "Please look at the Getting Started guide for additional information.\n",
            "vkCreateInstance Failure");*/
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