using BumbleBee.Code.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _client;
        private readonly ILogger<HttpClientService> _logger;

        public HttpClientService(ILogger<HttpClientService> logger)
        {
            _client = new HttpClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public HttpClientService(ILogger<HttpClientService> logger, IDictionary<string, string> headers)
        {
            _client = new HttpClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool SetDefaultHeaders(IDictionary<string, string> headers)
        {
            try
            {
                foreach (var header in headers)
                {
                    _client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to update default headers. Encountered the following exception : {ex.Message}, stack trace : {ex.StackTrace}");
                return false;
            }
        }

        public Task<HttpResponseMessage> GetAsync(string url, IDictionary<string, string> headers = null)
        {
            if (headers != null)
            {
                SetDefaultHeaders(headers);
            }

            var response = _client.GetAsync(url);
            ClearDefaultHeaders();

            return response;
        }

        public void ClearDefaultHeaders()
        {
            _client.DefaultRequestHeaders.Clear();
        }

        public async Task<HttpResponseMessage> PostAsync(string url, StringContent content = null, Dictionary<string, string> headers = null)
        {
            HttpRequestMessage message = new HttpRequestMessage();

            if (headers != null)
            {
                SetHeadersInHttpRequest(headers, message);
            }

            message.RequestUri = new Uri(url);
            message.Method = HttpMethod.Post;
            message.Content = content;

            var response = await _client.SendAsync(message);

            return response;
        }

        private void SetHeadersInHttpRequest(Dictionary<string, string> headers, HttpRequestMessage message)
        {
            message.Headers.Clear();

            foreach (var item in headers)
            {
                message.Headers.Add(item.Key, item.Value);
            }
        }
    }
}