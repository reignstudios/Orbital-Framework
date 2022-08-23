using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace Orbital.Networking.SharedMemory
{
	public enum SharedMemoryMode
	{
		CreateOrOpen,
		Open,
		Create
	}

	public enum SharedMemoryAccess
	{
		ReadWrite,
		Write,
		Read
	}

	public class SharedMemoryBuffer : IDisposable
	{
		private MemoryMappedFile mappedFile;
		public MemoryMappedViewAccessor accessor { get; private set; }

		public SharedMemoryBuffer(string name, int bufferSize, SharedMemoryMode mode, SharedMemoryAccess access)
		{
			MemoryMappedFileAccess fileAccess;
			MemoryMappedFileRights fileRights;
			switch (access)
			{
				case SharedMemoryAccess.ReadWrite:
					fileAccess = MemoryMappedFileAccess.ReadWrite;
					fileRights = MemoryMappedFileRights.ReadWrite;
					break;

				case SharedMemoryAccess.Write:
					fileAccess = MemoryMappedFileAccess.Write;
					fileRights = MemoryMappedFileRights.Write;
					break;

				case SharedMemoryAccess.Read:
					fileAccess = MemoryMappedFileAccess.Read;
					fileRights = MemoryMappedFileRights.Read;
					break;

				default: throw new Exception("Unsupported shared memory access type: " + access);
			}

			switch (mode)
			{
				case SharedMemoryMode.CreateOrOpen: mappedFile = MemoryMappedFile.CreateOrOpen(name, bufferSize, fileAccess); break;
				case SharedMemoryMode.Create: mappedFile = MemoryMappedFile.CreateNew(name, bufferSize, fileAccess); break;
				case SharedMemoryMode.Open: mappedFile = MemoryMappedFile.OpenExisting(name, fileRights); break;
				default: throw new Exception("Unsupported shared memory mode type: " + mode);
			}
			
			accessor = mappedFile.CreateViewAccessor(0, bufferSize, fileAccess);
		}

		public void Dispose()
		{
			if (accessor != null)
			{
				accessor.Dispose();
				accessor = null;
			}

			if (mappedFile != null)
			{
				mappedFile.Dispose();
				mappedFile = null;
			}
		}
	}
}
