using Tesseract;

public class TextExtractor
{
    public static async Task<string> ExtractTextFromImage(string imagePath, string tessdataPath)
    {
        try
        {
            using (var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img, region: Rect.FromCoords(500, 150, 1000, 700), PageSegMode.Auto))
                    {
                        var text = page.GetText();
                        await Util.SaveOutput(text, Path.GetFileNameWithoutExtension(imagePath) + ".extracted.txt");
                        return text;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OCR: {ex.Message}");
            return string.Empty;
        }
    }
}