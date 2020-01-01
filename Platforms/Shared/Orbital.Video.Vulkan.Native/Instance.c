#include "Instance.h"
#include <stdio.h>

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
	char* prefix = NULL;
	if (messageSeverity & VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT) prefix = "";
    else if (messageSeverity & VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT) prefix = "INFO";
    else if (messageSeverity & VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT) prefix = "WARNING";
    else if (messageSeverity & VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT) prefix = "ERROR";

	char* type = NULL;
	if (messageType & VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT) type = "GENERAL";
	else if (messageType & VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT) type = "VALIDATION";
    else if (messageType & VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT) type = "PERFORMANCE";

	#ifdef _WIN32
	char buffer[1024];
	sprintf_s(buffer, 1024, "Vulkan: %s: %s: %s", prefix, type, pCallbackData->pMessage);
	OutputDebugStringA(buffer);
	#else
	printf("Vulkan: %s: %s: %s", prefix, type, pCallbackData->pMessage);
	#endif
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
		// get supported extensions for instance
		uint32_t extensionPropertiesCount = 0;
		if (vkEnumerateInstanceExtensionProperties(NULL, &extensionPropertiesCount, NULL) != VK_SUCCESS) return 0;

		VkExtensionProperties* extensionProperties = alloca(sizeof(VkExtensionProperties) * extensionPropertiesCount);
		if (vkEnumerateInstanceExtensionProperties(NULL, &extensionPropertiesCount, extensionProperties) != VK_SUCCESS) return 0;

		// make sure device supports required extensions
		uint32_t initExtensionCount = 0;
		char* initExtensions[32];
		#ifdef _WIN32
		uint32_t expectedExtensionCount = 2;
		#endif
		for (uint32_t i = 0; i != extensionPropertiesCount; ++i)
		{
			if
			(
				strcmp(extensionProperties[i].extensionName, VK_KHR_SURFACE_EXTENSION_NAME) != 0
				#ifdef _WIN32
				&& strcmp(extensionProperties[i].extensionName, VK_KHR_WIN32_SURFACE_EXTENSION_NAME) != 0
				#endif
			)
			{
				continue;
			}

			initExtensions[initExtensionCount] = extensionProperties[i].extensionName;
			++initExtensionCount;
		}

		// if there are missing extensions exit
		if (expectedExtensionCount != initExtensionCount) return 0;

		// check for validation layers
		uint32_t validationLayerCount = 0;
		char* validationLayerNames[8] = {0};
		#ifdef _DEBUG
		uint32_t allValidationLayerCount = 0;
		if (vkEnumerateInstanceLayerProperties(&allValidationLayerCount, NULL) != VK_SUCCESS) return 0;
		if (allValidationLayerCount != 0)
		{
			VkLayerProperties *layers = alloca(sizeof(VkLayerProperties) * allValidationLayerCount);
			if (vkEnumerateInstanceLayerProperties(&allValidationLayerCount, layers) != VK_SUCCESS) return 0;
			for (uint32_t i = 0; i != allValidationLayerCount; ++i)
			{
				if
				(
					strcmp(layers[i].layerName, "VK_LAYER_KHRONOS_validation") == 0 ||
					strcmp(layers[i].layerName, "VK_LAYER_LUNARG_standard_validation") == 0
				)
				{
					validationLayerNames[validationLayerCount] = layers[i].layerName;
					++validationLayerCount;
				}
			}
		}
		#endif

		// setup info objects
		VkApplicationInfo appInfo = {0};
		appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
		appInfo.pNext = NULL;
		appInfo.pApplicationName = "Orbital";
		appInfo.applicationVersion = 0;
		appInfo.pEngineName = "Orbital.Video.Vulkan";
		appInfo.engineVersion = 0;
		appInfo.apiVersion = handle->nativeMaxFeatureLevel;

		VkInstanceCreateInfo createInfo = {0};
		createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
		createInfo.pNext = NULL;
		createInfo.pApplicationInfo = &appInfo;
		createInfo.enabledLayerCount = validationLayerCount;
		createInfo.ppEnabledLayerNames = validationLayerNames;
		createInfo.enabledExtensionCount = initExtensionCount;
		createInfo.ppEnabledExtensionNames = initExtensions;

		// enable debugging
		#ifdef _DEBUG
		#ifdef _WIN32
		if (IsDebuggerPresent())
		#endif
		if (handle->nativeMaxFeatureLevel >= VK_API_VERSION_1_1)
		{
			VkDebugUtilsMessengerCreateInfoEXT debugCreateInfo = {0};
			debugCreateInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
			debugCreateInfo.pNext = NULL;
			debugCreateInfo.flags = 0;
			debugCreateInfo.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
			debugCreateInfo.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
			debugCreateInfo.pfnUserCallback = debug_messenger_callback;
			debugCreateInfo.pUserData = handle;
			createInfo.pNext = &debugCreateInfo;
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

ORBITAL_EXPORT int Orbital_Video_Vulkan_Instance_QuerySupportedAdapters(Instance* handle, char** adapterNames, uint32_t adapterNameMaxLength, uint32_t* adapterIndices, uint32_t* adapterCount)
{
	uint32_t maxAdapterCount = *adapterCount;
	*adapterCount = 0;

	// get physical device group if vulkan API version is newer otherwise get physical device only
	if (handle->nativeMaxFeatureLevel >= VK_API_VERSION_1_1)
	{
		// collect all device groups supported
		uint32_t deviceGroupCount;
		if (vkEnumeratePhysicalDeviceGroups(handle->instance, &deviceGroupCount, NULL) != VK_SUCCESS) return 0;

		VkPhysicalDeviceGroupProperties* physicalDeviceGroups = alloca(sizeof(VkPhysicalDeviceGroupProperties) * deviceGroupCount);
		if (vkEnumeratePhysicalDeviceGroups(handle->instance, &deviceGroupCount, physicalDeviceGroups) != VK_SUCCESS) return 0;

		// get properties for all 1st devices in groups
		for (uint32_t i = 0; i != deviceGroupCount; ++i)
		{
			if (physicalDeviceGroups[i].physicalDeviceCount <= 0) continue;
			VkPhysicalDevice physicalDevice = physicalDeviceGroups[i].physicalDevices[0]; 

			VkPhysicalDeviceProperties physicalDeviceProperties = {0};
			vkGetPhysicalDeviceProperties(physicalDevice, &physicalDeviceProperties);
			
			// add name and increase count
			uint32_t maxLength = min(sizeof(char) * adapterNameMaxLength, sizeof(physicalDeviceProperties.deviceName));
			memcpy(adapterNames[(*adapterCount)], physicalDeviceProperties.deviceName, maxLength);
			adapterIndices[(*adapterCount)] = i;
			++(*adapterCount);
		}
	}
	else
	{
		// collect all devices supported
		uint32_t deviceCount;
		if (vkEnumeratePhysicalDevices(handle->instance, &deviceCount, NULL) != VK_SUCCESS) return 0;

		VkPhysicalDevice* physicalDevices = alloca(sizeof(VkPhysicalDevice) * deviceCount);
		if (vkEnumeratePhysicalDevices(handle->instance, &deviceCount, physicalDevices) != VK_SUCCESS) return 0;

		// get properties for all devices
		for (uint32_t i = 0; i != deviceCount; ++i)
		{
			VkPhysicalDeviceProperties physicalDeviceProperties = {0};
			vkGetPhysicalDeviceProperties(physicalDevices[i], &physicalDeviceProperties);

			// add name and increase count
			uint32_t maxLength = min(sizeof(char) * adapterNameMaxLength, sizeof(physicalDeviceProperties.deviceName));
			memcpy(adapterNames[(*adapterCount)], physicalDeviceProperties.deviceName, maxLength);
			adapterIndices[(*adapterCount)] = i;
			++(*adapterCount);
		}
	}

	return 1;
}