using Microsoft.OpenApi.Models;
using Scriban;

namespace ApiDocsBrowser.Services;

public class HtmlExportService
{
    public async Task<string> ExportToHtmlAsync(OpenApiDocument document, string title, bool darkMode = false)
    {
        var template = Template.Parse(HtmlTemplate);
        
        var model = new ExportModel
        {
            Title = title,
            DarkMode = darkMode,
            GeneratedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Servers = document.Servers.Select(s => s.Url).ToList(),
            Paths = document.Paths.Select(p => new PathModel
            {
                Path = p.Key,
                Operations = p.Value.Operations.Select(o => new OperationModel
                {
                    Method = o.Key.ToString().ToUpper(),
                    Summary = o.Value.Summary ?? "",
                    Description = o.Value.Description ?? "",
                    Parameters = o.Value.Parameters?.Select(param => new ParameterModel
                    {
                        Name = param.Name,
                        In = param.In.ToString(),
                        Required = param.Required,
                        Schema = GetSchemaType(param.Schema),
                        Description = param.Description ?? ""
                    }).ToList() ?? new List<ParameterModel>(),
                    Responses = o.Value.Responses.Select(r => new ResponseModel
                    {
                        StatusCode = r.Key,
                        Description = r.Value.Description ?? ""
                    }).ToList()
                }).ToList()
            }).ToList(),
            Schemas = document.Components?.Schemas?.Select(s => new SchemaModel
            {
                Name = s.Key,
                Type = GetSchemaType(s.Value),
                Properties = s.Value.Properties?.Select(p => new PropertyModel
                {
                    Name = p.Key,
                    Type = GetSchemaType(p.Value),
                    Description = p.Value.Description ?? ""
                }).ToList() ?? new List<PropertyModel>()
            }).ToList() ?? new List<SchemaModel>()
        };

        return await template.RenderAsync(model);
    }

    private static string GetSchemaType(OpenApiSchema? schema)
    {
        if (schema == null) return "object";
        
        if (schema.Reference != null)
        {
            return schema.Reference.Id;
        }
        
        if (schema.Type == "array" && schema.Items != null)
        {
            return $"Array[{GetSchemaType(schema.Items)}]";
        }
        
        return schema.Type ?? "object";
    }

    public class ExportModel
    {
        public string Title { get; set; } = "";
        public bool DarkMode { get; set; }
        public string GeneratedAt { get; set; } = "";
        public List<string> Servers { get; set; } = new();
        public List<PathModel> Paths { get; set; } = new();
        public List<SchemaModel> Schemas { get; set; } = new();
    }

    public class PathModel
    {
        public string Path { get; set; } = "";
        public List<OperationModel> Operations { get; set; } = new();
    }

    public class OperationModel
    {
        public string Method { get; set; } = "";
        public string Summary { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ParameterModel> Parameters { get; set; } = new();
        public List<ResponseModel> Responses { get; set; } = new();
    }

