//
// Fonts.cs
//
// Author:
//       Alexander Bothe <info@alexanderbothe.com>
//
// Copyright (c) 2013 Alexander Bothe
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.InteropServices;

namespace FTGL
{
	public enum RenderMode : int
	{
		Front = 0x0001,
		Back  = 0x0002,
		Side  = 0x0004,
		All   = 0xffff
	}

	public enum TextAlignment : int
	{
		Left    = 0,
		Center  = 1,
		Right   = 2,
		Justify = 3
	}

	public class FTGLException : Exception
	{
		public FTGLException(string msg) : base(msg) {}

		public FTGLException(IntPtr font) : base(string.Format("ftgl Exception: {0}", Fonts.GetFontError(font))) {

		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FontBoundaries
	{
		public float Lower;
		public float Left;
		public float Near;

		public float Upper;
		public float Right;
		public float Far;
	}

	public static class Fonts
	{
		private const string nativeLibName = "ftgl.dll";

		/*
		 * To apply custom colors to Pixmap fonts, call the following methods:
		 * glPixelTransferf(GL_RED_BIAS, -1.0f);
		 * glPixelTransferf(GL_GREEN_BIAS, -1.0f);
		 * glPixelTransferf(GL_BLUE_BIAS, -1.0f);
		 */

		/// <summary>
		/// Create a specialised FTGLfont object for handling pixmap (grey scale) fonts.
		/// </summary>
		/// <returns>An FTGLfont* object.</returns>
		/// <param name="pathToTtf">The font file name.</param>
		[DllImport(nativeLibName, EntryPoint = "ftglCreatePixmapFont", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreatePixmapFont([In] [MarshalAs(UnmanagedType.LPStr)] string pathToTtf);
	
		[DllImport(nativeLibName, EntryPoint = "ftglCreateTextureFont", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreateTextureFont([In] [MarshalAs(UnmanagedType.LPStr)] string pathToTtf);


		[DllImport(nativeLibName, EntryPoint = "ftglDestroyFont", CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyFont(IntPtr font);


		/// <summary>
		/// Get the current face size in points (1/72 inch).
		/// </summary>
		[DllImport(nativeLibName, EntryPoint = "ftglGetFontFaceSize", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint GetFontFaceSize(IntPtr font);

		/// <summary>
		/// Gets the line spacing for the font.
		/// </summary>
		[DllImport(nativeLibName, EntryPoint = "ftglGetFontLineHeight", CallingConvention = CallingConvention.Cdecl)]
		public static extern float GetFontLineHeight(IntPtr font);

		[DllImport(nativeLibName, EntryPoint = "ftglRenderFont", CallingConvention = CallingConvention.Cdecl)]
		public static extern void RenderFont(IntPtr font,[In] [MarshalAs(UnmanagedType.LPStr)] string text, RenderMode mode);


		/// <summary>
		/// Set the char size for the current face.
		/// </summary>
		/// <returns>1 if size was set correctly.</returns>
		/// <param name="faceSize">The face size in points (1/72 inch).</param>
		/// <param name="deviceResolution">The resolution of the target device, or 0 to use the default value of 72.</param>
		[DllImport(nativeLibName, EntryPoint = "ftglSetFontFaceSize", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SetFontFaceSize(IntPtr font, uint faceSize, uint deviceResolution=0);


		public static FontBoundaries GetFontBoundaryBox(IntPtr font, string text)
		{
			var bbox=new FontBoundaries();
			if(text != null)
				GetFontBoundaryBox (font, text, text.Length, ref bbox);
			return bbox;
		}

		[DllImport(nativeLibName, EntryPoint = "ftglGetFontBBox", CallingConvention = CallingConvention.Cdecl)]
		public static extern void GetFontBoundaryBox(IntPtr font, 
			[MarshalAs(UnmanagedType.LPStr)] string text, int textLength,
			[MarshalAs(UnmanagedType.Struct)] ref FontBoundaries boundaries
		);

		/// <summary>
		/// Gets the font Boundary box.
		/// </summary>
		/// <param name="font">Font.</param>
		/// <param name="text">Text.</param>
		/// <param name="textLength">Text length.</param>
		/// <param name="boundaries">Boundaries. Must be at least 6 items large!</param>
		[DllImport(nativeLibName, EntryPoint = "ftglGetFontBBox", CallingConvention = CallingConvention.Cdecl)]
		public static extern void GetFontBoundaryBox(IntPtr font, 
			[In] [MarshalAs(UnmanagedType.LPStr)] string text, int textLength, 
			[In] [MarshalAs(UnmanagedType.LPArray, SizeConst = 6)] float[] boundaries
		);

		/// <summary>
		/// Get the advance width for a string.
		/// </summary>
		[DllImport(nativeLibName, EntryPoint = "ftglGetFontAdvance", CallingConvention = CallingConvention.Cdecl)]
		public static extern float GetFontAdvance(IntPtr font, [In] [MarshalAs(UnmanagedType.LPStr)] string text);


		/// <summary>
		/// Set the outset distance for the font.
		/// Only FTOutlineFont, FTPolygonFont and FTExtrudeFont implement front outset. Only FTExtrudeFont implements back outset.
		/// </summary>
		/// <param name="frontOutsetDistance">Front outset distance.</param>
		/// <param name="backOutsetDistance">Back outset distance.</param>
		[DllImport(nativeLibName, EntryPoint = "ftglSetFontOutset", CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetFontOutset(IntPtr font, float frontOutsetDistance, float backOutsetDistance);

		[DllImport(nativeLibName, EntryPoint = "ftglGetFontError", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetFontError(IntPtr font);
	}
}

