namespace NosimusAI;

public class TestRunner
{
    private readonly AITestRunner _aiTestRunner;
    
    private readonly CallgraphExtractor _callgraphExtractor;
    
    public TestRunner(CallgraphExtractor callgraphExtractor, AITestRunner aiTestRunner)
    {
        _callgraphExtractor = callgraphExtractor;
        _aiTestRunner = aiTestRunner;
    }
    
    public async Task<TestResult> RunTest(string requirement, string @class, string testable, CancellationToken ct = default)
    {
        var pathes = await _callgraphExtractor.Extract(@class, testable, ct);
        var prompt = PromptBuilder.BuildPrompt(@class + "." + testable, requirement, pathes);
        var aiTestResult = await _aiTestRunner.RunTest(prompt, ct);
        
        return new TestResult
        {
            Passed = aiTestResult.Passed,
            ProblemExplanation = aiTestResult.ProblemExplanation,
            FixSuggestion = aiTestResult.FixSuggestion
        };
    }

    public async Task<TestResult> RunTest(string requirement, Type t, string testable, CancellationToken ct = default)
    {
        var pathes = await _callgraphExtractor.Extract(t.FullName!, testable, ct);
        var prompt = PromptBuilder.BuildPrompt(t.FullName + "." + testable, requirement, pathes);
        var aiTestResult = await _aiTestRunner.RunTest(prompt, ct);
        
        return new TestResult
        {
            Passed = aiTestResult.Passed,
            ProblemExplanation = aiTestResult.ProblemExplanation,
            FixSuggestion = aiTestResult.FixSuggestion
        };
    }
}