using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Wichteln.Shared;

namespace Wichteln.Client.Services;

/// <summary>Raised when the API refuses a request; Message is ready to show.</summary>
public sealed class ApiException(string message, HttpStatusCode status) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}

public sealed class ApiClient(HttpClient http, IJSRuntime js)
{
    private string? _token;

    public async Task<string?> GetTokenAsync() => _token ??= await js.InvokeAsync<string?>("wichteln.getToken");

    public async Task SetTokenAsync(string token)
    {
        _token = token;
        await js.InvokeVoidAsync("wichteln.setToken", token);
    }

    public async Task ClearTokenAsync()
    {
        _token = null;
        await js.InvokeVoidAsync("wichteln.clearToken");
    }

    public Task<MeDto> GetMeAsync() => SendAsync<MeDto>(HttpMethod.Get, "api/me");
    public Task<WishDto> AddWishAsync(WishInput input) => SendAsync<WishDto>(HttpMethod.Post, "api/wishes", input);
    public Task<WishDto> UpdateWishAsync(int id, WishInput input) => SendAsync<WishDto>(HttpMethod.Put, $"api/wishes/{id}", input);
    public Task DeleteWishAsync(int id) => SendAsync<object>(HttpMethod.Delete, $"api/wishes/{id}");
    public Task<WichtelDto> RevealAsync() => SendAsync<WichtelDto>(HttpMethod.Post, "api/reveal");
    public Task<WichtelDto> GetWichtelAsync() => SendAsync<WichtelDto>(HttpMethod.Get, "api/wichtel");

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null)
    {
        using var req = new HttpRequestMessage(method, path);
        var token = await GetTokenAsync();
        if (token is not null) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) req.Content = JsonContent.Create(body);

        HttpResponseMessage res;
        try { res = await http.SendAsync(req); }
        catch (HttpRequestException)
        {
            throw new ApiException("Keine Verbindung zum Server. Prüf dein Internet und versuch es nochmal.", 0);
        }

        using (res)
        {
            if (res.IsSuccessStatusCode)
                return res.StatusCode == HttpStatusCode.NoContent ? default! : (await res.Content.ReadFromJsonAsync<T>())!;

            string? detail = null;
            try { detail = (await res.Content.ReadFromJsonAsync<ProblemBody>())?.Detail; } catch { }
            throw new ApiException(detail ?? res.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Dieser Link ist ungültig. Frag Denis nach deinem persönlichen Link.",
                HttpStatusCode.TooManyRequests => "Zu viele Anfragen. Warte kurz und versuch es nochmal.",
                _ => "Da ist etwas schiefgelaufen. Versuch es nochmal.",
            }, res.StatusCode);
        }
    }

    private sealed record ProblemBody(string? Detail);
}
