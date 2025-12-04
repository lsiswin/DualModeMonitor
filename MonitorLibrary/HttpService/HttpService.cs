using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MonitorLibrary.HttpService
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private string _baseAddress;

        public HttpService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<T> DeleteAsync<T>(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(GetFullUrl(endpoint));
            return await HandleResponse<T>(response);
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(GetFullUrl(endpoint));
            return await HandleResponse<T>(response);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var content = CreateJsonContent(data);
            var response = await _httpClient.PostAsync(GetFullUrl(endpoint), content);
            return await HandleResponse<T>(response);
        }

        /// <summary>
        /// 解析完整的URL地址
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private string GetFullUrl(string endpoint)
        {
            if (endpoint.StartsWith("http"))
                return endpoint;

            return $"{_baseAddress}{endpoint.TrimStart('/')}";
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            var content = CreateJsonContent(data);
            var response = await _httpClient.PutAsync(GetFullUrl(endpoint), content);
            return await HandleResponse<T>(response);
        }

        /// <summary>
        /// 设置认证令牌
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="NotImplementedException"></exception>

        public void SetAuthenticationToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
            }
        }

        /// <summary>
        /// 设置基础地址
        /// </summary>
        /// <param name="baseAddress"></param>
        public void SetBaseAddress(string baseAddress)
        {
            _baseAddress = baseAddress;
            if (!_baseAddress.EndsWith("/"))
                _baseAddress += "/";
        }

        /// <summary>
        /// 创建JSON内容
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private StringContent CreateJsonContent(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// 处理HTTP响应
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"HTTP request failed with status code {response.StatusCode}. "
                        + $"Response: {errorContent}"
                );
            }
        }
    }
}
