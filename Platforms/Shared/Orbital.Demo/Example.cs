using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using Orbital.Host;
using Orbital.Numerics;
using Orbital.Video;

namespace Orbital.Demo
{
	public sealed partial class Example : IDisposable
	{
		private WindowBase window;

		public Example(WindowBase window)
		{
			this.window = window;
		}

		public void Init(string platformPath, string folder64Bit, string folder32Bit)
		{
			// pre-load native libs
			string libFolderBit;
			if (IntPtr.Size == 8) libFolderBit = folder64Bit;
			else if (IntPtr.Size == 4) libFolderBit = folder32Bit;
			else throw new NotSupportedException("Unsupported bit size: " + IntPtr.Size.ToString());

			#if RELEASE
			const string config = "Release";
			#else
			const string config = "Debug";
			#endif

			InitVideo(platformPath, libFolderBit, config);
			InitInput(platformPath, libFolderBit, config);
		}

		public void Dispose()
		{
			DisposeVideo();
			DisposeInput();
		}
		
		public void Run()
		{
			// get window size and viewport
			var windowSize = window.GetSize();
			var viewPort = new ViewPort(new Rect2(0, 0, windowSize.width, windowSize.height));

			// update input
			UpdateInput();

			// update camera
			camera.position = new Vec3(MathF.Cos(rot), .5f, MathF.Sin(rot)) * 3;
			camera.aspect = viewPort.GetAspect();
			camera.LookAt(Vec3.zero);

			// update constant buffer
			if (!constantBuffer.BeginUpdate(videoDevice.swapChain)) throw new Exception("Failed to update ConstantBuffer");
			constantBuffer.Update(MathF.Abs(MathF.Cos(rot * .5f)), shaderEffectVar_Constrast);
			constantBuffer.Update(camera.matrix, shaderEffectVar_Camera);
			constantBuffer.EndUpdate();
			rot += 0.01f;

			// render frame and present
			videoDevice.BeginFrame();

			commandList.Start(videoDevice.swapChain);// Render Triangle into RenderTexture
			commandList.BeginRenderPass(renderTextureTest.renderPass);
			commandList.SetViewPort(new ViewPort(0, 0, renderTextureTest.renderTexture.width, renderTextureTest.renderTexture.height));
			commandList.SetRenderState(renderTextureTest.renderState);
			commandList.Draw();
			commandList.EndRenderPass();
			commandList.Finish();
			commandList.Execute();

			commandList_Compute.Start(videoDevice.swapChain);// Execute compute shader
			commandList_Compute.SetComputeState(computeState);
			commandList_Compute.ExecuteComputeShader(renderTextureTest.renderTexture.width / 8, renderTextureTest.renderTexture.height / 8, 1);
			commandList_Compute.Finish();
			commandList_Compute.Execute();

			commandList.Start(videoDevice.swapChain);// Render Cube into MSAA RenderTexture
			commandList.BeginRenderPass(renderPass);
			commandList.SetViewPort(viewPort);
			commandList.SetRenderState(renderState);
			commandList.Draw();
			commandList.EndRenderPass();
			commandList.Finish();
			commandList.Execute();

			// copy render-texture into swap-chain surface
			if (renderTextureMSAA.msaaLevel == MSAALevel.Disabled) videoDevice.swapChain.CopyTexture(renderTextureMSAA);
			else videoDevice.swapChain.ResolveMSAA(renderTextureMSAA);
			videoDevice.EndFrame();
		}

		private void Log(string message)
		{
			Debug.WriteLine(message);
			Console.WriteLine(message);
		}
	}
}