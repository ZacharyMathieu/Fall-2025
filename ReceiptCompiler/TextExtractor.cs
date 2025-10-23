using Tesseract;

public class TextExtractor
{
    public static async Task<string> ExtractTextFromImage(string imagePath, string tessdataPath, string? outputName = null)
    {
        if (outputName == null)
        {
            outputName = Path.GetFileNameWithoutExtension(imagePath) + ".extracted.txt";
        }
        try
        {
            var img = Pix.LoadFromFile(imagePath).ConvertRGBToGray();

            using (var engine = new TesseractEngine(tessdataPath, "fra", EngineMode.Default))
            {
                var page = engine.Process(img, PageSegMode.SingleBlock);
                var text = page.GetText();
                await Util.SaveOutput(text, outputName);
                return text;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OCR:");
            Console.WriteLine(ex.Message);
            return string.Empty;
        }
    }
}