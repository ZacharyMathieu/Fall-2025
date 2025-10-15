using Microsoft.Extensions.AI;
using OllamaSharp;

public class ChatModelClient
{
    private readonly IChatClient chatClient;
    private readonly List<ChatMessage> chatHistory;
    private static readonly string port = "http://localhost:11434/";
    private static readonly string model = "llama3.2-vision";

    public ChatModelClient()
    {
        chatClient = new OllamaApiClient(new Uri(port), model);
        chatHistory = new();
    }

    public async Task<string> GetSummaryFromTextAsync(string text)
    {
        var prompt = CreatePrompt(text);
        Console.WriteLine("Prompt:");
        Console.WriteLine(prompt);
        chatHistory.Add(new ChatMessage(ChatRole.User, prompt));

        var response = "";
        try
        {
            await foreach (ChatResponseUpdate item in
                chatClient.GetStreamingResponseAsync(chatHistory))
            {
                response += item.Text;
            }

            Console.WriteLine("Raw Response:");
            Console.WriteLine(response);
            return CleanResponse(response);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error during chat model request: {ex.Message}");
            return string.Empty;
        }
    }

    private static string CreatePrompt(string content)
    {
        return $"[[\n{content}\n]]\nThe section above contains a [[receipt]].\nFind the store's name, the items purchased, their prices and the total and put them in the format:\n[SOURCE: <store name>, ITEMS: <item1> <price1>, <item2> <price2>, ..., TOTAL: <total amount>].\nIf any of this information is missing, leave it out.\nYou MUST include the SOURCE, ITEMS and TOTAL tags in your response.";
    }

    private static string CleanResponse(string response)
    {
        return response.Split("[").Last().Split("]").First().Trim();
    }
}