using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using UnityEngine;

namespace CursorBounds
{/*
	public class Binding
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left, Top, Right, Bottom;

			public RECT(int left, int top, int right, int bottom)
			{
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}

			public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

			public int X
			{
				get { return Left; }
				set { Right -= (Left - value); Left = value; }
			}

			public int Y
			{
				get { return Top; }
				set { Bottom -= (Top - value); Top = value; }
			}

			public int Height
			{
				get { return Bottom - Top; }
				set { Bottom = value + Top; }
			}

			public int Width
			{
				get { return Right - Left; }
				set { Right = value + Left; }
			}

			public System.Drawing.Point Location
			{
				get { return new System.Drawing.Point(Left, Top); }
				set { X = value.X; Y = value.Y; }
			}

			public System.Drawing.Size Size
			{
				get { return new System.Drawing.Size(Width, Height); }
				set { Width = value.Width; Height = value.Height; }
			}

			public static implicit operator System.Drawing.Rectangle(RECT r)
			{
				return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
			}

			public static implicit operator RECT(System.Drawing.Rectangle r)
			{
				return new RECT(r);
			}

			public static bool operator ==(RECT r1, RECT r2)
			{
				return r1.Equals(r2);
			}

			public static bool operator !=(RECT r1, RECT r2)
			{
				return !r1.Equals(r2);
			}

			public bool Equals(RECT r)
			{
				return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
			}

			public override bool Equals(object obj)
			{
				if (obj is RECT)
					return Equals((RECT)obj);
				else if (obj is System.Drawing.Rectangle)
					return Equals(new RECT((System.Drawing.Rectangle)obj));
				return false;
			}

			public override int GetHashCode()
			{
				return ((System.Drawing.Rectangle)this).GetHashCode();
			}

			public override string ToString()
			{
				return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
			}
		}

		[DllImport("user32.dll")]
		private static extern void ClipCursor(ref Rectangle rect);

		[DllImport("user32.dll")]
		private static extern void ClipCursor(IntPtr rect);

		[DllImport("user32.dll")]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		public static void ClampCursor()
		{
			Debug.LogWarning("Clamp Cursor");

			IntPtr hWnd = Binding.GetActiveWindow();

			Debug.Log(hWnd);

			RECT playRect = new RECT();
			Point pos = new Point(0, 0);

			Debug.Log(playRect);

			Debug.Log(pos);

			Binding.GetClientRect(hWnd, out playRect);
			Binding.ClientToScreen(hWnd, ref pos);

			Debug.Log(playRect);

			Debug.Log(pos);

			Size size = new Size((int)(playRect.Right - playRect.Left) + pos.X, (int)(playRect.Bottom - playRect.Top) + pos.Y);

			Debug.Log(size);

			Rectangle clip = new Rectangle(pos, size);

			Debug.Log(clip);

			Binding.ClipCursor(ref clip);
		}

		public static void ReleaseCursor()
		{
			Debug.LogWarning("Release Cursor");

			Binding.ClipCursor(IntPtr.Zero);
		}
	}
    */
}