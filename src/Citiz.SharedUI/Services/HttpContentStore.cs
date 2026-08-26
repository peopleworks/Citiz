using System.Net;
using Citiz.Content;

namespace Citiz.SharedUI.Services;

/// <summary>Reads the content files the build copies under <c>/content</c>. Same JSON as the repository, fetched once and cached by the service worker.</summary>
public sealed class HttpContentStore(HttpClient http) : IContentStore
{
    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"content/{relativePath}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            response.Dispose();
            throw new FileNotFoundException($"Content file '{relativePath}' was not found on the server.", relativePath);
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, $"content/{relativePath}");
        using var response = await http.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
