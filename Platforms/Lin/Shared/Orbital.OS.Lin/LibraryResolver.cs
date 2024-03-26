using System.Reflection;
using System.Runtime.InteropServices;

namespace Orbital.OS.Lin
{
    public static class LibraryResolver
    {
        public static void Init(Assembly assembly)
        {
            NativeLibrary.SetDllImportResolver(assembly, ResolveLibrary);
        }
        
        private static IntPtr ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            // check if library name is already version specific
            string ext = Path.GetExtension(libraryName);
            if (ext != ".so") return NativeLibrary.Load(libraryName);
            
            // check if lib just loads without additional work
            try
            {
                var lib = NativeLibrary.Load(libraryName);
                if (lib != IntPtr.Zero) return lib;
            }
            catch {}
            
            // resolve lib path
            string libPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
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
            }
			
            // scan for newest version of library
            var highestVersion = new Version(0, 0, 0);
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
                            highestVersion = new Version(versionNum, 0, 0);
                            highestVersionValue = versionNum.ToString();
                        }
                    }
                }
            }

            // add highest version to library
            libraryName += "." + highestVersionValue;
            
            // finish
            return NativeLibrary.Load(libraryName);
        }
    }
}