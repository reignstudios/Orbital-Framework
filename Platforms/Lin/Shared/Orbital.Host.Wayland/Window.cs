using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Host.Wayland
{
	public enum WindowContentType
	{
		/// <summary>
		/// The content type none means that either the application has no
		/// data about the content type, or that the content doesn't fit
		/// into one of the other categories.
		/// </summary>
		None,
		
		/// <summary>
		/// The content type photo describes content derived from digital
		/// still pictures and may be presented with minimal processing.
		/// </summary>
		Photo,
		
		/// <summary>
		/// The content type video describes a video or animation and may
		/// be presented with more accurate timing to avoid stutter. Where
		/// scaling is needed, scaling methods more appropriate for video
		/// may be used.
		/// </summary>
		Video,
		
		/// <summary>
		/// The content type game describes a running game. Its content
		/// may be presented with reduced latency.
		/// </summary>
		Game
	}
	
	public unsafe sealed class Window : WindowBase
	{
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern IntPtr Orbital_Host_Wayland_Window_Create(IntPtr app);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern int Orbital_Host_Wayland_Window_Init(IntPtr window, int width, int height, byte* appID, WindowType type, WindowContentType contentTypeType);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Window_Dispose(IntPtr window);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Window_SetTitle(IntPtr window, byte* title);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Window_Show(IntPtr window);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Window_GetSize(IntPtr window, int* width, int* height);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern int Orbital_Host_Wayland_Window_IsClosed(IntPtr window);
		
		internal static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public IntPtr handle { get; private set; }

		public Window(string title, int width, int height, WindowType type, WindowStartupPosition startupPosition, WindowContentType contentType)
		{
			handle = Orbital_Host_Wayland_Window_Create(Application.handle);
			fixed (byte* appIDPtr = Application.appIDData)
			{
				if (Orbital_Host_Wayland_Window_Init(handle, width, height, appIDPtr, type, contentType) == 0)
				{
					throw new Exception("Failed: Orbital_Host_Wayland_Window_Init");
				}
			}

			// set title
			var titleData = Encoding.UTF8.GetBytes(title + "\0");
			fixed (byte* titlePtr = titleData)
			{
				Orbital_Host_Wayland_Window_SetTitle(handle, titlePtr);
			}

			// show
			Orbital_Host_Wayland_Window_Show(handle);

			// track window
			_windows.Add(this);
		}

		public override void Dispose()
		{
			Close();
		}

		public override IntPtr GetHandle()
		{
			return handle;
		}

		public override object GetManagedHandle()
		{
			return this;
		}

		public override void Close()
		{
			_windows.Remove(this);
			if (handle != IntPtr.Zero)
			{
				Orbital_Host_Wayland_Window_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override bool IsClosed()
		{
			return Orbital_Host_Wayland_Window_IsClosed(handle) != 0;
		}

		public override Size2 GetSize()
		{
			Size2 result;
			Orbital_Host_Wayland_Window_GetSize(handle, &result.width, &result.height);
			return result;
		}
	}
}
