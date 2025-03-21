using SkiaSharp;

namespace NihongoBot.Application.Helpers;

public class KanaRenderer
{

    public static byte[] RenderCharacterToImage(string character)
    {
        int width = 250;
        int height = 250;
        int fontSize = 126;
        if (character.Length > 1)
            fontSize = 100;

        using SKBitmap bitmap = new(width, height);
        using SKCanvas canvas = new(bitmap);
        using SKPaint paint = new()
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        string fontPath = "fonts/NotoSansJP-Regular.ttf";
        // Check if file exists in the bin folder
        string fullFontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fontPath);
        if (!File.Exists(fullFontPath))
        {
            Console.WriteLine("Font file not found in bin folder.");
            return Array.Empty<byte>();
        }
        fontPath = fullFontPath;
        using SKTypeface typeface = SKTypeface.FromFile(fontPath);
        using SKFont font = new()
        {
            Size = fontSize,
            Typeface = typeface
        };

        canvas.Clear(SKColors.White);

        // Measure the string to center it
        SKRect textBounds = new();
        font.MeasureText(character, out textBounds);
        float x = (width - textBounds.Width) / 2 - 10;
        float y = (height + textBounds.Height) / 2;

        canvas.DrawText(character, x, y, SKTextAlign.Left, font, paint);

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
