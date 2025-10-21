class Util
{
    public static async Task SaveOutput(string text, string file)
    {
        var output = Path.Combine(Directory.GetCurrentDirectory(), "output");
        var outputFile = Path.Combine(output, file);
        Directory.CreateDirectory(output);
        await File.WriteAllTextAsync(outputFile, text);
    }
}