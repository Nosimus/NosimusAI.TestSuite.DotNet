using System.Text.Json;
using Microsoft.Extensions.Options;
using NosimusAI.Models;
using OpenAI.Chat;

namespace NosimusAI;

public sealed class AITestRunner
{
    private readonly ChatClient _chatClient;

    public AITestRunner(IOptions<NosimusSettings> settings)
    {
        _chatClient = new(model: settings.Value.OpenAiModel, apiKey: settings.Value.OpenAiKey);
    }

    public async Task<TestResult> RunTest(string testPrompt, CancellationToken ct)
    {
        List<ChatMessage> messages =
        [
            new UserChatMessage(testPrompt),
        ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "test_result",
                jsonSchema: BinaryData.FromBytes("""
                                                 {
                                                     "type": "object",
                                                     "properties": {
                                                         "passed": { "type": "boolean" },
                                                         "problemExplanation": {"type": "string" },
                                                         "fixSuggestion": {"type": "string" }
                                                     },
                                                     "required": ["passed", "problemExplanation", "fixSuggestion"],
                                                     "additionalProperties": false
                                                 }
                                                 """u8.ToArray()),
                jsonSchemaIsStrict: true)
        };

        ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options, ct);

        var resultString = completion.Content[0].Text;
        return JsonSerializer.Deserialize<TestResult>(resultString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}