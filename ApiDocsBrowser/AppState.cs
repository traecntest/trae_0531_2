using Microsoft.OpenApi.Models;

namespace ApiDocsBrowser;

public static class AppState
{
    public static OpenApiDocument? CurrentDocument { get; set; }
}
