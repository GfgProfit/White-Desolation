using System;
using System.IO;
using System.Net;
using System.Text;

public sealed class ServerHttpResponse
{
    public bool Success;
    public int StatusCode;
    public string Body;
    public string Error;
}

public static class ServerHttpClient
{
    private const int TimeoutMilliseconds = 5000;

    public static ServerHttpResponse Send(string method, string url, string jsonBody = null)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Timeout = TimeoutMilliseconds;
            request.ReadWriteTimeout = TimeoutMilliseconds;
            request.Accept = "application/json";

            if (!string.IsNullOrEmpty(jsonBody))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(jsonBody);
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = bytes.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                return BuildResponse(response, null);
            }
        }
        catch (WebException exception)
        {
            if (exception.Response is HttpWebResponse response)
            {
                return BuildResponse(response, exception.Message);
            }

            return new ServerHttpResponse
            {
                Success = false,
                StatusCode = 0,
                Body = string.Empty,
                Error = exception.Message
            };
        }
        catch (Exception exception)
        {
            return new ServerHttpResponse
            {
                Success = false,
                StatusCode = 0,
                Body = string.Empty,
                Error = exception.Message
            };
        }
    }

    public static string EscapePathSegment(string value)
    {
        return Uri.EscapeDataString(value ?? string.Empty);
    }

    private static ServerHttpResponse BuildResponse(HttpWebResponse response, string fallbackError)
    {
        string body = ReadResponseBody(response);
        int statusCode = (int)response.StatusCode;

        return new ServerHttpResponse
        {
            Success = statusCode >= 200 && statusCode <= 299,
            StatusCode = statusCode,
            Body = body,
            Error = fallbackError
        };
    }

    private static string ReadResponseBody(HttpWebResponse response)
    {
        Stream stream = response.GetResponseStream();

        if (stream == null)
        {
            return string.Empty;
        }

        using (stream)
        using (StreamReader reader = new(stream, Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }
}
