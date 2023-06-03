using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Serialization.TextJson;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching.Memory;

namespace GiganticEmu.Shared;

public static class HttpExtensions
{
    public record PollyFlurlRequest(IFlurlRequest Request, IAsyncPolicy Policy);

    private static MemoryCacheProvider? CACHE_PROVIDER = new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions()));

    static HttpExtensions()
    {
        FlurlHttp.Configure(settings => settings.WithTextJsonSerializer(new JsonSerializerOptions { }));
    }

    private static bool IsTransientError(FlurlHttpException exception)
    {
        int[] httpStatusCodesWorthRetrying =
        {
            (int)HttpStatusCode.RequestTimeout,
            (int)HttpStatusCode.BadGateway,
            (int)HttpStatusCode.ServiceUnavailable,
            (int)HttpStatusCode.GatewayTimeout,
        };

        return exception.StatusCode.HasValue && httpStatusCodesWorthRetrying.Contains(exception.StatusCode.Value);
    }

    public static PollyFlurlRequest WithPolly(this IFlurlRequest request, Func<PolicyBuilder, IAsyncPolicy> config)
        => new PollyFlurlRequest(request, config(Policy.Handle<FlurlHttpException>(IsTransientError)));

    public static PollyFlurlRequest Cached(this PollyFlurlRequest request, TimeSpan? t = null)
        => new PollyFlurlRequest(request.Request, request.Policy.WrapAsync(Policy.CacheAsync(
                cacheProvider: CACHE_PROVIDER,
                ttl: t ?? TimeSpan.FromMinutes(1),
                cacheKeyStrategy: _ => request.Request.Url
            )));

    public static PollyFlurlRequest WithPolly(this string url, Func<PolicyBuilder, IAsyncPolicy> config)
        => new PollyFlurlRequest(new FlurlRequest(url), config(Policy.Handle<FlurlHttpException>(IsTransientError)));

    public static PollyFlurlRequest WithPolly(this Uri uri, Func<PolicyBuilder, IAsyncPolicy> config)
        => new PollyFlurlRequest(new FlurlRequest(uri), config(Policy.Handle<FlurlHttpException>(IsTransientError)));

    public static PollyFlurlRequest WithPolly(this Url url, Func<PolicyBuilder, IAsyncPolicy> config)
        => new PollyFlurlRequest(new FlurlRequest(url), config(Policy.Handle<FlurlHttpException>(IsTransientError)));

    public static Task<IFlurlResponse> DeleteAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.DeleteAsync(cancellationToken, completionOption));
    }

    // public static Task<string> DownloadFileAsync(this PollyFlurlRequest request, string localFolderPath, string? localFileName = null, int bufferSize = 4096, CancellationToken cancellationToken = default)
    // {
    //     return request.Policy.ExecuteAsync(() => request.Request.DownloadFileAsync(localFolderPath, localFileName, bufferSize, cancellationToken));
    // }

    public static Task<IFlurlResponse> GetAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.GetAsync(cancellationToken, completionOption));
    }

    public static Task<byte[]> GetBytesAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.GetBytesAsync(cancellationToken, completionOption));
    }

    public static Task<dynamic> GetJsonAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.GetJsonAsync(cancellationToken, completionOption));
    }

    public static Task<T> GetJsonAsync<T>(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.GetJsonAsync<T>(cancellationToken, completionOption));
    }

    public static Task<IList<dynamic>> GetJsonListAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.GetJsonListAsync(cancellationToken, completionOption));
    }

    // public static Task<Stream> GetStreamAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    // {
    //     return request.Policy.ExecuteAsync(() => request.Request.GetStreamAsync(cancellationToken, completionOption));
    // }

    public static Task<string> GetStringAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.GetStringAsync(cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> HeadAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.HeadAsync(cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> OptionsAsync(this PollyFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.OptionsAsync(cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PatchAsync(this PollyFlurlRequest request, HttpContent? content = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PatchAsync(content, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PatchJsonAsync(this PollyFlurlRequest request, object data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PatchJsonAsync(data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PatchStringAsync(this PollyFlurlRequest request, string data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PatchStringAsync(data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PostAsync(this PollyFlurlRequest request, HttpContent? content = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PostAsync(content, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PostJsonAsync(this PollyFlurlRequest request, object data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PostJsonAsync(data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PostStringAsync(this PollyFlurlRequest request, string data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PostStringAsync(data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PostUrlEncodedAsync(this PollyFlurlRequest request, object data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PostUrlEncodedAsync(data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PutAsync(this PollyFlurlRequest request, HttpContent? content = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PutAsync(content, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PutJsonAsync(this PollyFlurlRequest request, object data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PutJsonAsync(data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> PutStringAsync(this PollyFlurlRequest request, string data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.PutStringAsync(data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> SendJsonAsync(this PollyFlurlRequest request, HttpMethod verb, object data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.SendJsonAsync(verb, data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> SendStringAsync(this PollyFlurlRequest request, HttpMethod verb, string data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.SendStringAsync(verb, data, cancellationToken, completionOption));
    }

    public static Task<IFlurlResponse> SendUrlEncodedAsync(this PollyFlurlRequest request, HttpMethod verb, object data, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return request.Policy.ExecuteAsync(() => request.Request.SendUrlEncodedAsync(verb, data, cancellationToken, completionOption));
    }

    public static async Task<Stream> GetStreamAsync(this PollyFlurlRequest request, int bufferSize = 4096, Action<double>? progressCallback = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return await request.Policy.ExecuteAsync(async () =>
        {
            IFlurlResponse response = await request.Request.GetAsync(cancellationToken, completionOption);

            var stream = await response.GetStreamAsync();

            var buffer = new byte[bufferSize];
            var totalBytes = Convert.ToDouble(response.ResponseMessage.Content.Headers.ContentLength);

            return new ProgressStream(stream, (read) => progressCallback?.Invoke(read / totalBytes));
        });
    }

    public static async Task<string> DownloadFileAsync(this PollyFlurlRequest request, string localFolderPath, string? localFileName = null, int bufferSize = 4096, Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(async () =>
        {
            var filename = localFileName ?? Path.GetFileName(request.Request.Url);

            if (!Directory.Exists(localFolderPath))
                Directory.CreateDirectory(localFolderPath);

            using var input = await request.GetStreamAsync(bufferSize, progressCallback, cancellationToken, HttpCompletionOption.ResponseHeadersRead);
            using var output = File.OpenWrite(Path.Join(localFolderPath, localFileName));

            await input.CopyToAsync(output);
        });
        return Path.Join(localFolderPath, localFileName);
    }
}