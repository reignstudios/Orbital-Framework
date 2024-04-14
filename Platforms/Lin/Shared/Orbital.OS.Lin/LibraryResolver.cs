using System.Reflection;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace Orbital.OS.Lin
{
    public static class LibraryResolver
    {
        public static Dictionary<string,IntPtr> loadedLibraries { get; private set; }

        public static void Init(Assembly assembly)
        {
            loadedLibraries = new Dictionary<string,IntPtr>();
            NativeLibrary.SetDllImportResolver(assembly, ResolveLibrary);
        }
        
        private static IntPtr ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            // check if library already loaded
            if (loadedLibraries.ContainsKey(libraryName))
			{
				return loadedLibraries[libraryName];
			}

            // check if library name is already version specific
            string ext = Path.GetExtension(libraryName);
            if (ext != ".so")
            {
				IntPtr result = NativeLibrary.Load(libraryName);
                if (result != IntPtr.Zero)
                {
				    loadedLibraries.Add(libraryName, result);
                    return result;
                }
			}
            
            // try to load lib without additional work
            try
            {
                var result = NativeLibrary.Load(libraryName);
				if (result != IntPtr.Zero)
				{
					loadedLibraries.Add(libraryName, result);
					return result;
				}
			}
            catch {}
            
            // try to load lib with our full path
            try
            {
                string fullPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), libraryName);
                var result = NativeLibrary.Load(fullPath);
				if (result != IntPtr.Zero)
				{
					loadedLibraries.Add(libraryName, result);
					return result;
				}
			}
            catch {}

			// resolve lib path
			/*string libPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
            if (string.IsNullOrEmpty(libPath))
            {
                if (IntPtr.Size == 8)
                {
                    libPath = "/lib64";
                    if (!Directory.Exists(libPath))
                    {
                        libPath = "/usr/lib64";
                        if (!Directory.Exists(libPath))
                        {
                            libPath = "/lib";
                            if (!Directory.Exists(libPath))
                            {
                                libPath = "/usr/lib";
                                if (!Directory.Exists(libPath))
                                {
                                    return IntPtr.Zero;
                                }
                            }
                        }
                    }
                }
                else
                {
                    libPath = "/lib";
                    if (!Directory.Exists(libPath))
                    {
                        libPath = "/usr/lib";
                        if (!Directory.Exists(libPath))
                        {
                            return IntPtr.Zero;
                        }
                    }
                }
            }*/

			// try to load from system defined path
			string libPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
			if (!string.IsNullOrEmpty(libPath))
			{
                if (ScanForLibNameRecursive(libraryName, libPath, out var result)) return result;
			}
				
			// try to load by Linux standard paths
            if (IntPtr.Size == 8)
			{
				IntPtr result;
				libPath = "/lib64";
				if (Directory.Exists(libPath))
				{
					if (ScanForLibNameRecursive(libraryName, libPath, out result)) return result;

					libPath = "/usr/lib64";
					if (Directory.Exists(libPath))
					{
						if (ScanForLibNameRecursive(libraryName, libPath, out result)) return result;

						libPath = "/lib";
						if (Directory.Exists(libPath))
						{
							if (ScanForLibNameRecursive(libraryName, libPath, out result)) return result;

							libPath = "/usr/lib";
							if (Directory.Exists(libPath))
							{
								if (ScanForLibNameRecursive(libraryName, libPath, out result)) return result;
							}
						}
					}
				}
			}
			else
			{
				IntPtr result;
				libPath = "/lib";
				if (Directory.Exists(libPath))
				{
					if (ScanForLibNameRecursive(libraryName, libPath, out result)) return result;

					libPath = "/usr/lib";
					if (Directory.Exists(libPath))
					{
						if (ScanForLibNameRecursive(libraryName, libPath, out result)) return result;
					}
				}
			}

			// scan for newest version of library
			/*var highestVersion = new Version(0, 0, 0);
            string highestVersionValue = "0";
            var files = Directory.GetFiles(libPath);
            foreach (var file in files)
            {
                string filename = Path.GetFileName(file);
                if (filename.StartsWith(libraryName))
                {
                    ext = Path.GetExtension(filename);
                    if (ext != ".so")
                    {
                        int length = libraryName.Length + 1;
                        string versionValue = filename.Substring(length, filename.Length - length);
                        Version version;
                        int versionNum;
                        if (Version.TryParse(versionValue, out version))
                        {
                            if (version > highestVersion)
                            {
                                highestVersion = version;
                                highestVersionValue = version.ToString();
                            }
                        }
                        else if (int.TryParse(versionValue, out versionNum))
                        {
                            version = new Version(versionNum, 0, 0);
                            if (version > highestVersion)
                            {
                                highestVersion = version;
                                highestVersionValue = versionValue;
                            }
                        }
                    }
                }
            }*/


			/*// load highest version to library
			try
            {
                return NativeLibrary.Load(libraryName + "." + highestVersionValue);
            }
            catch {}*/

            throw new Exception("Failed to load or resolve library: " + libraryName);
        }

        private static bool ScanForLibNameRecursive(string libraryName, string libPath, out IntPtr result)
        {
			// find highest version
			var highestVersion = new Version(0, 0, 0);
			string highestVersionValue = "0";
            bool success = false;
			var files = Directory.GetFiles(libPath);
			foreach (var file in files)
			{
				string filename = Path.GetFileName(file);
				if (filename.StartsWith(libraryName))
				{
					string ext = Path.GetExtension(filename);
					if (ext != ".so")
					{
						int length = libraryName.Length + 1;
						string versionValue = filename.Substring(length, filename.Length - length);
						Version version;
						int versionNum;
						if (Version.TryParse(versionValue, out version))
						{
							success = true;
							if (version > highestVersion)
							{
								highestVersion = version;
								highestVersionValue = version.ToString();
							}
						}
						else if (int.TryParse(versionValue, out versionNum))
						{
							success = true;
							version = new Version(versionNum, 0, 0);
							if (version > highestVersion)
							{
								highestVersion = version;
								highestVersionValue = versionValue;
							}
						}
					}
				}
			}

			// try to load library
			if (success)
			{
				try
				{
					string path = Path.Combine(libPath, libraryName + "." + highestVersionValue);
					result = NativeLibrary.Load(path);
					if (result != IntPtr.Zero)
					{
						loadedLibraries.Add(libraryName, result);
						return true;
					}
				}
				catch { }
			}

			// scan sub-folders
			var folders = Directory.GetDirectories(libPath);
			foreach (var folder in folders)
			{
				if (ScanForLibNameRecursive(libraryName, folder, out result)) return true;
			}

			// fail
			result = IntPtr.Zero;
			return false;
		}
    }
}