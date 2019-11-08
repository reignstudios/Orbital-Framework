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

		public Window(Point2 position, Size2 size, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
		{
			form = new Form();
			Init(position.x, position.y, size.width, size.height, sizeType, type, startupPosition);
		}

		public Window(int x, int y, int width, int height, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
		{
			form = new Form();
			Init(x, y, width, height, sizeType, type, startupPosition);
		}

		private void Init(int x, int y, int width, int height, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
		{
			// set form type
			switch (type)
			{
				case WindowType.Tool:
					form.MaximizeBox = false;
					form.MinimizeBox = false;
					form.FormBorderStyle = FormBorderStyle.FixedSingle;
					break;

				case WindowType.Popup:
					form.MaximizeBox = false;
					form.MinimizeBox = false;
					form.FormBorderStyle = FormBorderStyle.None;
					break;
			}

			// set form size
			SetSize(width, height, sizeType);

			// set form startup position
			switch (startupPosition)
			{
				case WindowStartupPosition.Custom:
					form.StartPosition = FormStartPosition.Manual;
					SetPosition(x, y);
					break;

				case WindowStartupPosition.CenterScreen:
				form.StartPosition = FormStartPosition.CenterScreen;
					break;
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

		public override void SetTitle(string title)
		{
			form.Text = title;
		}

		public override void Show()
		{
			form.Show();
		}

		public override void Hide()
		{
			form.Hide();
		}

		public override void Close()
		{
			form.Close();
		}

		public override bool IsVisible()
		{
			return form.Visible;
		}

		public override bool IsClosed()
		{
			return form.IsDisposed;
		}

		public override Point2 GetPosition()
		{
			var position = form.Location;
			return new Point2(position.X, position.Y);
		}

		public override void SetPosition(Point2 position)
		{
			SetPosition(position.x, position.y);
		}

		public override void SetPosition(int x, int y)
		{
			form.Location = new Point(x, y);
		}

		public override Size2 GetSize(WindowSizeType type)
		{
			if (type == WindowSizeType.WorkingArea)
			{
				var size = form.ClientSize;
				return new Size2(size.Width, size.Height);
			}
			else
			{
				var size = form.Size;
				return new Size2(size.Width, size.Height);
			}
		}

		public override void SetSize(Size2 size, WindowSizeType type)
		{
			SetSize(size.width, size.height, type);
		}

		public override void SetSize(int width, int height, WindowSizeType type)
		{
			if (type == WindowSizeType.WorkingArea) form.ClientSize = new Size(width, height);
			else form.Size = new Size(width, height);
		}
	}
}
