using System;
using System.Windows.Forms;
using System.Drawing;
using Orbital.Numerics;

namespace Orbital.Host.WinForms
{
	public sealed class Window : WindowBase
	{
		public readonly Form form;

		public Window(Form form)
		{
			this.form = form;
		}

		public Window(Size2 size, WindowType type, WindowStartupPosition startupPosition)
		{
			form = new Form();
			Init(size.width, size.height, type, startupPosition);
		}

		public Window(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			form = new Form();
			Init(width, height, type, startupPosition);
		}

		private void Init(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			// set form type
			switch (type)
			{
				case WindowType.Tool:
					form.MaximizeBox = false;
					form.MinimizeBox = false;
					form.FormBorderStyle = FormBorderStyle.FixedSingle;
					break;

				case WindowType.Borderless:
					form.MaximizeBox = false;
					form.MinimizeBox = false;
					form.FormBorderStyle = FormBorderStyle.None;
					break;

				case WindowType.Fullscreen:
					form.MaximizeBox = false;
					form.MinimizeBox = false;
					form.FormBorderStyle = FormBorderStyle.None;
					var display = Displays.GetPrimaryDisplay();
					width = display.width;
					height = display.height;
					break;
			}

			// set form size
			form.ClientSize = new Size(width, height);

			// set form startup position
			if (type == WindowType.Fullscreen)
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = 0;
				form.Top = 0;
			}
			else
			{
				switch (startupPosition)
				{
					case WindowStartupPosition.CenterScreen:
					form.StartPosition = FormStartPosition.CenterScreen;
						break;
				}
			}
		}

		public override void Dispose()
		{
			Close();
		}

		public override IntPtr GetHandle()
		{
			return form.Handle;
		}

		public override object GetManagedHandle()
		{
			return form;
		}

		public override void SetTitle(string title)
		{
			form.Text = title;
		}

		public override void Show()
		{
			form.Show();
		}

		public override void Close()
		{
			form.Close();
		}

		public override bool IsClosed()
		{
			return form.IsDisposed;
		}

		public override Size2 GetSize()
		{
			var size = form.ClientSize;
			return new Size2(size.Width, size.Height);
		}
	}
}
