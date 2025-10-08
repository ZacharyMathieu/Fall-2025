var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();


var app = builder.Build();

// Serve static files from wwwroot and web folders
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "web")),
    RequestPath = "/web"
});
app.UseRouting();

var files = new HashSet<string>();

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
        files.Add(file.FileName);
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploads);
        var filePath = Path.Combine(uploads, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
    }
    context.Response.Redirect("/");
});

app.MapPost("/analyse", async context =>
{
    context.Response.Redirect("/");
});

app.Run();
