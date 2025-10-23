var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "web")),
    RequestPath = "/web"
});
app.UseRouting();

var images = new HashSet<string>() { };
// var images = new HashSet<string>() { "test3.jfif" };

app.MapGet("/", async context =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "web", "main.html");
    var html = await File.ReadAllTextAsync(htmlPath);

    if (images.Count > 0)
    {
        var fileTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "web", "file.html");
        var fileTemplate = await File.ReadAllTextAsync(fileTemplatePath);
        var filesHtml = string.Join("", images.Select(f => fileTemplate.Replace("{{filename}}", System.Net.WebUtility.HtmlEncode(f))));
        html = html.Replace("<span id='fileName'>No file chosen</span>", filesHtml);
    }

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

app.MapPost("/remove", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var filename = form["filename"].ToString();
    if (!string.IsNullOrWhiteSpace(filename) && images.Contains(filename))
    {
        images.Remove(filename);
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        var filePath = Path.Combine(uploads, filename);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    context.Response.Redirect("/");
});

app.MapPost("/upload", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var file = form.Files["image"];
    if (file != null && file.Length > 0 && !images.Contains(file.FileName))
    {
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploads);
        var filePath = Path.Combine(uploads, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        images.Add(file.FileName);
    }
    context.Response.Redirect("/");
});

app.MapPost("/analyse", async context =>
{
    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    var output = Path.Combine(Directory.GetCurrentDirectory(), "output");
    Directory.CreateDirectory(output);
    foreach (var image in images)
    {
        var imagePath = Path.Combine(uploads, image);
        ImageProprocessor.PreprocessImage(imagePath);
        var preprocessedImage = Path.Combine(output, Path.GetFileNameWithoutExtension(imagePath) + ".preprocessed.png");
        var text = await TextExtractor.ExtractTextFromImage(preprocessedImage, "tessdata", Path.GetFileNameWithoutExtension(image) + ".extracted.txt");

        if (!string.IsNullOrWhiteSpace(text))
        {
            var chatModel = new ChatModelClient();
            var summary = await chatModel.GetSummaryFromTextAsync(text, Path.GetFileNameWithoutExtension(image));
        }
        else
        {
            Console.WriteLine($"No text extracted from {image}, skipping analysis.");
        }
    }

    context.Response.Redirect("/");
    await Task.CompletedTask;
});

app.Run();

// var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

// var imagePath = "1.jfif";
// var filePath = Path.Combine(uploads, imagePath);
// ImageProprocessor.PreprocessImage(filePath);
// var preprocessedImage = Path.Combine(uploads, Path.GetFileNameWithoutExtension(imagePath) + ".preprocessed.png");
// var text = await TextExtractor.ExtractTextFromImage(preprocessedImage, "tessdata");
// Console.WriteLine("Extracted Text:");
// Console.WriteLine(text);
