
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MFUtility.Helpers
{
    public class HttpHelper
    {
        private static readonly HttpClient _httpClient;
        private static readonly JsonSerializerSettings _jsonSettings;

        static HttpHelper()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // 设置默认超时时间

            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
        }

        #region ** GET 请求 **

        public static async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            var response = await SendRequestAsync(HttpMethod.Get, url, null, headers);
            return await DeserializeResponse<T>(response);
        }

        public static async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null)
        {
            var response = await SendRequestAsync(HttpMethod.Get, url, null, headers);
            return await response.Content.ReadAsStringAsync();
        }

        #endregion

        #region ** POST 请求 **

        public static async Task<T?> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            var response = await SendRequestAsync(HttpMethod.Post, url, data, headers);
            return await DeserializeResponse<T>(response);
        }

        public static async Task<string> PostAsync(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            var response = await SendRequestAsync(HttpMethod.Post, url, data, headers);
            return await response.Content.ReadAsStringAsync();
        }

        #endregion

        #region ** PUT 请求 **

        public static async Task<T?> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            var response = await SendRequestAsync(HttpMethod.Put, url, data, headers);
            return await DeserializeResponse<T>(response);
        }

        public static async Task<string> PutAsync(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            var response = await SendRequestAsync(HttpMethod.Put, url, data, headers);
            return await response.Content.ReadAsStringAsync();
        }

        #endregion

        #region ** DELETE 请求 **

        public static async Task<T?> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, url, null, headers);
            return await DeserializeResponse<T>(response);
        }

        public static async Task<string> DeleteAsync(string url, Dictionary<string, string>? headers = null)
        {
            var response = await SendRequestAsync(HttpMethod.Delete, url, null, headers);
            return await response.Content.ReadAsStringAsync();
        }

        #endregion

        #region ** 请求通用方法 **

        private static async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            using var requestMessage = new HttpRequestMessage(method, url);

            // 设置请求头
            requestMessage.Headers.Clear();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // 如果有请求数据（POST/PUT）
            if (data != null)
            {
                var jsonData = JsonConvert.SerializeObject(data, _jsonSettings);
                requestMessage.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            }

            try
            {
                // 执行请求
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode(); // 如果响应码不是2xx，则抛出异常
                return response;
            }
            catch (Exception ex)
            {
                // 这里可以做错误处理，抛出异常或重试
                throw new Exception($"请求失败: {ex.Message}");
            }
        }

        private static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
                return default;

            try
            {
                return JsonConvert.DeserializeObject<T>(responseContent, _jsonSettings);
            }
            catch (JsonException ex)
            {
                // 可以自定义异常处理
                throw new Exception($"反序列化失败: {ex.Message}");
            }
        }

        #endregion

        #region ** 取消请求 **

        public static void CancelRequests()
        {
            _httpClient.CancelPendingRequests();
        }

        #endregion
    }
}
