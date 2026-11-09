using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Services;

public sealed record ApiResult<T>(T? Value, string? Error, int StatusCode)
{
    public bool Succeeded => StatusCode is >= 200 and < 300;
}


public sealed class RaceDayApiClient(HttpClient http)
{
    public Task<ApiResult<T>> GetAsync<T>(string path, string? token = null) =>
        SendAsync<T>(HttpMethod.Get, path, null, token);

    public Task<ApiResult<T>> PostAsync<T>(string path, object? body, string? token = null) =>
        SendAsync<T>(HttpMethod.Post, path, body, token);

    public Task<ApiResult<T>> PutAsync<T>(string path, object body, string? token = null) =>
        SendAsync<T>(HttpMethod.Put, path, body, token);


    public async Task<ApiResult<T>> PostFileAsync<T>(string path, IFormFile file, string? token = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        form.Add(content, "file", Path.GetFileName(file.FileName));
        request.Content = form;

        return await SendMultipartAsync<T>(request);
    }


    public Task<ApiResult<bool>> DeleteAsync(string path, string? token = null) =>
        SendAsync<bool>(HttpMethod.Delete, path, null, token);

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body, string? token)
    {
        using var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        try
        {
            using var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<T>(default, await GetErrorAsync(response), (int)response.StatusCode);

            if (response.StatusCode == HttpStatusCode.NoContent || typeof(T) == typeof(bool))
                return new ApiResult<T>((T)(object)true, null, (int)response.StatusCode);

            var value = await response.Content.ReadFromJsonAsync<T>();
            return new ApiResult<T>(value, null, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T>(default, "Can't connect to RaceDay. Start the API and try again.", 0);
        }
        catch (TaskCanceledException)
        {
            return new ApiResult<T>(default, "RaceDay took too long to respond. Try again.", 0);
        }
        catch (JsonException)
        {
            return new ApiResult<T>(default, "RaceDay returned an unexpected response. Try again.", 0);
        }
    }


    private async Task<ApiResult<T>> SendMultipartAsync<T>(HttpRequestMessage request)
    {
        try
        {
            using var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<T>(default, await GetErrorAsync(response), (int)response.StatusCode);

            var value = await response.Content.ReadFromJsonAsync<T>();
            return new ApiResult<T>(value, null, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T>(default, "Can't connect to RaceDay. Start the API and try again.", 0);
        }
        catch (TaskCanceledException)
        {
            return new ApiResult<T>(default, "RaceDay took too long to respond. Try again.", 0);
        }
        catch (JsonException)
        {
            return new ApiResult<T>(default, "RaceDay returned an unexpected response. Try again.", 0);
        }
    }


    private static async Task<string> GetErrorAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return "Your session expired. Please sign in again.";
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return "You don't have permission to do that.";
        if (response.StatusCode == HttpStatusCode.NotFound)
            return "That item could not be found.";

        try
        {
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            if (document.RootElement.TryGetProperty("message", out var message))
                return message.GetString() ?? "Check the information and try again.";
            if (document.RootElement.TryGetProperty("title", out var title))
                return title.GetString() ?? "Check the information and try again.";
        }
        catch (JsonException)
        {
        }

        return response.StatusCode == HttpStatusCode.Conflict
            ? "This action conflicts with existing information."
            : "Check the information and try again.";
    }
}
