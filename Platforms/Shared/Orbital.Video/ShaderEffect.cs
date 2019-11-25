using System;
using System.IO;
using Orbital.IO;

namespace Orbital.Video
{
	public abstract class ShaderEffectBase : IDisposable
	{
		public abstract void Dispose();

		public bool Init(Stream stream)
		{
			var reader = new StreamBinaryReader(stream);
			int shaderCount = stream.ReadByte();
			for (int i = 0; i != shaderCount; ++i)
			{
				// read shader type
				var type = (ShaderType)stream.ReadByte();

				// read shader data
				int shaderSize = reader.ReadInt32();
				var shaderData = new byte[shaderSize];
				int read = stream.Read(shaderData, 0, shaderSize);
				if (read < shaderSize) throw new Exception("End of file reached");

				// create shader
				if (!CreateShader(shaderData, type)) return false;
			}

			return true;
		}

		protected abstract bool CreateShader(byte[] data, ShaderType type);
	}
}
