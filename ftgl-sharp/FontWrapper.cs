//
// FontWrapper.cs
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
using System.IO;
using System.Collections.Generic;

namespace FTGL
{
	public enum FontKind
	{
		Pixmap,
		Texture
	}

	public class FontWrapper : IDisposable
	{
		class FontEntry
		{
			public readonly IntPtr Handle;
			public int ReferenceCount;

			public FontEntry(IntPtr h)
			{
				Handle = h;
				ReferenceCount = 1;
			}
		}

		static Dictionary<int, FontEntry> fontCache = new Dictionary<int, FontEntry>();
		/// <summary>
		/// FIXME: Make extra font handles/objects for each different font size. 
		/// </summary>
		static Dictionary<int, uint> lastSetSizes = new Dictionary<int, uint>();

		readonly FontKind kind;
		readonly int Hash;
		public override int GetHashCode (){return Hash;}
		readonly IntPtr font;
		readonly bool disp;
		uint RenderSize;

		public uint FontSize
		{
			get{ 
				return RenderSize != 0 ? RenderSize : Fonts.GetFontFaceSize (font);
			}
			set{
				RenderSize= value;
			}
		}

		/// <summary>
		/// Sets the font size. Time intensive!
		/// </summary>
		void SetRenderSize()
		{
			if (lastSetSizes[Hash] != RenderSize) {
				Fonts.SetFontFaceSize (font, RenderSize);
				lastSetSizes[Hash] = RenderSize;
			}
		}

		public float LineHeight
		{
			get{
				SetRenderSize ();
				return Fonts.GetFontLineHeight (font);
			}
		}

		public float GetAdvance(string text)
		{
			SetRenderSize ();
			return Fonts.GetFontAdvance (font, text);
		}

		public FontBoundaries GetBoundaries(string text)
		{
			SetRenderSize ();
			return Fonts.GetFontBoundaryBox (font, text);
		}

		public void Render(string text, RenderMode mode = RenderMode.Front)
		{
			if (text != null) {
				SetRenderSize ();
				Fonts.RenderFont (font, text, mode);
			}
		}

		public static int CalculateHashCode(string fontFace, FontKind kind)
		{
			return fontFace.GetHashCode () + ((byte)kind << 24);
		}

		public static FontWrapper LoadFile(string pathToFont, FontKind kind = FontKind.Texture)
		{
			if (!File.Exists (pathToFont))
				throw new FileNotFoundException (pathToFont + " could not be found!");

			var hash = CalculateHashCode (pathToFont, kind);
			FontEntry fe;
			if (fontCache.TryGetValue (hash, out fe)) {
				fe.ReferenceCount++;
				return new FontWrapper (hash,fe.Handle,kind);
			}

			IntPtr font;

			switch (kind) {
				case FontKind.Pixmap:
					font = Fonts.CreatePixmapFont (pathToFont);
					break;
				default:
				case FontKind.Texture:
					font = Fonts.CreateTextureFont (pathToFont);
					break;
			}

			if (font == IntPtr.Zero)
				throw new FTGLException (IntPtr.Zero);

			fontCache [hash] = new FontEntry(font);
			return new FontWrapper (hash, font, kind);
		}

		private FontWrapper(int hash,IntPtr font, FontKind kind)
		{
			if (!lastSetSizes.ContainsKey (hash))
				lastSetSizes [hash] = 0;

			this.kind = kind;
			this.Hash = hash;
			this.disp = kind != FontKind.Texture;
			this.font = font;
		}

		~FontWrapper()
		{
			Dispose ();
		}

		bool disposed=false;
		public virtual void Dispose ()
		{
			if (!disposed) {
				disposed = true;
				var fe = fontCache [Hash];
				if (--fe.ReferenceCount <= 0) {
					if(disp)
						Fonts.DestroyFont(font);
					fontCache.Remove (Hash);
					lastSetSizes.Remove (Hash);
				}
			}
		}
	}
}
