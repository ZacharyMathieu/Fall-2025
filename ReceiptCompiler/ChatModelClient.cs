using Microsoft.Extensions.AI;
using OllamaSharp;

public class ChatModelClient
{
    private readonly IChatClient chatClient;
    private readonly List<ChatMessage> chatHistory;
    private static readonly string port = "http://localhost:11434/";
    private static readonly string model = "llama3.1";

    public ChatModelClient()
    {
        chatClient = new OllamaApiClient(new Uri(port), model);
        chatHistory = new();
    }

    public async Task<string> GetSummaryFromTextAsync(string text, string fileName)
    {
        var prompt = CreatePrompt(text);
        chatHistory.Add(new ChatMessage(ChatRole.User, prompt));

        var response = "";
        try
        {
            await foreach (ChatResponseUpdate item in
                chatClient.GetStreamingResponseAsync(chatHistory))
            {
                response += item.Text;
            }

            await Util.SaveOutput(response, $"{fileName}.raw.txt");

            var cleanedResponse = CleanResponse(response);
            await Util.SaveOutput(cleanedResponse, $"{fileName}.cleaned.txt");
            return cleanedResponse;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error during chat model request: {ex.Message}");
            return string.Empty;
        }
    }

    private static string CreatePrompt(string content)
    {
        return $"[[\n{content}\n]]\nThe section above contains a [[receipt]]."
        + "\nFind the store's name, the items purchased, their prices and the total and put them in the format:"
        + "\n[SOURCE: <store name>, ITEMS: <item1> <price1>, <item2> <price2>, ..., TOTAL: <total amount>]."
        + "\nIf any of this information is missing, leave it out.";
    }

    private static string CleanResponse(string response)
    {
        return response.Split("[").Last().Split("]").First().Trim();
    }
}