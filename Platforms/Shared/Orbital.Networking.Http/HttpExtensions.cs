using System;
using System.IO;
using System.Net;

namespace Orbital.Networking.Http
{
	public static class HttpExtensions
	{
		public static string GetResponseAsText(this HttpWebResponse _this)
		{
			using (var stream = _this.GetResponseStream())
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		public static bool IsGZipSupported(this HttpListenerRequest source)
		{
			string AcceptEncoding = source.Headers["Accept-Encoding"];
			return !string.IsNullOrEmpty(AcceptEncoding) && (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate"));
		}
	}
}
