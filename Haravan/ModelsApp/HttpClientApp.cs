using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Haravan.ModelsApp
{
    public class HttpClientApp
    {   
        public HttpClient httpClient;
        //===================================Contructor======================================================
        //=========================================================================================
        public HttpClientApp()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }
        public HttpClientApp(string token)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
        public void SetAuthorizationBasic(string token)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {token}");
        }
        //=====================================Header====================================================
        //=========================================================================================
        public void SetAuthorization(string token)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
        public void RemoveAuthorization(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
        }
        public void ClearHeader()
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
        public void AddHeader(string key , string value)
        {
            httpClient.DefaultRequestHeaders.Add(key, value);
        }
        public void RemoveHeader(string key)
        {
            httpClient.DefaultRequestHeaders.Remove(key);
        }
        //===================================Method=======================================================
        //=========================================================================================
        public async Task<ResponseApiHaravan> Get_Request(string url)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Get;
                httpRequestMessage.RequestUri = new Uri(url);


                var response = await httpClient.SendAsync(httpRequestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                if (response.IsSuccessStatusCode)
                    return new ResponseApiHaravan("ok", "", responseContent);
                else 
                return new ResponseApiHaravan("err", response.StatusCode.ToString(), responseContent); 
            }
            catch (Exception ex)
            {
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Error(ex.Message);
                ResponseApiHaravan res = new ResponseApiHaravan("err", ex.Message, "");
                return res; 
            }
        }
        public async Task<ResponseApiHaravan> Get_Request_WithBody(string url,string body)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Get;
                httpRequestMessage.RequestUri = new Uri(url);

                // Tạo StringContent
                var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
                httpRequestMessage.Content = httpContent;

                var response = await httpClient.SendAsync(httpRequestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Info($"Call Api haravan : {url}");
                if (response.IsSuccessStatusCode)
                    return new ResponseApiHaravan("ok", "", responseContent);
                else
                    return new ResponseApiHaravan("err", response.StatusCode.ToString(), responseContent);
            }
            catch (Exception ex)
            {
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Error(ex.Message);
                ResponseApiHaravan res = new ResponseApiHaravan("err", ex.Message, "");
                return res;
            }
        }

        public async Task<ResponseApiHaravan> Post_Request_WithBody(string url, string body)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.RequestUri = new Uri(url);

                var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
                httpRequestMessage.Content = httpContent;

                var response = await httpClient.SendAsync(httpRequestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Info($"Call Api haravan : {url}");
                if (response.IsSuccessStatusCode)
                    return new ResponseApiHaravan("ok", "", responseContent);
                else
                    return new ResponseApiHaravan("err", response.StatusCode.ToString(), responseContent);
            }
            catch (Exception ex)
            {
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Error(ex.Message);
                ResponseApiHaravan res = new ResponseApiHaravan("err", ex.Message, ex.ToString());
                return res;
            }
        }

        public async Task<ResponseApiHaravan> Post_Request_FormData(string url, MultipartFormDataContent body)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.RequestUri = new Uri(url);

                httpRequestMessage.Content = body;
                var response = await httpClient.SendAsync(httpRequestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Info($"Call Api haravan : {url}");
                if (response.IsSuccessStatusCode)
                    return new ResponseApiHaravan("ok", "", responseContent);
                else
                    return new ResponseApiHaravan("err", response.StatusCode.ToString(), responseContent);
            }
            catch (Exception ex)
            {
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Error(ex.Message);
                ResponseApiHaravan res = new ResponseApiHaravan("err", ex.Message, "");
                return res;
            }
        }

        public async Task<ResponseApiHaravan> Put_Request_WithBody(string url, string body)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Put;
                httpRequestMessage.RequestUri = new Uri(url);

                var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
                httpRequestMessage.Content = httpContent;

                var response = await httpClient.SendAsync(httpRequestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Info($"Call Api haravan : {url}");
                if (response.IsSuccessStatusCode)
                    return new ResponseApiHaravan("ok", "", responseContent);
                else
                    return new ResponseApiHaravan("err", response.StatusCode.ToString(), responseContent);
            }
            catch (Exception ex)
            {
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Error(ex.Message);
                ResponseApiHaravan res = new ResponseApiHaravan("err", ex.Message, "");
                return res;
            }
        }

        public async Task<ResponseApiHaravan> Del_Request(string url)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Delete;
                httpRequestMessage.RequestUri = new Uri(url);

                var response = await httpClient.SendAsync(httpRequestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Info($"Call Api haravan : {url}");
                if (response.IsSuccessStatusCode)
                    return new ResponseApiHaravan("ok", "", responseContent);
                else
                    return new ResponseApiHaravan("err", response.StatusCode.ToString(), responseContent);
            }
            catch (Exception ex)
            {
                ILog log = Logger.GetLog(typeof(HttpClientApp));
                log.Error(ex.Message);
                ResponseApiHaravan res = new ResponseApiHaravan("err", ex.Message, "");
                return res;
            }
        }
    }
}
