using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Options;

namespace NosimusAI;

public sealed class CallgraphExtractor
{
    private readonly string _projectPath;
    
    private Solution? _solution;

    public CallgraphExtractor(IOptions<NosimusSettings> settings)
    {
        _projectPath = settings.Value.SolutionPath;
    }
    
    public async Task<IReadOnlyCollection<string>> Extract(string entryClass, string entryMethod, CancellationToken ct = default)
    {
        var pathes = new HashSet<string>();
        
        if (_solution == null)
        {
            var workspace = MSBuildWorkspace.Create();
            _solution = await workspace.OpenSolutionAsync(_projectPath, cancellationToken: ct);
        }

        if (!_solution.Projects.Any())
        {
            throw new ApplicationException("No projects found in solution.\nVerify solution path, or install Microsoft.CodeAnalysis.CSharp and Microsoft.CodeAnalysis.CSharp.Workspaces packages.");
        }

        var classToFileMap = new Dictionary<string, string>();
        var callGraph = new Dictionary<string, HashSet<string>>();

        foreach (var project in _solution.Projects)
        {
            foreach (var document in project.Documents)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync(ct);
                var root = await syntaxTree.GetRootAsync(ct);
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                var namespaceDeclarations = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
                var fileScopedNamespaceDeclarations =
                    root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();

                var @namespace = string.Empty;

                foreach (var fileScopedNamespaceDeclaration in fileScopedNamespaceDeclarations)
                {
                    @namespace = fileScopedNamespaceDeclaration.Name.ToString();
                }

                foreach (var namespaceDeclaration in namespaceDeclarations)
                {
                    @namespace += namespaceDeclaration.Name.ToString();
                }

                foreach (var classDecl in classDeclarations)
                {
                    var className = string.IsNullOrWhiteSpace(@namespace) ?
                        classDecl.Identifier.Text :
                        @namespace + "." + classDecl.Identifier.Text;
                    classToFileMap[className] = document.FilePath; // Store file path
                }
            }

            foreach (var document in project.Documents)
            {
                var semanticModel = await document.GetSemanticModelAsync(ct);
                var syntaxTree = await document.GetSyntaxTreeAsync();
                var root = await syntaxTree.GetRootAsync();
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                var namespaceDeclarations = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
                var fileScopedNamespaceDeclarations =
                    root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();

                var @namespace = string.Empty;

                foreach (var fileScopedNamespaceDeclaration in fileScopedNamespaceDeclarations)
                {
                    @namespace = fileScopedNamespaceDeclaration.Name.ToString();
                }

                foreach (var namespaceDeclaration in namespaceDeclarations)
                {
                    @namespace += namespaceDeclaration.Name.ToString();
                }

                foreach (var classDecl in classDeclarations)
                {
                    var className = string.IsNullOrWhiteSpace(@namespace) ?
                        classDecl.Identifier.Text :
                        @namespace + "." + classDecl.Identifier.Text;

                    foreach (var method in classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>())
                    {
                        AddCallsToGraph(method, callGraph, className, semanticModel);
                    }

                    foreach (var constructor in classDecl.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
                    {
                        AddCallsToGraph(constructor, callGraph, className, semanticModel);
                    }

                    foreach (var property in classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>())
                    {
                        AddPropertyToGraph(property, callGraph, className);
                    }
                }
            }
        }

        pathes.Add(classToFileMap[entryClass]);
        PrintCallGraph(pathes, $"{entryClass}.{entryMethod}", callGraph, classToFileMap);

        var result = new List<string>();
        
        foreach (var path in pathes.Distinct())
        {
            result.Add(await File.ReadAllTextAsync(path, ct));
        }

        return result;
    }

    static void AddCallsToGraph(
        BaseMethodDeclarationSyntax methodOrConstructor,
        Dictionary<string, HashSet<string>> callGraph, 
        string className,
        SemanticModel semanticModel)
    {
        var methodName = methodOrConstructor is MethodDeclarationSyntax methodDecl
            ? $"{className}.{methodDecl.Identifier.Text}"
            : $"{className}.<ctor>";

        var calledMethods = methodOrConstructor.DescendantNodes().OfType<InvocationExpressionSyntax>();
        var objectCreations = methodOrConstructor.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();
        var propertyAccesses = methodOrConstructor.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

        if (!callGraph.ContainsKey(methodName))
        {
            callGraph[methodName] = new HashSet<string>();
        }

        // Add method calls
        foreach (var call in calledMethods)
        {
            var symbol = semanticModel.GetSymbolInfo(call).Symbol;
            if (symbol == null)
            {
                continue;
            }
            callGraph[methodName].Add(symbol.ContainingType + "." + symbol.Name);
        }

        // Add constructor calls
        foreach (var objCreate in objectCreations)
        {
            var symbol = semanticModel.GetSymbolInfo(objCreate).Symbol;
            if (symbol == null)
            {
                continue;
            }
            callGraph[methodName].Add(symbol.ContainingType + ".<ctor>");
        }

        // Add property accesses
        foreach (var propAccess in propertyAccesses)
        {
            var symbol = semanticModel.GetSymbolInfo(propAccess).Symbol;
            if (symbol == null)
            {
                continue;
            }
            var propertyName = propAccess.Name.Identifier.Text;
            callGraph[methodName].Add($"{symbol.ContainingType}.{propertyName}");
        }
    }

    static void AddPropertyToGraph(PropertyDeclarationSyntax property, Dictionary<string, HashSet<string>> callGraph, string className)
    {
        var propertyName = $"{className}.{property.Identifier.Text}";

        if (!callGraph.ContainsKey(propertyName))
        {
            callGraph[propertyName] = new HashSet<string>();
        }

        callGraph[propertyName].Add(propertyName);
    }

    static void PrintCallGraph(
        HashSet<string> pathes,
        string method,
        Dictionary<string, HashSet<string>> callGraph, 
        Dictionary<string, string> classToFileMap)
    {
        if (!callGraph.TryGetValue(method, out var value1))
            return;

        foreach (var callee in value1)
        {
            var className = string.Join('.', callee.Split('.')[..^1]);

            if (!classToFileMap.TryGetValue(className, out var value))
            {
                continue;
            }
            
            var filePath = classToFileMap.ContainsKey(className) ? value : "Unknown File";

            if (pathes.Contains(filePath))
            {
                continue;
            }
            
            pathes.Add(filePath);
            PrintCallGraph(pathes, callee, callGraph, classToFileMap);
        }
    }
}