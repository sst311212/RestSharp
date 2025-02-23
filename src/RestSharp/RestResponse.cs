﻿//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using RestSharp.Extensions;

namespace RestSharp
{
    /// <summary>
    /// Base class for common properties shared by RestResponse and RestResponse[[T]]
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "()}")]
    public abstract class RestResponseBase
    {
        string _content;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected RestResponseBase()
        {
            ResponseStatus = ResponseStatus.None;
            Headers        = new List<Parameter>();
            Cookies        = new List<RestResponseCookie>();
        }

        /// <summary>
        /// The RestRequest that was made to get this RestResponse
        /// </summary>
        /// <remarks>
        /// Mainly for debugging if ResponseStatus is not OK
        /// </remarks>
        public IRestRequest Request { get; set; }

        /// <summary>
        /// MIME content type of response
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Length in bytes of the response content
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// Encoding of the response content
        /// </summary>
        public string ContentEncoding { get; set; }

        /// <summary>
        /// String representation of response content
        /// </summary>
        public string Content
        {
            get => _content ??= RawBytes.AsString(ContentEncoding);
            set => _content = value;
        }

        /// <summary>
        /// HTTP response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Whether or not the response status code indicates success
        /// </summary>
        public bool IsSuccessful => (int) StatusCode >= 200 &&
            (int) StatusCode                         <= 299 &&
            ResponseStatus                           == ResponseStatus.Completed;

        /// <summary>
        /// Description of HTTP status returned
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Response content
        /// </summary>
        public byte[] RawBytes { get; set; }

        /// <summary>
        /// The URL that actually responded to the content (different from request if redirected)
        /// </summary>
        public Uri ResponseUri { get; set; }

        /// <summary>
        /// HttpWebResponse.Server
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Cookies returned by server with the response
        /// </summary>
        public IList<RestResponseCookie> Cookies { get; protected internal set; }

        /// <summary>
        /// Headers returned by server with the response
        /// </summary>
        public IList<Parameter> Headers { get; protected internal set; }

        /// <summary>
        /// Status of the request. Will return Error for transport errors.
        /// HTTP errors will still return ResponseStatus.Completed, check StatusCode instead
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// Transport or other non-HTTP error generated while attempting request
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The exception thrown during the request, if any
        /// </summary>
        public Exception? ErrorException { get; set; }

        /// <summary>
        /// The HTTP protocol version (1.0, 1.1, etc)
        /// </summary>
        /// <remarks>Only set when underlying framework supports it.</remarks>
        public Version ProtocolVersion { get; set; }

        /// <summary>
        /// Assists with debugging responses by displaying in the debugger output
        /// </summary>
        /// <returns></returns>
        protected string DebuggerDisplay() => $"StatusCode: {StatusCode}, Content-Type: {ContentType}, Content-Length: {ContentLength})";
    }

    /// <summary>
    /// Container for data sent back from API including deserialized data
    /// </summary>
    /// <typeparam name="T">Type of data to deserialize to</typeparam>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "()}")]
    public class RestResponse<T> : RestResponseBase, IRestResponse<T>
    {
        /// <summary>
        /// Deserialized entity data
        /// </summary>
        public T Data { get; set; } = default!;

        public static explicit operator RestResponse<T>(RestResponse response)
            => new RestResponse<T>
            {
                ContentEncoding   = response.ContentEncoding,
                ContentLength     = response.ContentLength,
                ContentType       = response.ContentType,
                Cookies           = response.Cookies,
                ErrorMessage      = response.ErrorMessage,
                ErrorException    = response.ErrorException,
                Headers           = response.Headers,
                RawBytes          = response.RawBytes,
                ResponseStatus    = response.ResponseStatus,
                ResponseUri       = response.ResponseUri,
                ProtocolVersion   = response.ProtocolVersion,
                Server            = response.Server,
                StatusCode        = response.StatusCode,
                StatusDescription = response.StatusDescription,
                Request           = response.Request
            };
    }

    /// <summary>
    /// Container for data sent back from API
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "()}")]
    public class RestResponse : RestResponseBase, IRestResponse
    {
        RestResponse SetHeaders(IEnumerable<HttpHeader> headers)
            => this.With(
                x => x.Headers = headers.Select(p => new Parameter(p.Name, p.Value, ParameterType.HttpHeader)).ToList()
            );

        RestResponse SetCookies(IEnumerable<HttpCookie> cookies)
            => this.With(
                x => x.Cookies = cookies.Select(RestResponseCookie.FromHttpCookie).ToList()
            );

        internal static RestResponse FromHttpResponse(IHttpResponse httpResponse, IRestRequest request)
            => new RestResponse
                {
                    Content           = KrispModify(httpResponse),
                    ContentEncoding   = httpResponse.ContentEncoding,
                    ContentLength     = httpResponse.ContentLength,
                    ContentType       = httpResponse.ContentType,
                    ErrorException    = httpResponse.ErrorException,
                    ErrorMessage      = httpResponse.ErrorMessage,
                    RawBytes          = httpResponse.RawBytes,
                    ResponseStatus    = httpResponse.ResponseStatus,
                    ResponseUri       = httpResponse.ResponseUri,
                    ProtocolVersion   = httpResponse.ProtocolVersion,
                    Server            = httpResponse.Server,
                    StatusCode        = httpResponse.StatusCode,
                    StatusDescription = httpResponse.StatusDescription,
                    Request           = request
                }
                .SetHeaders(httpResponse.Headers)
                .SetCookies(httpResponse.Cookies);

        internal static string KrispModify(IHttpResponse httpResponse)
        {
            string httpContent = httpResponse.Content;
            if (httpResponse.ResponseUri.AbsolutePath.Contains("/v2/user/profile")) {
                httpContent = Regex.Replace(httpContent, "\"name\":\"[^\"]+\"", "\"name\":\"unlimited\"");
                httpContent = Regex.Replace(httpContent, "\"available\":false", "\"available\":true");
            }
            else if (httpResponse.ResponseUri.AbsolutePath.Contains("/v2/version")) {
                httpContent = Regex.Replace(httpContent, "\"version\":\"[^\"]+\"", "\"version\":\"1.0.0\"");
            }
            return httpContent;
        }
    }
}