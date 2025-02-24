using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
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

    [UsedImplicitly]
    public async Task<IReadOnlyCollection<ControllerEndpointGherkinTest>> GenerateGherkinForAspNetControllers(Assembly assembly, CancellationToken ct)
    {
        var result = new List<ControllerEndpointGherkinTest>();
        
        foreach (var controller in EndpointsExtractor.Extract(assembly))
        {
            foreach (var controllerEndpoint in controller.Endpoints)
            {
                Console.WriteLine($"Generating Gherkin test for {controller.Controller.FullName!}.{controllerEndpoint}");
                
                var code = await _callgraphExtractor.Extract(controller.Controller.FullName!, controllerEndpoint, ct);
        
                var prompt = PromptBuilder.BuildGherkinTestExtractorPrompt(controller.Controller.FullName! + "." + controllerEndpoint, code);
        
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
                var gherkinTest = JsonSerializer.Deserialize<GherkinTest>(resultString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                result.Add(new ControllerEndpointGherkinTest
                {
                    ControllerType = controller.Controller,
                    Endpoint = controllerEndpoint,
                    TestText = gherkinTest!.TestText,
                });
            }
        }

        return result;
    }
    
    [UsedImplicitly]
    public async Task<GherkinTest> GenerateGherkin(Type @class, string testable, CancellationToken ct)
    {
        var code = await _callgraphExtractor.Extract(@class.FullName, testable, ct);
        
        var prompt = PromptBuilder.BuildGherkinTestExtractorPrompt(@class.FullName + "." + testable, code);
        
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

    [UsedImplicitly]
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