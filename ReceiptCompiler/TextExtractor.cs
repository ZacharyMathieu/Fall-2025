using Tesseract;

public class TextExtractor
{
    private static readonly string[] languages = ["eng"];
    // private static readonly string[] languages = ["eng", "fra"];

    public static void ExtractTextFromImage(string imagePath, string tessdataPath)
    {
        foreach (var lang in languages)
        {
            try
            {
                using (var engine = new TesseractEngine(tessdataPath, lang, EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(imagePath))
                    {
                        using (var page = engine.Process(img, region: Rect.FromCoords(500, 150, 1000, 700), PageSegMode.Auto))
                        {
                            SaveOutput(page.GetText(), imagePath, lang);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during OCR: {ex.Message}");
            }
        }
    }

    private static void SaveOutput(string text, string file, string lang)
    {
        var output = Path.Combine(Directory.GetCurrentDirectory(), "output");
        var outputDir = Path.Combine(output, Path.GetFileName(file));
        var outputFile = Path.Combine(outputDir, lang + ".txt");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(outputFile, text);
    }
}