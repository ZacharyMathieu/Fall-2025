using Tesseract;

public class TextExtractor
{
    public static async Task<string> ExtractTextFromImage(string imagePath, string tessdataPath, Cluster cluster)
    {
        try
        {
            var img = Pix.LoadFromFile(imagePath).ConvertRGBToGray();
            var engine = new TesseractEngine(tessdataPath, "fra", EngineMode.Default);
            var page = engine.Process(img, cluster.ToRect(), PageSegMode.SingleBlock);
            var text = page.GetText();
            await Util.SaveOutput(text, Path.GetFileNameWithoutExtension(imagePath) + ".extracted.txt");
            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OCR:");
            Console.WriteLine(ex.Message);
            return string.Empty;
        }
    }
}