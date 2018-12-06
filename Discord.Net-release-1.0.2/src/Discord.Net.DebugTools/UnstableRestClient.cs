using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Net.Rest
{
    internal sealed class UnstableRestClient : IRestClient, IDisposable
    {
        private const double FailureRate = 0.10; //10%

        private const int HR_SECURECHANNELFAILED = -2146233079;

        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly JsonSerializer _errorDeserializer;
        private readonly Random _rand;
        private CancellationToken _cancelToken;
        private bool _isDisposed;

        public DefaultRestClient(string baseUrl)
        {
            _baseUrl = baseUrl;

            _client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = false,
                UseProxy = false
            });
            SetHeader("accept-encoding", "gzip, deflate");

            _cancelToken = CancellationToken.None;
            _errorDeserializer = new JsonSerializer();
            _rand = new Random();
        }
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _client.Dispose();
                _isDisposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

        public void SetHeader(string key, string value)
        {
            _client.DefaultRequestHeaders.Remove(key);
            if (value != null)
                _client.DefaultRequestHeaders.Add(key, value);
        }
        public void SetCancelToken(CancellationToken cancelToken)
        {
            _cancelToken = cancelToken;
        }

        public async Task<RestResponse> SendAsync(string method, string endpoint, CancellationToken cancelToken, bool headerOnly)
        {
            string uri = Path.Combine(_baseUrl, endpoint);
            using (var restRequest = new HttpRequestMessage(GetMethod(method), uri))
                return await SendInternalAsync(restRequest, cancelToken, headerOnly).ConfigureAwait(false);
        }
        public async Task<RestResponse> SendAsync(string method, string endpoint, string json, CancellationToken cancelToken, bool headerOnly)
        {
            string uri = Path.Combine(_baseUrl, endpoint);
            using (var restRequest = new HttpRequestMessage(GetMethod(method), uri))
            {
                restRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return await SendInternalAsync(restRequest, cancelToken, headerOnly).ConfigureAwait(false);
            }
        }
        public async Task<RestResponse> SendAsync(string method, string endpoint, IReadOnlyDictionary<string, object> multipartParams, CancellationToken cancelToken, bool headerOnly)
        {
            string uri = Path.Combine(_baseUrl, endpoint);
            using (var restRequest = new HttpRequestMessage(GetMethod(method), uri))
            {
                var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                if (multipartParams != null)
                {
                    foreach (var p in multipartParams)
                    {
                        switch (p.Value)
                        {
                            case string stringValue: { content.Add(new StringContent(stringValue), p.Key); continue; }
                            case byte[] byteArrayValue: { content.Add(new ByteArrayContent(byteArrayValue), p.Key); continue; }
                            case Stream streamValue: { content.Add(new StreamContent(streamValue), p.Key); continue; }
                            case MultipartFile fileValue:
                            {
                                var stream = fileValue.Stream;
                                if (!stream.CanSeek)
                                {
                                    var memoryStream = new MemoryStream();
                                    await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                                    memoryStream.Position = 0;
                                    stream = memoryStream;
                                }
                                content.Add(new StreamContent(stream), p.Key, fileValue.Filename);
                                continue;
                            }
                            default: throw new InvalidOperationException($"Unsupported param type \"{p.Value.GetType().Name}\"");
                        }
                    }
                }
                restRequest.Content = content;
                return await SendInternalAsync(restRequest, cancelToken, headerOnly).ConfigureAwait(false);
            }
        }

        private async Task<RestResponse> SendInternalAsync(HttpRequestMessage request, CancellationToken cancelToken, bool headerOnly)
        {
            if (!UnstableCheck())
                throw new TimeoutException();

            cancelToken = CancellationTokenSource.CreateLinkedTokenSource(_cancelToken, cancelToken).Token;
            HttpResponseMessage response = await _client.SendAsync(request, cancelToken).ConfigureAwait(false);
            
            var headers = response.Headers.ToDictionary(x => x.Key, x => x.Value.FirstOrDefault(), StringComparer.OrdinalIgnoreCase);
            var stream = !headerOnly ? await response.Content.ReadAsStreamAsync().ConfigureAwait(false) : null;

            return new RestResponse(response.StatusCode, headers, stream);
        }

        private static readonly HttpMethod _patch = new HttpMethod("PATCH");
        private HttpMethod GetMethod(string method)
        {
            switch (method)
            {
                case "DELETE": return HttpMethod.Delete;
                case "GET": return HttpMethod.Get;
                case "PATCH": return _patch;
                case "POST": return HttpMethod.Post;
                case "PUT": return HttpMethod.Put;
                default: throw new ArgumentOutOfRangeException(nameof(method), $"Unknown HttpMethod: {method}");
            }
        }

        private bool UnstableCheck()
        {
            return _rand.NextDouble() > FailureRate;
        }
    }
}
