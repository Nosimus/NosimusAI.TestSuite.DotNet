using System.Text.Json;
using Microsoft.Extensions.Options;
using NosimusAI.Models;
using OpenAI.Chat;

namespace NosimusAI;

public class BusinessRequirementExtractor
{
    private readonly ChatClient _chatClient;
    
    private readonly CallgraphExtractor _callgraphExtractor;

    public BusinessRequirementExtractor(IOptions<NosimusSettings> settings,
        CallgraphExtractor callgraphExtractor)
    {
        _callgraphExtractor = callgraphExtractor;
        _chatClient = new(model: settings.Value.OpenAiModel, apiKey: settings.Value.OpenAiKey);
    }
    
    public async Task<GherkinTest> GenerateGherkin(Type @class, string testable, CancellationToken ct)
    {
        var pathes = await _callgraphExtractor.Extract(@class.FullName, testable, ct);
        var prompt = PromptBuilder.BuildGherkinTestExtractorPrompt(@class.FullName + "." + testable, pathes);
        
        List<ChatMessage> messages =
        [
            new UserChatMessage(prompt),
        ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "gherkin_test",
                jsonSchema: BinaryData.FromBytes("""
                                                 {
                                                     "type": "object",
                                                     "properties": {
                                                         "testText": {"type": "string" }
                                                     },
                                                     "required": ["testText"],
                                                     "additionalProperties": false
                                                 }
                                                 """u8.ToArray()),
                jsonSchemaIsStrict: true)
        };

        ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options, ct);

        var resultString = completion.Content[0].Text;
        return JsonSerializer.Deserialize<GherkinTest>(resultString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }


    public async Task<GherkinTest> GenerateGherkin(string @class, string testable, CancellationToken ct)
    {
        var pathes = await _callgraphExtractor.Extract(@class, testable, ct);
        var prompt = PromptBuilder.BuildGherkinTestExtractorPrompt(@class + "." + testable, pathes);
        
        List<ChatMessage> messages =
        [
            new UserChatMessage(prompt),
        ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "gherkin_test",
                jsonSchema: BinaryData.FromBytes("""
                                                 {
                                                     "type": "object",
                                                     "properties": {
                                                         "testText": {"type": "string" }
                                                     },
                                                     "required": ["testText"],
                                                     "additionalProperties": false
                                                 }
                                                 """u8.ToArray()),
                jsonSchemaIsStrict: true)
        };

        ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options, ct);

        var resultString = completion.Content[0].Text;
        return JsonSerializer.Deserialize<GherkinTest>(resultString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}