using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Penguin.Code.Application.Services.Interfaces
{
    public interface IHttpClientService
    {
        void ClearDefaultHeaders();

        Task<HttpResponseMessage> GetAsync(string url, IDictionary<string, string> headers = null);

        Task<HttpResponseMessage> PostAsync(string url, StringContent content = null, Dictionary<string, string> headers = null);

        bool SetDefaultHeaders(IDictionary<string, string> headers);
    }
}