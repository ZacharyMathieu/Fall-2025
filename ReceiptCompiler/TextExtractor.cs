using Tesseract;

public class TextExtractor
{
    public static string ExtractTextFromImage(string imagePath, string tessdataPath)
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
                        SaveOutput(text, imagePath);
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

    private static void SaveOutput(string text, string file)
    {
        var output = Path.Combine(Directory.GetCurrentDirectory(), "output");
        var outputFile = Path.Combine(output, Path.GetFileNameWithoutExtension(file) + ".extracted.txt");
        Directory.CreateDirectory(output);
        File.WriteAllText(outputFile, text);
    }
}