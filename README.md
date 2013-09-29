FTGL-sharp
==========

FreeType GL interface/wrapper for .Net/Mono.
FTGL is used for rendering truetype fonts in OpenGL contexts.

Use the FontWrapper class as following:

	// ... while initializing
	var font = FontWrapper.LoadFile("..../myFont.ttf");
	font.FontSize = 20; // * 1/72 inch font size (iirc)
	
	// ... while drawing
	GL.Translate(textX, textY, textZ); // (optional) set text render origin
	font.Render("MyText");
	
	// ... while exiting
	font.Dispose();

To get the text's final width, call
	font.GetAdvance("MyText");

Same goes for a line's height:
	font.LineHeight;

The library isn't complete at all, but the main functionality is working already!
