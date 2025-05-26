using System.Diagnostics.CodeAnalysis;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.OS;

namespace Maui.PDFView.Platforms.Android.Common;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public readonly struct PdfAsBitmaps(Func<ParcelFileDescriptor?>? fileDescriptor, ScreenHelper screen, Thickness? crop)
{
    public PdfAsBitmaps(string? fileName, ScreenHelper screen, Thickness? crop)
        : this(
            () => FileNameToDescriptor(fileName),
            screen,
            crop
        )
    {
    }
    
    public List<Bitmap> ToList()
    {
        var file = fileDescriptor?.Invoke();
        if (file == null)
        {
            return new List<Bitmap>();
        }
        
        using var renderer = new PdfRenderer(file);
        var pages = new List<Bitmap>();
        for (int i = 0; i < renderer.PageCount; i++)
        {
            var page = renderer.OpenPage(i);
            if (page != null)
            {
                pages.Add(
                    page.RenderTo(screen.PageBitmap(page), crop) // Apply the crop to the bitmap
                );
                
                page.Close();
            }
        }

        return pages;
    }
    
    private static ParcelFileDescriptor?FileNameToDescriptor(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        return ParcelFileDescriptor.Open(new Java.IO.File(fileName), ParcelFileMode.ReadOnly);
    }
}