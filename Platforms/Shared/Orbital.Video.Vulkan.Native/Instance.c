#include "Instance.h"

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

ORBITAL_EXPORT Instance* Orbital_Video_Vulkan_Instance_Create()
{
	Instance* result = (Instance*)calloc(1, sizeof(Instance));
	return result;
}

#ifdef _DEBUG
VKAPI_ATTR VkBool32 VKAPI_CALL debug_messenger_callback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT *pCallbackData, void *pUserData)
{
	// TODO: log message
	return 1;
}
#endif

ORBITAL_EXPORT int Orbital_Video_Vulkan_Instance_Init(Instance* handle, FeatureLevel minimumFeatureLevel)
{
	// get native feature level
	if (!FeatureLevelToNative(minimumFeatureLevel, &handle->nativeMinFeatureLevel)) return 0;
	
	// init max feature level
	handle->nativeMaxFeatureLevel = VK_API_VERSION_1_1;
	while (1)// loop until we find max api version avaliable
	{
		// setup info objects
		VkApplicationInfo appInfo = {0};
		appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
		appInfo.pNext = NULL;
		appInfo.pApplicationName = "Orbital";
		appInfo.applicationVersion = 0;
		appInfo.pEngineName = "Orbital.Video.Vulkan";
		appInfo.engineVersion = 0;
		appInfo.apiVersion = handle->nativeMaxFeatureLevel;

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
		if (handle->nativeMaxFeatureLevel >= VK_API_VERSION_1_1)
		{
			VkDebugUtilsMessengerCreateInfoEXT dbg_messenger_create_info = {0};
			dbg_messenger_create_info.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
			dbg_messenger_create_info.pNext = NULL;
			dbg_messenger_create_info.flags = 0;
			dbg_messenger_create_info.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
			dbg_messenger_create_info.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
			dbg_messenger_create_info.pfnUserCallback = debug_messenger_callback;
			dbg_messenger_create_info.pUserData = handle;
			createInfo.pNext = &dbg_messenger_create_info;
		}
		#endif

		// try to init sdk instance
		if (vkCreateInstance(&createInfo, NULL, &handle->instance) == VK_SUCCESS) return 1;

		// if sdk instance failed to init, try older version
		handle->instance = NULL;
		if (handle->nativeMaxFeatureLevel == VK_API_VERSION_1_1)
		{
			handle->nativeMaxFeatureLevel = VK_API_VERSION_1_0;
			continue;
		}
		
		break;
	}

	return 0;
}

ORBITAL_EXPORT void Orbital_Video_Vulkan_Instance_Dispose(Instance* handle)
{
	vkDestroyInstance(handle->instance, NULL);
	free(handle);
}