    public class ParameterModel
    {
        public string Name { get; set; } = "";
        public string In { get; set; } = "";
        public bool Required { get; set; }
        public string Schema { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class ResponseModel
    {
        public string StatusCode { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class SchemaModel
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public List<PropertyModel> Properties { get; set; } = new();
    }

    public class PropertyModel
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
    }

    private const string HtmlTemplate = @"
<!DOCTYPE html>
<html lang=""zh-CN"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{{ model.Title }} - API 文档</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        
        :root {
            --bg-primary: #ffffff;
            --bg-secondary: #f8fafc;
            --bg-tertiary: #f1f5f9;
            --text-primary: #0f172a;
            --text-secondary: #64748b;
            --border: #e2e8f0;
            --primary: #2563eb;
            --get: #10b981;
            --post: #3b82f6;
            --put: #f59e0b;
            --delete: #ef4444;
            --patch: #8b5cf6;
        }
        
        [data-dark=""true""] {
            --bg-primary: #0f172a;
            --bg-secondary: #1e293b;
            --bg-tertiary: #334155;
            --text-primary: #f1f5f9;
            --text-secondary: #94a3b8;
            --border: #334155;
        }
        
        body {
            font-family: Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
            background: var(--bg-secondary);
            color: var(--text-primary);
            line-height: 1.6;
        }
        
        .container { display: flex; min-height: 100vh; }
        
        .sidebar {
            width: 300px;
            background: var(--bg-primary);
            border-right: 1px solid var(--border);
            position: fixed;
            height: 100vh;
            overflow-y: auto;
            padding: 24px 0;
        }
        
        .sidebar-header {
            padding: 0 24px 24px;
            border-bottom: 1px solid var(--border);
            margin-bottom: 16px;
        }
        
        .sidebar-title {
            font-size: 1.25rem;
            font-weight: 700;
            color: var(--text-primary);
        }
        
        .sidebar-subtitle {
            font-size: 0.875rem;
            color: var(--text-secondary);
            margin-top: 4px;
        }
        
        .nav-section { margin-bottom: 24px; }
        
        .nav-title {
            font-size: 0.75rem;
            font-weight: 600;
            text-transform: uppercase;
            color: var(--text-secondary);
            padding: 0 24px 8px;
            letter-spacing: 0.05em;
        }
        
        .nav-item {
            display: block;
            padding: 8px 24px;
            color: var(--text-primary);
            text-decoration: none;
            font-size: 0.875rem;
            cursor: pointer;
            transition: background 0.15s;
        }
        
        .nav-item:hover { background: var(--bg-secondary); }
        
        .main {
            flex: 1;
            margin-left: 300px;
            padding: 40px 60px;
            max-width: 1200px;
        }
        
        .page-title {
            font-size: 2rem;
            font-weight: 700;
            margin-bottom: 8px;
        }
        
        .page-description {
            color: var(--text-secondary);
            margin-bottom: 32px;
        }
        
        .section { margin-bottom: 48px; }
        
        .section-title {
            font-size: 1.5rem;
            font-weight: 700;
            margin-bottom: 24px;
            padding-bottom: 12px;
            border-bottom: 2px solid var(--border);
        }
        
        .operation-card {
            background: var(--bg-primary);
            border: 1px solid var(--border);
            border-radius: 12px;
            margin-bottom: 16px;
            overflow: hidden;
        }
        
        .operation-header {
            display: flex;
            align-items: center;
            gap: 12px;
            padding: 16px 20px;
            background: var(--bg-secondary);
            cursor: pointer;
            transition: background 0.15s;
        }
        
        .operation-header:hover { background: var(--bg-tertiary); }
        
        .method-badge {
            padding: 4px 12px;
            border-radius: 6px;
            font-size: 0.75rem;
            font-weight: 700;
            color: white;
            min-width: 70px;
            text-align: center;
        }
        
        .method-GET { background: var(--get); }
        .method-POST { background: var(--post); }
        .method-PUT { background: var(--put); }
        .method-DELETE { background: var(--delete); }
        .method-PATCH { background: var(--patch); }
        
        .operation-path {
            font-family: 'JetBrains Mono', Consolas, monospace;
            font-size: 0.875rem;
            font-weight: 600;
            flex: 1;
        }
        
        .operation-summary {
            color: var(--text-secondary);
            font-size: 0.875rem;
        }
        
        .operation-body { padding: 20px; }
        
        .subsection-title {
            font-size: 0.875rem;
            font-weight: 600;
            margin-bottom: 12px;
            color: var(--text-secondary);
            text-transform: uppercase;
            letter-spacing: 0.05em;
        }
        
        .table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 16px;
        }
        
        .table th, .table td {
            padding: 10px 12px;
            text-align: left;
            border-bottom: 1px solid var(--border);
            font-size: 0.875rem;
        }
        
        .table th {
            background: var(--bg-secondary);
            font-weight: 600;
        }
        
        .badge {
            display: inline-block;
            padding: 2px 8px;
            border-radius: 4px;
            font-size: 0.75rem;
            font-weight: 600;
        }
        
        .badge-required {
            background: #fee2e2;
            color: #dc2626;
        }
        
        [data-dark=""true""] .badge-required {
            background: rgba(239, 68, 68, 0.2);
        }
        
        .badge-optional {
            background: var(--bg-tertiary);
            color: var(--text-secondary);
        }
        
        .schema-card {
            background: var(--bg-primary);
            border: 1px solid var(--border);
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 16px;
        }
        
        .schema-name {
            font-size: 1rem;
            font-weight: 600;
            margin-bottom: 12px;
        }
        
        .footer {
            margin-top: 60px;
            padding-top: 24px;
            border-top: 1px solid var(--border);
            color: var(--text-secondary);
            font-size: 0.875rem;
            text-align: center;
        }
        
        .server-info {
            background: var(--bg-primary);
            border: 1px solid var(--border);
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 32px;
        }
        
        .server-title {
            font-weight: 600;
            margin-bottom: 8px;
        }
        
        .server-url {
            font-family: 'JetBrains Mono', Consolas, monospace;
            padding: 8px 12px;
            background: var(--bg-secondary);
            border-radius: 6px;
            display: inline-block;
            font-size: 0.875rem;
        }
        
        code {
            font-family: 'JetBrains Mono', Consolas, monospace;
            background: var(--bg-tertiary);
            padding: 2px 6px;
            border-radius: 4px;
            font-size: 0.875em;
        }
    </style>
</head>
<body data-dark=""{{ model.DarkMode }}"">
    <div class=""container"">
        <aside class=""sidebar"">
            <div class=""sidebar-header"">
                <div class=""sidebar-title"">{{ model.Title }}</div>
                <div class=""sidebar-subtitle"">API 文档</div>
            </div>
            
