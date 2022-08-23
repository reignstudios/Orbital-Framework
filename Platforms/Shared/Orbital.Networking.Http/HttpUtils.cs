using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orbital.Networking.Http
{
	public enum HttpMethods
	{
		GET,
		POST,
		PUT,
		PATCH,
		DELETE
	}

	public enum HttpContentType
	{
		/// <summary>
		/// No form data
		/// </summary>
		None,

		/// <summary>
		/// JSON data
		/// </summary>
		JSON,

		/// <summary>
		/// 'application/octet-stream'
		/// Used when you're only submitting binary form data
		/// </summary>
		BinaryData,

		/// <summary>
		/// 'application/x-www-form-urlencoded'
		/// Used when you're submitting query-string formated data as text
		/// </summary>
		QueryStringFormData,

		/// <summary>
		/// 'multipart/form-data'
		/// Used when you're submitting multiple form data types
		/// </summary>
		MultiPartFormData
	}

	public class HttpSimpleRequestDesc
	{
		/// <summary>
		/// Body sent as UTF-8 text.
		/// NOTE: If this value is set 'bodyStream' is not used
		/// </summary>
		public string body;

		/// <summary>
		/// The type of content being sent
		/// </summary>
		public string contentType;

		/// <summary>
		/// Request headers
		/// </summary>
		public Dictionary<string, string> headers;
	}

	public class HttpUtilsRequest
	{
		public HttpWebRequest request { get; internal set; }
		public Stream requestStream { get; internal set; }
		private string multiPartBoundary;
		private byte[] multiParyBoundaryLine;

		public void StartMultiPartFormWrite()
		{
			if (multiPartBoundary != null) throw new Exception("'StartMultiPartWrite' can only be called once");

			// open boundaries
			requestStream = request.GetRequestStream();
			multiPartBoundary = $"-------------{Guid.NewGuid()}-------------";
			multiParyBoundaryLine = Encoding.ASCII.GetBytes($"\r\n--{multiPartBoundary}\r\n");
			request.ContentType = "multipart/form-data; boundary=" + multiPartBoundary;// mime type for form + binary data
		}

		public void EndMultiPartFormWrite()
		{
			if (multiPartBoundary == null) throw new Exception("'StartMultiPartWrite' must be called first");

			// close boundaries
			var boundaryLineEnd = Encoding.ASCII.GetBytes($"\r\n--{multiPartBoundary}--");
			requestStream.Write(boundaryLineEnd, 0, boundaryLineEnd.Length);
			requestStream.Close();
		}

		public void WriteMultiPartBoundary()
		{
			requestStream.Write(multiParyBoundaryLine, 0, multiParyBoundaryLine.Length);
		}

		public void WriteMultiPartFormData(Dictionary<string, string> data)
		{
			foreach (var entry in data)
			{
				WriteMultiPartBoundary();
				string formEntry = $"Content-Disposition: form-data; name=\"{entry.Key}\"\r\n\r\n{entry.Value}";
				var formEntryData = Encoding.UTF8.GetBytes(formEntry);
				requestStream.Write(formEntryData, 0, formEntryData.Length);// write header + data
			}
		}

		public void WriteMultiPartFormStream(Stream stream, string formName, string streamFilename)
		{
			WriteMultiPartBoundary();
			string binaryHeader = $"Content-Disposition: form-data; name=\"{formName}\"; filename=\"{streamFilename}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
			var binaryHeaderData = Encoding.UTF8.GetBytes(binaryHeader);
			requestStream.Write(binaryHeaderData, 0, binaryHeaderData.Length);// write header
			stream.CopyTo(requestStream);// write data
		}

		public void WriteMultiPartFormFile(string filename, string formName = null)
		{
			if (string.IsNullOrEmpty(formName)) formName = Path.GetFileNameWithoutExtension(formName);
			using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				WriteMultiPartFormStream(stream, formName, filename);
			}
		}

		public void WriteFormDataAsQueryString(string queryString)
		{
			request.ContentType = "application/x-www-form-urlencoded";// mine type for form only data as query-string
			using (var stream = request.GetRequestStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(queryString);
			}
		}

		public void StartSinglePartWrite()
		{
			requestStream = request.GetRequestStream();
		}

		public void EndSinglePartWrite()
		{
			requestStream.Close();// no form data so close stream
		}

		public void WriteStream(Stream stream)
		{
			request.ContentType = "application/octet-stream";// mime type for binary data
			stream.CopyTo(requestStream);// write data
		}

		public void WriteFile(string filename)
		{
			using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				WriteStream(stream);
			}
		}
	}

	public static class HttpUtils
	{
		public static string GetCommonContentType(HttpContentType contentType)
		{
			switch (contentType)
			{
				case HttpContentType.None: return null;
				case HttpContentType.JSON: return "application/json";
				case HttpContentType.BinaryData: return "application/octet-stream";
				case HttpContentType.QueryStringFormData: return "application/x-www-form-urlencoded";
				case HttpContentType.MultiPartFormData: return "multipart/form-data";
				default: throw new NotImplementedException("HttpContentType: " + contentType.ToString());
			}
		}

		public static HttpUtilsRequest RequestStart(string url, HttpMethods method, int timeout = 60)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method.ToString();
			request.Timeout = timeout * 1000;

			var result = new HttpUtilsRequest();
			result.request = request;

			return result;
		}

		public static WebResponse RequestEnd(HttpUtilsRequest request)
		{
			return request.request.GetResponse();
		}

		public static string RequestEnd_WithResultAsText(HttpUtilsRequest request)
		{
			using (var response = request.request.GetResponse())
			using (var responseStream = response.GetResponseStream())
			using (var reader = new StreamReader(responseStream, Encoding.UTF8))
			{
				string result = reader.ReadToEnd();
				responseStream.Close();
				response.Close();
				return result;
			}
		}

		public static T RequestEnd_DeserializedJson<T>(HttpUtilsRequest request, bool propertyNameCaseInsensitive = true)
		{
			string json = RequestEnd_WithResultAsText(request);
			var options = new JsonSerializerOptions();
			options.PropertyNameCaseInsensitive = propertyNameCaseInsensitive;
			return JsonSerializer.Deserialize<T>(json, options);
		}

		public static string GetErrorText(WebException e, out HttpWebResponse response)
		{
			response = null;
			if (e.Response == null) return string.Empty;
			if (e.Response is HttpWebResponse httpResponse)
			{
				response = httpResponse;
				return httpResponse.GetResponseAsText();
			}
			else
			{
				using (var responseStream = e.Response.GetResponseStream())
				using (var reader = new StreamReader(responseStream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public static string GetErrorText(Exception e, out HttpWebResponse response)
		{
			response = null;
			if (e is WebException) return GetErrorText((WebException)e, out response);
			else return e.Message;
		}

		public static string GetErrorText(Exception e)
		{
			string result = GetErrorText(e, out var response);
			if (response != null) response.Dispose();
			return result;
		}

		/// <summary>
		/// Make an http request
		/// </summary>
		/// <param name="url">Request url</param>
		/// <param name="method">Request method</param>
		/// <param name="desc">(Optional) Request meta data</param>
		/// <param name="timeout">How long to wait for response in sec</param>
		/// <returns>Http Response</returns>
		public static HttpWebResponse SimpleRequest(string url, HttpMethods method, HttpSimpleRequestDesc desc = null, int timeout = 60)
		{
			var request = WebRequest.CreateHttp(url);
			request.Method = method.ToString();
			request.Timeout = timeout * 1000;

			if (desc != null)
			{
				// add headers
				if (desc.headers != null)
				{
					foreach (var header in desc.headers) request.Headers.Add(header.Key, header.Value);
				}

				// set body meta data
				if (!string.IsNullOrEmpty(desc.body) && !string.IsNullOrEmpty(desc.contentType))
				{
					request.ContentType = desc.contentType;
					var data = Encoding.UTF8.GetBytes(desc.body);
					request.ContentLength = data.Length;
					using (var stream = request.GetRequestStream())
					{
						stream.Write(data, 0, data.Length);
					}
				}
			}

			// submit request
			return (HttpWebResponse)request.GetResponse();
		}

		/// <summary>
		/// Make an http request
		/// </summary>
		/// <param name="url">Request url</param>
		/// <param name="method">Request method</param>
		/// <param name="desc">(Optional) Request meta data</param>
		/// <param name="timeout">How long to wait for response in sec</param>
		/// <returns>Http Response</returns>
		public static async Task<HttpWebResponse> SimpleRequestAsync(string url, HttpMethods method, HttpSimpleRequestDesc desc = null, int timeout = 60)
		{
			var request = WebRequest.CreateHttp(url);
			request.Method = method.ToString();
			request.Timeout = timeout * 1000;

			if (desc != null)
			{
				// add headers
				if (desc.headers != null)
				{
					foreach (var header in desc.headers) request.Headers.Add(header.Key, header.Value);
				}

				// set body meta data
				if (!string.IsNullOrEmpty(desc.body) && !string.IsNullOrEmpty(desc.contentType))
				{
					request.ContentType = desc.contentType;
					var data = Encoding.UTF8.GetBytes(desc.body);
					request.ContentLength = data.Length;
					using (var stream = await request.GetRequestStreamAsync())
					{
						await stream.WriteAsync(data, 0, data.Length);
					}
				}
			}

			// submit request
			return (HttpWebResponse)await request.GetResponseAsync();
		}

		public static string FormatUrl(HttpProtocals protocal, string host, int port)
		{
			return $"{protocal.ToString().ToLower()}://{host}:{port}/";
		}
	}
}
