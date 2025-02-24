using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NosimusAI.Models;

namespace NosimusAI;

internal sealed class EndpointsExtractor
{
    public static IReadOnlyCollection<ControllerWithEndpointsResult> Extract(Assembly assembly)
    {
        var result = new List<ControllerWithEndpointsResult>();
        var controllers = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(t));

        foreach (var controller in controllers)
        {
            var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.IsPublic && m.DeclaringType == controller);
            var endpoints = actions.Where(x => x.GetCustomAttributes().OfType<HttpMethodAttribute>().Any()).ToArray();
            result.Add(new ControllerWithEndpointsResult
            {
                Controller = controller,
                Endpoints = endpoints.Select(e => e.Name).ToArray()
            });            
        }

        return result;
    }
}