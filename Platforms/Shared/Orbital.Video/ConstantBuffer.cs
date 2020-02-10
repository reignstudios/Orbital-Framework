using System;
using Orbital.Numerics;

namespace Orbital.Video
{
	public enum ConstantBufferMode
	{
		/// <summary>
		/// Memory will be optimized for GPU only use
		/// </summary>
		GPUOptimized,

		/// <summary>
		/// Memory will be frequently written to by CPU
		/// </summary>
		Write,

		/// <summary>
		/// Memory will be frequently read from the CPU
		/// </summary>
		Read
	}

	public abstract class ConstantBufferBase : IDisposable
	{
		public readonly DeviceBase device;

		/// <summary>
		/// Size of the buffer with alignment padding
		/// </summary>
		public int size { get; protected set; }

		public ConstantBufferBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		/// <summary>
		/// Open command buffer for updates
		/// </summary>
		/// <returns>True if successful</returns>
		public abstract bool BeginUpdate();

		/// <summary>
		/// Close command buffer from updates
		/// </summary>
		public abstract void EndUpdate();

		#if CS_7_3
		public abstract void Update<T>(T data) where T : unmanaged;
		public abstract void Update<T>(T data, int offset) where T : unmanaged;
		public abstract void Update<T>(T[] data, int offset) where T : unmanaged;
		public abstract void Update<T>(T data, ShaderEffectVariableMapping variable) where T : unmanaged;
		public abstract void Update<T>(T[] data, ShaderEffectVariableMapping variable) where T : unmanaged;
		#else
		public abstract void Update<T>(T data) where T : struct;
		public abstract void Update<T>(T data, int offset) where T : struct;
		public abstract void Update<T>(T[] data, int offset) where T : struct;
		#endif

		public unsafe abstract void Update(void* data, int dataSize, int offset);

		public abstract void Update(float value, int offset);
		public abstract void Update(Vec2 vector, int offset);
		public abstract void Update(Vec3 vector, int offset);
		public abstract void Update(Vec4 vector, int offset);

		public abstract void Update(Mat2 matrix, int offset);
		public abstract void Update(Mat2x3 matrix, int offset);
		public abstract void Update(Mat3 matrix, int offset);
		public abstract void Update(Mat3x2 matrix, int offset);
		public abstract void Update(Mat4 matrix, int offset);

		public abstract void Update(Quat quaternion, int offset);

		/// <summary>
		/// Writes color as float4
		/// </summary>
		public abstract void Update(Color4 color, int offset);

		public abstract void Update(float value, ShaderEffectVariableMapping variable);
		public abstract void Update(Vec2 vector, ShaderEffectVariableMapping variable);
		public abstract void Update(Vec3 vector, ShaderEffectVariableMapping variable);
		public abstract void Update(Vec4 vector, ShaderEffectVariableMapping variable);

		public abstract void Update(Mat2 matrix, ShaderEffectVariableMapping variable);
		public abstract void Update(Mat2x3 matrix, ShaderEffectVariableMapping variable);
		public abstract void Update(Mat3 matrix, ShaderEffectVariableMapping variable);
		public abstract void Update(Mat3x2 matrix, ShaderEffectVariableMapping variable);
		public abstract void Update(Mat4 matrix, ShaderEffectVariableMapping variable);

		public abstract void Update(Quat quaternion, ShaderEffectVariableMapping variable);

		/// <summary>
		/// Writes color as float4
		/// </summary>
		public abstract void Update(Color4 color, ShaderEffectVariableMapping variable);

		public abstract void Update(float[] values, int offset);
		public abstract void Update(Vec2[] vectors, int offset);
		public abstract void Update(Vec3[] vectors, int offset);
		public abstract void Update(Vec4[] vectors, int offset);

		public abstract void Update(Mat2[] matrices, int offset);
		public abstract void Update(Mat2x3[] matrices, int offset);
		public abstract void Update(Mat3[] matrices, int offset);
		public abstract void Update(Mat3x2[] matrices, int offset);
		public abstract void Update(Mat4[] matrices, int offset);

		public abstract void Update(Quat[] quaternions, int offset);

		/// <summary>
		/// Writes color as float4
		/// </summary>
		public abstract void Update(Color4[] colors, int offset);

		public abstract void Update(float[] values, ShaderEffectVariableMapping variable);
		public abstract void Update(Vec2[] vectors, ShaderEffectVariableMapping variable);
		public abstract void Update(Vec3[] vectors, ShaderEffectVariableMapping variable);
		public abstract void Update(Vec4[] vectors, ShaderEffectVariableMapping variable);

		public abstract void Update(Mat2[] matrices, ShaderEffectVariableMapping variable);
		public abstract void Update(Mat2x3[] matrices, ShaderEffectVariableMapping variable);
		public abstract void Update(Mat3[] matrices, ShaderEffectVariableMapping variable);
		public abstract void Update(Mat3x2[] matrices, ShaderEffectVariableMapping variable);
		public abstract void Update(Mat4[] matrices, ShaderEffectVariableMapping variable);

		public abstract void Update(Quat[] quaternions, ShaderEffectVariableMapping variable);

		/// <summary>
		/// Writes color as float4
		/// </summary>
		public abstract void Update(Color4[] colors, ShaderEffectVariableMapping variable);
	}
}
