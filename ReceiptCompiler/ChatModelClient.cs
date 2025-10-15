using Microsoft.Extensions.AI;
using OllamaSharp;

public class ChatModelClient
{
    private readonly IChatClient chatClient;
    private readonly List<ChatMessage> chatHistory;

    public ChatModelClient()
    {
        chatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2:latest");
        chatHistory = new();
    }

    public async Task<string> GetSummaryFromTextAsync(string text)
    {
        var prompt = CreatePrompt(text);
        chatHistory.Add(new ChatMessage(ChatRole.User, prompt));

        var response = "";
        await foreach (ChatResponseUpdate item in
            chatClient.GetStreamingResponseAsync(chatHistory))
        {
            response += item.Text;
        }
        return response;
    }

    private static string CreatePrompt(string content)
    {
        return $"[\n{content}\n] Format the store's name, the items purchased, their prices and the total in the format [SOURCE: <store name>, ITEMS: <item1> <price1>, <item2> <price2>, ..., TOTAL: <total amount>]. If any of this information is missing, leave it out.";
    }
}