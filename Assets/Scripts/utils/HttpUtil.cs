using UnityEngine;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class HttpUtil
{
    private static HttpClient client;

    static HttpUtil()
    {
        client = new HttpClient();
        client.Timeout = System.TimeSpan.FromSeconds(30);
    }

    public static async Task<string> GetAsync(string url, Dictionary<string, string> headers = null)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            Debug.Log($"发送GET请求: {url}");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log($"GET请求成功，响应长度: {responseBody.Length} 字符");
            return responseBody;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GET请求失败: {e.Message}");
            return null;
        }
    }

    public static async Task<string> PostAsync(string url, string content, string contentType = "application/json", Dictionary<string, string> headers = null)
    {
        try
        {
            HttpContent httpContent = new StringContent(content);
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = httpContent
            };
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            Debug.Log($"发送POST请求: {url}");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log($"POST请求成功，响应长度: {responseBody.Length} 字符");
            return responseBody;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"POST请求失败: {e.Message}");
            return null;
        }
    }

    public static async Task<string> PutAsync(string url, string content, string contentType = "application/json", Dictionary<string, string> headers = null)
    {
        try
        {
            HttpContent httpContent = new StringContent(content);
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = httpContent
            };
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            Debug.Log($"发送PUT请求: {url}");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log($"PUT请求成功，响应长度: {responseBody.Length} 字符");
            return responseBody;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PUT请求失败: {e.Message}");
            return null;
        }
    }

    public static async Task<string> DeleteAsync(string url, Dictionary<string, string> headers = null)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            Debug.Log($"发送DELETE请求: {url}");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log($"DELETE请求成功，响应长度: {responseBody.Length} 字符");
            return responseBody;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DELETE请求失败: {e.Message}");
            return null;
        }
    }

    public static void Dispose()
    {
        client?.Dispose();
    }
}

public class HttpUtilExample : MonoBehaviour
{
    public string testUrl = "http://localhost/posts";

    private async void Start()
    {
        // 示例：发送GET请求
        string response = await HttpUtil.GetAsync(testUrl);
        if (response != null)
        {
            Debug.Log($"GET请求响应: {response.Substring(0, Mathf.Min(response.Length, 200))}...");
        }

        // 示例：发送POST请求
        string postData = "{\"title\":\"测试\",\"body\":\"这是一个测试请求\",\"userId\":1}";
        string postResponse = await HttpUtil.PostAsync("http://localhost/posts", postData);
        if (postResponse != null)
        {
            Debug.Log($"POST请求响应: {postResponse.Substring(0, Mathf.Min(postResponse.Length, 200))}...");
        }
    }
}
