﻿using System;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;
using Newtonsoft.Json;

namespace Griffin.Cqs.Http;

/// <summary>
///     Will send Command/Query objects over HTTP to a server somewhere.
/// </summary>
public class CqsHttpClient : IMessageBus, IQueryBus
{
    private readonly Uri _webApiUri;
    private string _commandUri;
    private string _eventUri;
    private string _queryUri;
    private string _requestUri;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CqsHttpClient" /> class.
    /// </summary>
    /// <param name="uri">Uri to the HTTP server that can receive and process the CQS objects.</param>
    /// <exception cref="System.ArgumentNullException">uri</exception>
    public CqsHttpClient(Uri uri)
    {
        if (uri == null) throw new ArgumentNullException("uri");
        _webApiUri = uri;
        _commandUri = _requestUri = _eventUri = _queryUri = "";
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CqsHttpClient" /> class.
    /// </summary>
    /// <param name="uri">Uri to the HTTP server that can receive and process the CQS objects.</param>
    /// <exception cref="System.ArgumentNullException">uri</exception>
    public CqsHttpClient(string uri)
    {
        if (uri == null) throw new ArgumentNullException("uri");
        _webApiUri = new Uri(uri);
        _commandUri = _requestUri = _eventUri = _queryUri = "";
    }

    /// <summary>
    ///     Sub path for commands (i.e. the full uri is <code>Uri + CommandUri</code>).
    /// </summary>
    /// <value>Default is empty.</value>
    public string CommandUri
    {
        get => _commandUri;
        set => _commandUri = value ?? "";
    }

    /// <summary>
    ///     Will use the internal JSON serializer if this property is not specified.
    /// </summary>
    public ICqsDeserializer CqsSerializer { get; set; }

    /// <summary>
    ///     Sub path for events (i.e. the full uri is <code>Uri + EventUri</code>).
    /// </summary>
    /// <value>Default is empty.</value>
    public string EventUri
    {
        get => _eventUri;
        set => _eventUri = value ?? "";
    }

    /// <summary>
    ///     Sub path for queries (i.e. the full uri is <code>Uri + QueryUri</code>).
    /// </summary>
    /// <value>Default is empty.</value>
    public string QueryUri
    {
        get => _queryUri;
        set => _queryUri = value ?? "";
    }

    /// <summary>
    ///     Can be used to add additional headers etc.
    /// </summary>
    /// <remarks>
    ///     The header <c>X-Cqs-Type</c> contains that .NET type for the CQS object which is about to be sent. The body
    ///     contains the object as JSON.
    /// </remarks>
    public Action<HttpWebRequest> RequestDecorator { get; set; }

    /// <summary>
    ///     Sub path for request/reply (i.e. the full uri is <code>Uri + RequestUri</code>).
    /// </summary>
    /// <value>Default is empty.</value>
    public string RequestUri
    {
        get => _requestUri;
        set => _requestUri = value ?? "";
    }

    public Task SendAsync(ClaimsPrincipal principal, object message)
    {
        return SendAsync(new Message(message));
    }

    public Task SendAsync(ClaimsPrincipal principal, Message message)
    {
        return SendAsync(message);
    }

    public async Task SendAsync(Message message)
    {
        await DoRequestAsync("MESSAGE", "POST", _webApiUri + _requestUri, message);
    }

    public Task SendAsync(object message)
    {
        return SendAsync(new Message(message));
    }

    public async Task<TResult> QueryAsync<TResult>(ClaimsPrincipal principal, Query<TResult> query)
    {
        return (TResult)await DoRequestAsync("QUERY", "POST", _webApiUri + _queryUri, query);
    }

    /// <summary>
    ///     Send the query to the http server and wait for the result
    /// </summary>
    /// <typeparam name="TResult">Type of result that the query will return</typeparam>
    /// <param name="query">Query to execute.</param>
    /// <returns>
    ///     Task which will complete once we've got the result (or something failed, like a query wait timeout).
    /// </returns>
    public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
    {
        return (TResult)await DoRequestAsync("QUERY", "POST", _webApiUri + _queryUri, query);
    }

    private async Task<object> DeserializeBody(HttpWebResponse response)
    {
        var responseTypeName = response.Headers["X-Cqs-Object-Type"];
        if (responseTypeName == null)
            return null;

        var responseStream = response.GetResponseStream();
        var reader = new StreamReader(responseStream);
        var responseJson = await reader.ReadToEndAsync();
        var type = Type.GetType(responseTypeName, false);
        if (type == null)
            throw new InvalidOperationException("Failed to load type " + responseTypeName);

        return CqsSerializer == null
            ? JsonConvert.DeserializeObject(responseJson, type)
            : CqsSerializer.Deserialize(type, responseJson);
    }

    private async Task<object> DoRequestAsync(string cqsType, string httpMethod, string uri, object cqsObject)
    {
        var json = cqsObject.ToString();
        HttpWebResponse response = null;
        Exception innerException;
        var errorDescription = "";
        try
        {
            var request = WebRequest.CreateHttp(uri);
            request.Method = httpMethod;
            request.Headers["X-Cqs-Object-Type"] = cqsObject.GetType().GetSimpleAssemblyQualifiedName();
            request.Headers["X-Cqs-Type"] = cqsType;

            var contentType = "application/json;encoding=utf8";
            json = CqsSerializer == null
                ? JsonConvert.SerializeObject(cqsObject)
                : CqsSerializer.Serialize(cqsObject, out contentType);
            request.ContentType = contentType;

            var jsonBuffer = Encoding.UTF8.GetBytes(json);
            var stream = await request.GetRequestStreamAsync();
            stream.Write(jsonBuffer, 0, jsonBuffer.Length);
            stream.Close();

            if (RequestDecorator != null)
                RequestDecorator(request);

            response = (HttpWebResponse)await request.GetResponseAsync();
            return await DeserializeBody(response);
        }
        catch (Exception exception)
        {
            innerException = exception;
            var webEx = exception as WebException;
            if (webEx != null)
            {
                response = (HttpWebResponse)webEx.Response;
                if (response != null)
                {
                    if (response.ContentLength > 0)
                    {
                        var stream = response.GetResponseStream();
                        var reader = new StreamReader(stream, Encoding.UTF8);
                        errorDescription = reader.ReadToEnd();
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return null;
                }
            }

            if (response == null)
                throw new WebException(
                    uri + " failed to process " + cqsObject.GetType().Name + " " + json + "\r\n" + errorDescription,
                    exception);
        }

        var ex = (Exception)await DeserializeBody(response);
        if (ex == null) throw new Exception("Failed to handle " + json + "\r\n" + errorDescription, innerException);

        throw ex;
    }
}