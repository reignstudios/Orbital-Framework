using size_t = System.IntPtr;
using MirDisplayConfig = System.IntPtr;
using MirOutput = System.IntPtr;
using MirOutputMode = System.IntPtr;

namespace Orbital.Host.Mir
{
	public struct DisplayEx
	{
		public Display display;
		public double refreshRate;
		public MirClient.MirPixelFormat[] formats;
	}

	public unsafe static class Displays
	{
		public static int GetPixelFormatByteCount(MirClient.MirPixelFormat format)
		{
			switch (format)
			{
				case MirClient.MirPixelFormat.mir_pixel_format_abgr_8888:
				case MirClient.MirPixelFormat.mir_pixel_format_xbgr_8888:
				case MirClient.MirPixelFormat.mir_pixel_format_argb_8888:
				case MirClient.MirPixelFormat.mir_pixel_format_xrgb_8888:
					return 4;

				case MirClient.MirPixelFormat.mir_pixel_format_bgr_888:
				case MirClient.MirPixelFormat.mir_pixel_format_rgb_888:
					return 3;

				case MirClient.MirPixelFormat.mir_pixel_format_rgb_565:
				case MirClient.MirPixelFormat.mir_pixel_format_rgba_5551:
				case MirClient.MirPixelFormat.mir_pixel_format_rgba_4444:
					return 2;
			}
			return 0;
		}

		public static MirOutput FindPrimaryOutput(MirDisplayConfig displayConfig)
		{
			int displayCount = MirClient.mir_display_config_get_num_outputs(displayConfig);
			for (int i = 0; i < displayCount; i++)
			{
				MirOutput output = MirClient.mir_display_config_get_output(displayConfig, (size_t)i);
				MirClient.MirOutputConnectionState state = MirClient.mir_output_get_connection_state(output);
				if (state == MirClient.MirOutputConnectionState.mir_output_connection_state_connected && MirClient.mir_output_is_enabled(output) != 0)
				{
					return output;
				}
			}
			return MirOutput.Zero;
		}

		public static Display GetPrimaryDisplay()
		{
			Display result;
			result.isPrimary = true;

			MirDisplayConfig displayConfig = MirClient.mir_connection_create_display_configuration(Application.connection);
			try
			{
				// get display output
				MirOutput output = FindPrimaryOutput(displayConfig);
				if (output == MirOutput.Zero) throw new Exception("No active outputs found");

				// get display size
				MirOutputMode mode = MirClient.mir_output_get_current_mode(output);
				result.width = MirClient.mir_output_mode_get_width(mode);
				result.height = MirClient.mir_output_mode_get_height(mode);
			}
			finally
			{
				MirClient.mir_display_config_release(displayConfig);
			}

			return result;
		}

		public static Display[] GetDisplays()
		{
			Display[] displays;
			MirDisplayConfig displayConfig = MirClient.mir_connection_create_display_configuration(Application.connection);
			try
			{
				int displayCount = MirClient.mir_display_config_get_num_outputs(displayConfig);
				displays = new Display[displayCount];
				bool primarySet = false;
				for (int i = 0; i < displayCount; i++)
				{
					// get output
					MirOutput output = MirClient.mir_display_config_get_output(displayConfig, (size_t)i);
					if (output == MirOutput.Zero) continue;

					// validate display is active
					MirClient.MirOutputConnectionState state = MirClient.mir_output_get_connection_state(output);
					if (state == MirClient.MirOutputConnectionState.mir_output_connection_state_connected && MirClient.mir_output_is_enabled(output) != 0) continue;

					// is primary
					if (!primarySet)
					{
						primarySet = true;
						displays[i].isPrimary = true;
					}
					else
					{
						displays[i].isPrimary = false;
					}

					// get size
					MirOutputMode mode = MirClient.mir_output_get_current_mode(output);
					displays[i].width = MirClient.mir_output_mode_get_width(mode);
					displays[i].height = MirClient.mir_output_mode_get_height(mode);
				}
			}
			finally
			{
				MirClient.mir_display_config_release(displayConfig);
			}

			return displays;
		}

		public static DisplayEx GetPrimaryDisplayEx()
		{
			DisplayEx result;
			result.display.isPrimary = true;

			MirDisplayConfig displayConfig = MirClient.mir_connection_create_display_configuration(Application.connection);
			try
			{
				// get display output
				MirOutput output = FindPrimaryOutput(displayConfig);
				if (output == MirOutput.Zero) throw new Exception("No active outputs found");

				// validate RGBA8 format exists
				int pixelFormatCount = MirClient.mir_output_get_num_pixel_formats(output);
				result.formats = new MirClient.MirPixelFormat[pixelFormatCount];
				for (int i = 0; i < pixelFormatCount; i++)
				{
					result.formats[i] = MirClient.mir_output_get_pixel_format(output, (size_t)i);
				}

				// get display size & refresh rate
				MirOutputMode mode = MirClient.mir_output_get_current_mode(output);
				result.display.width = MirClient.mir_output_mode_get_width(mode);
				result.display.height = MirClient.mir_output_mode_get_height(mode);
				result.refreshRate = MirClient.mir_output_mode_get_refresh_rate(mode);
			}
			finally
			{
				MirClient.mir_display_config_release(displayConfig);
			}

			return result;
		}

		public static DisplayEx[] GetDisplaysEx()
		{
			DisplayEx[] displays;
			MirDisplayConfig displayConfig = MirClient.mir_connection_create_display_configuration(Application.connection);
			try
			{
				int displayCount = MirClient.mir_display_config_get_num_outputs(displayConfig);
				displays = new DisplayEx[displayCount];
				bool primarySet = false;
				for (int i = 0; i < displayCount; i++)
				{
					// get output
					MirOutput output = MirClient.mir_display_config_get_output(displayConfig, (size_t)i);
					if (output == MirOutput.Zero) continue;

					// validate display is active
					MirClient.MirOutputConnectionState state = MirClient.mir_output_get_connection_state(output);
					if (state == MirClient.MirOutputConnectionState.mir_output_connection_state_connected && MirClient.mir_output_is_enabled(output) != 0) continue;

					// is primary
					if (!primarySet)
					{
						primarySet = true;
						displays[i].display.isPrimary = true;
					}
					else
					{
						displays[i].display.isPrimary = false;
					}

					// validate RGBA8 format exists
					int pixelFormatCount = MirClient.mir_output_get_num_pixel_formats(output);
					displays[i].formats = new MirClient.MirPixelFormat[pixelFormatCount];
					for (int f = 0; f < pixelFormatCount; f++)
					{
						displays[i].formats[f] = MirClient.mir_output_get_pixel_format(output, (size_t)f);
					}

					// get size & refresh rate
					MirOutputMode mode = MirClient.mir_output_get_current_mode(output);
					displays[i].display.width = MirClient.mir_output_mode_get_width(mode);
					displays[i].display.height = MirClient.mir_output_mode_get_height(mode);
					displays[i].refreshRate = MirClient.mir_output_mode_get_refresh_rate(mode);
				}
			}
			finally
			{
				MirClient.mir_display_config_release(displayConfig);
			}

			return displays;
		}
	}
}
