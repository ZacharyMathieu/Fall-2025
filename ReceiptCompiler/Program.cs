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

var files = new HashSet<string>() { "2.jpg" };

app.MapGet("/", async context =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "web", "main.html");
    var html = await File.ReadAllTextAsync(htmlPath);

    if (files.Count > 0)
    {
        var fileTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "web", "file.html");
        var fileTemplate = await File.ReadAllTextAsync(fileTemplatePath);
        var filesHtml = string.Join("", files.Select(f => fileTemplate.Replace("{{filename}}", System.Net.WebUtility.HtmlEncode(f))));
        html = html.Replace("<span id='fileName'>No file chosen</span>", filesHtml);
    }

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

app.MapPost("/remove", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var filename = form["filename"].ToString();
    if (!string.IsNullOrWhiteSpace(filename) && files.Contains(filename))
    {
        files.Remove(filename);
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
    if (file != null && file.Length > 0 && !files.Contains(file.FileName))
    {
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploads);
        var filePath = Path.Combine(uploads, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        files.Add(file.FileName);
    }
    context.Response.Redirect("/");
});

app.MapPost("/analyse", async context =>
{
    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    foreach (var file in files)
    {
        var filePath = Path.Combine(uploads, file);
        var text = TextExtractor.ExtractTextFromImage(filePath, "tessdata");
        // var text = "WHOLE FOODS\nOrange 20$";

        if (!string.IsNullOrWhiteSpace(text))
        {
            var chatModel = new ChatModelClient();
            var summary = await chatModel.GetSummaryFromTextAsync(text);
            Console.WriteLine("Summary:");
            Console.WriteLine(summary);
        }
    }

    context.Response.Redirect("/");
    await Task.CompletedTask;
});

app.Run();