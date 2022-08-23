using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Orbital.Networking.Http
{
	public enum HttpProtocals
	{
		Http,
		Https
	}

	public class HttpActiveListener
	{
		public string url, user;
	}

	/// <summary>
	/// Route callback
	/// </summary>
	/// <param name="sender">HttpServer calling this method</param>
	/// <param name="context">Http Context for this route</param>
	/// <param name="route">Route name being invoked</param>
	/// <param name="statusCode">Required status code</param>
	public delegate void HttpRouteCallback(HttpServer sender, HttpListenerContext context, string route, out HttpStatusCode statusCode);

	/// <summary>
	/// Call when you're done with a WebSocket context
	/// </summary>
	/// <param name="statusCode">Required status code</param>
	public delegate void HttpWebSocketEndpointDoneCallback(HttpStatusCode statusCode);

	/// <summary>
	/// WebSocket endpoint callback
	/// </summary>
	/// <param name="sender">HttpServer calling this endpoint</param>
	/// <param name="context">Http WebSocket Context for this route</param>
	/// <param name="endpoint">Endpoint name being invoked</param>
	/// <param name="statusCode">Required status code</param>
	public delegate void HttpWebSocketEndpointCallback(HttpServer sender, HttpListenerWebSocketContext context, string endpoint, HttpWebSocketEndpointDoneCallback doneCallback);

	public class HttpServer : IDisposable
	{
		public readonly int port;
		public readonly string rootResourcePath;
		private HttpListener listener;
		public readonly List<string> indexFiles;
		public readonly Dictionary<string, string> mimeTypes;

		private Dictionary<string, HttpRouteCallback> routes;
		private Dictionary<string, HttpWebSocketEndpointCallback> webSocketEndpoints;

		public delegate void InternalErrorCallbackMethod(Exception e);
		public event InternalErrorCallbackMethod InternalErrorCallback;

		public HttpServer(HttpProtocals protocal, int port, string rootResourcePath)
		: this(protocal, "+", port, rootResourcePath)
		{ }

		public HttpServer(HttpProtocals protocal, string host, int port, string rootResourcePath)
		{
			this.port = port;
			this.rootResourcePath = rootResourcePath;
			listener = new HttpListener();
			listener.Prefixes.Add(HttpUtils.FormatUrl(protocal, host, port));
			routes = new Dictionary<string, HttpRouteCallback>();
			webSocketEndpoints = new Dictionary<string, HttpWebSocketEndpointCallback>();

			indexFiles = new List<string>()
			{
				"index.html",
				"index.htm"
			};

			mimeTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
			{
				{".asf", "video/x-ms-asf"},
				{".asx", "video/x-ms-asf"},
				{".avi", "video/x-msvideo"},
				{".bin", "application/octet-stream"},
				{".cco", "application/x-cocoa"},
				{".crt", "application/x-x509-ca-cert"},
				{".css", "text/css"},
				{".deb", "application/octet-stream"},
				{".der", "application/x-x509-ca-cert"},
				{".dll", "application/octet-stream"},
				{".dmg", "application/octet-stream"},
				{".ear", "application/java-archive"},
				{".eot", "application/octet-stream"},
				{".exe", "application/octet-stream"},
				{".flv", "video/x-flv"},
				{".gif", "image/gif"},
				{".hqx", "application/mac-binhex40"},
				{".htc", "text/x-component"},
				{".htm", "text/html"},
				{".html", "text/html"},
				{".ico", "image/x-icon"},
				{".img", "application/octet-stream"},
				{".iso", "application/octet-stream"},
				{".jar", "application/java-archive"},
				{".jardiff", "application/x-java-archive-diff"},
				{".jng", "image/x-jng"},
				{".jnlp", "application/x-java-jnlp-file"},
				{".jpeg", "image/jpeg"},
				{".jpg", "image/jpeg"},
				{".js", "application/x-javascript"},
				{".mml", "text/mathml"},
				{".mng", "video/x-mng"},
				{".mov", "video/quicktime"},
				{".mp3", "audio/mpeg"},
				{".mpeg", "video/mpeg"},
				{".mpg", "video/mpeg"},
				{".msi", "application/octet-stream"},
				{".msm", "application/octet-stream"},
				{".msp", "application/octet-stream"},
				{".pdb", "application/x-pilot"},
				{".pdf", "application/pdf"},
				{".pem", "application/x-x509-ca-cert"},
				{".pl", "application/x-perl"},
				{".pm", "application/x-perl"},
				{".png", "image/png"},
				{".svg", "image/svg+xml"},
				{".prc", "application/x-pilot"},
				{".ra", "audio/x-realaudio"},
				{".rar", "application/x-rar-compressed"},
				{".rpm", "application/x-redhat-package-manager"},
				{".rss", "text/xml"},
				{".run", "application/x-makeself"},
				{".sea", "application/x-sea"},
				{".shtml", "text/html"},
				{".sit", "application/x-stuffit"},
				{".swf", "application/x-shockwave-flash"},
				{".tcl", "application/x-tcl"},
				{".tk", "application/x-tcl"},
				{".txt", "text/plain"},
				{".war", "application/java-archive"},
				{".wbmp", "image/vnd.wap.wbmp"},
				{".wmv", "video/x-ms-wmv"},
				{".woff", "font/woff"},
				{".woff2", "font/woff2"},
				{".xml", "text/xml"},
				{".xpi", "application/x-xpinstall"},
				{".zip", "application/zip"},
				{".map", "application/json"}
			};
		}

		public void Dispose()
		{
			lock (this)
			{
				Stop();
				listener = null;
			}
		}

		/// <summary>
		/// Start listening for http requests
		/// </summary>
		public void Start()
		{
			lock (this)
			{
				if (listener.IsListening) throw new Exception("Already started. Must call stop first.");
				listener.Start();
				listener.BeginGetContext(RequestCallback, null);
			}
		}

		/// <summary>
		/// Stop listening for http requests
		/// </summary>
		public void Stop()
		{
			if (listener != null) listener.Stop();
		}

		private void RequestCallback(IAsyncResult ar)
		{
			HttpListenerContext context;
			lock (this)
			{
				if (listener == null || !listener.IsListening) return;
			
				context = listener.EndGetContext(ar);
				listener.BeginGetContext(RequestCallback, null);
			}

			// handle web socket connection requests
			if (context.Request.IsWebSocketRequest)
			{
				string endpoint = context.Request.QueryString["endpoint"];
				if (webSocketEndpoints.ContainsKey(endpoint)) HandleWebSocketRequestAsync(context, endpoint);
				else Finish(context.Response, HttpStatusCode.Forbidden);
				return;
			}

			// get resource path
			string resourcePath = context.Request.Url.AbsolutePath.TrimStart('/');

			// test if request is handled by a route
			if (routes.ContainsKey(resourcePath))
			{
				HttpStatusCode statusCode;
				try
				{
					routes[resourcePath](this, context, resourcePath, out statusCode);
				}
				catch (Exception e)
				{
					Finish(context.Response, HttpStatusCode.InternalServerError);
					InternalErrorCallback?.Invoke(e);
					return;
				}

				Finish(context.Response, statusCode);
				return;
			}

			// check for index file
			if (string.IsNullOrEmpty(resourcePath))
			{
				foreach (string indexFile in indexFiles)
				{
					if (File.Exists(Path.Combine(rootResourcePath, indexFile)))
					{
						resourcePath = indexFile;
						break;
					}
				}
			}

			// send file
			string filename = Path.Combine(rootResourcePath, resourcePath.TrimStart('/').Replace('/', '\\'));
			if (File.Exists(filename))
			{
				try
				{
					using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						if (!context.Request.IsGZipSupported())
						{
							// get mime type
							context.Response.ContentType = mimeTypes[Path.GetExtension(filename)];
							context.Response.ContentLength64 = stream.Length;

							// copy file stream to response
							stream.CopyTo(context.Response.OutputStream, 81920);
							stream.Flush();
						}
						else
						{
							using (var memoryStream = new MemoryStream())
							using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
							{
								// get mime type
								context.Response.AddHeader("Content-Encoding", "gzip");
								context.Response.ContentType = mimeTypes[Path.GetExtension(filename)];

								// copy file stream compressed stream
								stream.CopyTo(gZipStream, 81920);
								stream.Flush();
								gZipStream.Flush();
								memoryStream.Flush();
								gZipStream.Close();

								// copy compressed stream to repsonse
								context.Response.ContentLength64 = memoryStream.Length;
								memoryStream.Position = 0;
								memoryStream.CopyTo(context.Response.OutputStream);
							}
						}
					}
				}
				catch (Exception e)
				{
					Finish(context.Response, HttpStatusCode.InternalServerError);
					InternalErrorCallback?.Invoke(e);
					return;
				}
			}
			else
			{
				Finish(context.Response, HttpStatusCode.NotFound);
				InternalErrorCallback?.Invoke(new Exception("File not found: " + filename));
				return;
			}

			// finish
			Finish(context.Response, HttpStatusCode.OK, filename);
		}

		private async void HandleWebSocketRequestAsync(HttpListenerContext context, string endpoint)
		{
			void DoneCallback(HttpStatusCode statusCode)
			{
				Finish(context.Response, statusCode);
			}

			try
			{
				var socket = await context.AcceptWebSocketAsync(null);
				while (socket.WebSocket.State == WebSocketState.Connecting) await Task.Delay(500);

				if (socket.WebSocket.State == WebSocketState.Open)
				{
					var webSocketEndpointCallback = webSocketEndpoints[endpoint];
					webSocketEndpointCallback(this, socket, endpoint, DoneCallback);
				}
				else
				{
					Finish(context.Response, HttpStatusCode.Gone);
				}
			}
			catch (WebSocketException e)
			{
				Finish(context.Response, HttpStatusCode.GatewayTimeout);
				InternalErrorCallback?.Invoke(e);
				return;
			}
			catch (Exception e)
			{
				Finish(context.Response, HttpStatusCode.InternalServerError);
				InternalErrorCallback?.Invoke(e);
				return;
			}
		}

		private void Finish(HttpListenerResponse response, HttpStatusCode statusCode, string filename = null)
		{
			try
			{
				response.StatusCode = (int)statusCode;
				if (statusCode == HttpStatusCode.OK)
				{
					response.AddHeader("Date", DateTime.Now.ToString("r"));
					if (filename != null && File.Exists(filename)) response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToString("r"));
				}

				response.OutputStream.Flush();
				response.Close();
			}
			catch { }
		}

		public void AddRoute(string route, HttpRouteCallback callback)
		{
			if (routes.ContainsKey(route)) throw new Exception("Route already added.");
			if (string.IsNullOrEmpty(route)) throw new Exception("Route cannot be emtpy");
			if (callback == null) throw new Exception("Route callback cannot be emtpy");
			routes.Add(route, callback);
		}

		public void RemoveRoute(string route)
		{
			if (!routes.ContainsKey(route)) throw new Exception("Route doesn't exist");
			routes.Remove(route);
		}

		public void AddWebSocketEndpoint(string endpoint, HttpWebSocketEndpointCallback callback)
		{
			if (webSocketEndpoints.ContainsKey(endpoint)) throw new Exception("WebSocket Endpoint already added.");
			if (string.IsNullOrEmpty(endpoint)) throw new Exception("WebSocket Endpoint cannot be emtpy");
			if (callback == null) throw new Exception("WebSocket Endpoint callback cannot be null");
			webSocketEndpoints.Add(endpoint, callback);
		}

		public void RemoveWebSocketEndpoint(string endpoint)
		{
			if (!webSocketEndpoints.ContainsKey(endpoint)) throw new Exception("WebSocket Endpoint doesn't exist");
			webSocketEndpoints.Remove(endpoint);
		}

		/// <summary>
		/// Invokes "netsh http add urlacl url=URL user=DOMAIN listen=yes"
		/// </summary>
		/// <param name="url">Url to allow through the firewall</param>
		/// <param name="domain">Domain/User to allow through the firewall</param>
		public static void AllowListenerThroughFirewall(string url, string domain)
		{
			using (var process = Process.Start("netsh", $"http add urlacl url={url} user={domain} listen=yes"))
			{
				process.WaitForExit();
			}
		}

		/// <summary>
		/// Invokes "netsh http add urlacl url=http(s)://host:port/ user=DOMAIN listen=yes"
		/// </summary>
		/// <param name="protocal">Http protocal</param>
		/// <param name="host">Url host name</param>
		/// <param name="port">Url port</param>
		/// <param name="domain">Computer domain/name</param>
		public static void AllowListenerThroughFirewall(HttpProtocals protocal, string host, int port, string domain = "Everyone")
		{
			AllowListenerThroughFirewall(HttpUtils.FormatUrl(protocal, host, port), domain);
		}

		/// <summary>
		/// Invokes "netsh http delete urlacl url=URL"
		/// </summary>
		/// <param name="url">Url to remove from the firewall</param>
		public static void RemoveListenerThroughFirewall(string url)
		{
			using (var process = Process.Start("netsh", $"http delete urlacl url={url}"))
			{
				process.WaitForExit();
			}
		}

		/// <summary>
		/// Invokes "netsh http delete urlacl url=http(s)://host:port/"
		/// </summary>
		/// <param name="protocal">Http protocal</param>
		/// <param name="host">Url host name</param>
		/// <param name="port">Url port</param>
		public static void RemoveListenerThroughFirewall(HttpProtocals protocal, string host, int port)
		{
			RemoveListenerThroughFirewall(HttpUtils.FormatUrl(protocal, host, port));
		}

		/// <summary>
		/// Invokes "netsh http show urlacl" and parses result
		/// </summary>
		/// <returns>List of listeners allowed through firewall</returns>
		public static List<HttpActiveListener> GetHttpFirewallListeners()
		{
			var results = new List<HttpActiveListener>();
			HttpActiveListener activeResult = null;
			void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
			{
				if (e.Data == null) return;
				string line = e.Data.TrimStart();

				// create new result
				if (line.StartsWith("Reserved URL"))
				{
					activeResult = new HttpActiveListener();

					// parse url
					var match = Regex.Match(line, @"Reserved URL\s*:\s(.*)");
					if (match.Success) activeResult.url = match.Groups[1].Value.Trim();
				}
				else if (activeResult != null)
				{
					// check for user arg
					if (line.StartsWith("User:"))
					{
						var match = Regex.Match(line, @"User: (.*)");
						if (match.Success) activeResult.user = match.Groups[1].Value.Trim();
					}

					// check for listen arg
					if (line.StartsWith("Listen:"))
					{
						var match = Regex.Match(line, @"Listen: (.*)");
						if (match.Success && match.Groups[1].Value == "Yes") results.Add(activeResult);
					}
				}
			}
			
			using (var process = new Process())
			{
				process.StartInfo.FileName = "netsh";
				process.StartInfo.Arguments = "http show urlacl";
				process.StartInfo.RedirectStandardOutput = true;
				process.OutputDataReceived += Process_OutputDataReceived;
				process.Start();
				process.BeginOutputReadLine();
				process.WaitForExit();
			}

			return results;
		}
	}
}