            <nav>
                <div class=""nav-section"">
                    <div class=""nav-title"">API</div>
                    {{ for path in model.Paths }}
                        {{ for operation in path.Operations }}
                            <a class=""nav-item"" href=""#{{ path.Path | string.replace '/', '-' | string.replace '{', '' | string.replace '}', '' }}-{{ operation.Method }}"">
                                {{ operation.Method }} {{ path.Path }}
                            </a>
                        {{ end }}
                    {{ end }}
                </div>
                
                {{ if model.Schemas.size > 0 }}
                <div class=""nav-section"">
                    <div class=""nav-title"">模型</div>
                    {{ for schema in model.Schemas }}
                        <a class=""nav-item"" href=""#schema-{{ schema.Name }}"">{{ schema.Name }}</a>
                    {{ end }}
                </div>
                {{ end }}
            </nav>
        </aside>
        
        <main class=""main"">
            <h1 class=""page-title"">{{ model.Title }}</h1>
            <p class=""page-description"">由 ApiDocsBrowser 生成于 {{ model.GeneratedAt }}</p>
            
            {{ if model.Servers.size > 0 }}
            <div class=""server-info"">
                <div class=""server-title"">服务器地址</div>
                {{ for server in model.Servers }}
                    <code class=""server-url"">{{ server }}</code>
                {{ end }}
            </div>
            {{ end }}
            
            <div class=""section"">
                <h2 class=""section-title"">接口文档</h2>
                
                {{ for path in model.Paths }}
                    {{ for operation in path.Operations }}
                    <div class=""operation-card"" id=""{{ path.Path | string.replace '/', '-' | string.replace '{', '' | string.replace '}', '' }}-{{ operation.Method }}"">
                        <div class=""operation-header"">
                            <span class=""method-badge method-{{ operation.Method }}"">{{ operation.Method }}</span>
                            <span class=""operation-path"">{{ path.Path }}</span>
                            <span class=""operation-summary"">{{ operation.Summary }}</span>
                        </div>
                        
                        <div class=""operation-body"">
                            {{ if operation.Description }}
                            <p style=""margin-bottom: 20px; color: var(--text-secondary);"">{{ operation.Description }}</p>
                            {{ end }}
                            
                            {{ if operation.Parameters.size > 0 }}
                            <div style=""margin-bottom: 24px;"">
                                <div class=""subsection-title"">参数</div>
                                <table class=""table"">
                                    <thead>
                                        <tr>
                                            <th>名称</th>
                                            <th>位置</th>
                                            <th>类型</th>
                                            <th>必填</th>
                                            <th>描述</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {{ for param in operation.Parameters }}
                                        <tr>
                                            <td><code>{{ param.Name }}</code></td>
                                            <td>{{ param.In }}</td>
                                            <td><code>{{ param.Schema }}</code></td>
                                            <td>
                                                <span class=""badge {{ if param.Required }}badge-required{{ else }}badge-optional{{ end }}"">
                                                    {{ if param.Required }}必填{{ else }}可选{{ end }}
                                                </span>
                                            </td>
                                            <td>{{ param.Description }}</td>
                                        </tr>
                                        {{ end }}
                                    </tbody>
                                </table>
                            </div>
                            {{ end }}
                            
                            <div>
                                <div class=""subsection-title"">响应</div>
                                <table class=""table"">
                                    <thead>
                                        <tr>
                                            <th>状态码</th>
                                            <th>描述</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {{ for response in operation.Responses }}
                                        <tr>
                                            <td><code>{{ response.StatusCode }}</code></td>
                                            <td>{{ response.Description }}</td>
                                        </tr>
                                        {{ end }}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                    {{ end }}
                {{ end }}
            </div>
            
            {{ if model.Schemas.size > 0 }}
            <div class=""section"">
                <h2 class=""section-title"">数据模型</h2>
                
                {{ for schema in model.Schemas }}
                <div class=""schema-card"" id=""schema-{{ schema.Name }}"">
                    <div class=""schema-name"">{{ schema.Name }} <code style=""color: var(--text-secondary);"">{{ schema.Type }}</code></div>
                    
                    {{ if schema.Properties.size > 0 }}
                    <table class=""table"" style=""margin-bottom: 0;"">
                        <thead>
                            <tr>
                                <th>属性</th>
                                <th>类型</th>
                                <th>描述</th>
                            </tr>
                        </thead>
                        <tbody>
                            {{ for prop in schema.Properties }}
                            <tr>
                                <td><code>{{ prop.Name }}</code></td>
                                <td><code>{{ prop.Type }}</code></td>
                                <td>{{ prop.Description }}</td>
                            </tr>
                            {{ end }}
                        </tbody>
                    </table>
                    {{ end }}
                </div>
                {{ end }}
            </div>
            {{ end }}
            
            <div class=""footer"">
                由 ApiDocsBrowser 生成 | {{ model.GeneratedAt }}
            </div>
        </main>
    </div>
</body>
</html>
";
}
