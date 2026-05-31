using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ApiDocsBrowser.Services;

public class OpenApiParserService
{
    public async Task<OpenApiDocument?> ParseAsync(string content)
    {
        return await ParseAutoAsync(content);
    }

    public List<(OpenApiTag Tag, List<(string Path, OperationType Method, OpenApiOperation Operation)> Operations)> GroupByTags(OpenApiDocument document)
    {
        var result = new List<(OpenApiTag, List<(string, OperationType, OpenApiOperation)>)>();
        var grouped = GroupPathsByTag(document);

        foreach (var group in grouped)
        {
            var tag = document.Tags.FirstOrDefault(t => t.Name == group.Key) ?? new OpenApiTag { Name = group.Key };
            result.Add((tag, group.Value));
        }

        return result;
    }

    public async Task<OpenApiDocument?> ParseJsonAsync(string jsonContent)
    {
        var openApiReader = new OpenApiStringReader();
        var document = openApiReader.Read(jsonContent, out var diagnostic);
        
        if (diagnostic.Errors.Count > 0)
        {
            throw new InvalidOperationException($"解析错误: {string.Join(", ", diagnostic.Errors.Select(e => e.Message))}");
        }
        
        return document;
    }

    public async Task<OpenApiDocument?> ParseYamlAsync(string yamlContent)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        var yamlObject = deserializer.Deserialize<Dictionary<object, object>>(yamlContent);
        var jsonContent = System.Text.Json.JsonSerializer.Serialize(yamlObject);
        
        return await ParseJsonAsync(jsonContent);
    }

    public async Task<OpenApiDocument?> ParseAutoAsync(string content)
    {
        content = content.Trim();
        
        if (content.StartsWith('{') || content.StartsWith('['))
        {
            return await ParseJsonAsync(content);
        }
        
        return await ParseYamlAsync(content);
    }

    public async Task<OpenApiDocument?> LoadFromUrlAsync(HttpClient httpClient, string url)
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return await ParseAutoAsync(content);
    }

    public List<OpenApiTag> GetTags(OpenApiDocument document)
    {
        return document.Tags.ToList();
    }

    public Dictionary<string, List<(string Path, OperationType Type, OpenApiOperation Operation)>> GroupPathsByTag(OpenApiDocument document)
    {
        var grouped = new Dictionary<string, List<(string, OperationType, OpenApiOperation)>>();
        
        foreach (var pathItem in document.Paths)
        {
            foreach (var operation in pathItem.Value.Operations)
            {
                var tags = operation.Value.Tags.Any() 
                    ? operation.Value.Tags.Select(t => t.Name) 
                    : new[] { "default" };

                foreach (var tag in tags)
                {
                    if (!grouped.ContainsKey(tag))
                    {
                        grouped[tag] = new List<(string, OperationType, OpenApiOperation)>();
                    }
                    grouped[tag].Add((pathItem.Key, operation.Key, operation.Value));
                }
            }
        }
        
        return grouped;
    }

    public string GetOperationMethodColor(OperationType type)
    {
        return type switch
        {
            OperationType.Get => "#10b981",
            OperationType.Post => "#3b82f6",
            OperationType.Put => "#f59e0b",
            OperationType.Delete => "#ef4444",
            OperationType.Patch => "#8b5cf6",
            OperationType.Options => "#6b7280",
            OperationType.Head => "#6b7280",
            OperationType.Trace => "#6b7280",
            _ => "#6b7280"
        };
    }
}
