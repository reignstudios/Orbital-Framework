using System;

namespace Orbital.Video
{
	public struct ComputeStateDesc
	{
		/// <summary>
		/// Compute shader to execute
		/// </summary>
		public ComputeShaderBase computeShader;

		/// <summary>
		/// Constant buffers to be accessed in compute shader
		/// </summary>
		public ConstantBufferBase[] constantBuffers;

		/// <summary>
		/// Textures to be accessed in compute shader
		/// </summary>
		public TextureBase[] textures;

		/// <summary>
		/// Depth-Stencils to use as texture resources.
		/// NOTE: Register indicies will come after 'textures'.
		/// </summary>
		public DepthStencilBase[] textureDepthStencils;

		/// <summary>
		/// Random access buffers to be accessed in compute shader
		/// </summary>
		public object[] randomAccessBuffers;
	}

	public abstract class ComputeStateBase : IDisposable
	{
		public readonly DeviceBase device;

		public ComputeStateBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		protected void InitBase(ref ComputeStateDesc desc)
		{
			int constantBufferCount = desc.constantBuffers != null ? desc.constantBuffers.Length : 0;
			if (desc.computeShader.constantBufferCount != constantBufferCount) throw new ArgumentException("ComputeStateDesc constant-buffer count doesn't match ComputeShader requirements");

			int textureCount = desc.textures != null ? desc.textures.Length : 0;
			if (desc.computeShader.textureCount != textureCount) throw new ArgumentException("ComputeStateDesc texture count doesn't match ComputeShader requirements");

			int randomAccessBufferCount = desc.randomAccessBuffers != null ? desc.randomAccessBuffers.Length : 0;
			if (desc.computeShader.randomAccessBufferCount != randomAccessBufferCount) throw new ArgumentException("ComputeStateDesc random access buffer count doesn't match ComputeShader requirements");
		}
	}
}
