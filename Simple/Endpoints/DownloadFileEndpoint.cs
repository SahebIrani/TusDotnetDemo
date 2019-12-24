using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace Simple.Endpoints
{
	public static class DownloadFileEndpoint
	{
		public static async Task HandleRoute(HttpContext context)
		{
			DefaultTusConfiguration config = context.RequestServices.GetRequiredService<DefaultTusConfiguration>();

			if (!(config.Store is ITusReadableStore store)) return;

			string fileId = (string)context.Request.RouteValues["fileId"];
			ITusFile file = await store.GetFileAsync(fileId, context.RequestAborted);

			if (file == null)
			{
				context.Response.StatusCode = 404;
				await context.Response.WriteAsync($"File with id {fileId} was not found.", context.RequestAborted);
				return;
			}

			System.IO.Stream fileStream = await file.GetContentAsync(context.RequestAborted);
			Dictionary<string, Metadata> metadata = await file.GetMetadataAsync(context.RequestAborted);

			context.Response.ContentType = GetContentTypeOrDefault(metadata);
			context.Response.ContentLength = fileStream.Length;

			if (metadata.TryGetValue("name", out var nameMeta))
				context.Response.Headers.Add("Content-Disposition",
					new[] { $"attachment; filename=\"{nameMeta.GetString(Encoding.UTF8)}\"" });

			using (fileStream)
				await fileStream.CopyToAsync(context.Response.Body, 81920, context.RequestAborted);
		}

		private static string GetContentTypeOrDefault(Dictionary<string, Metadata> metadata)
		{
			if (metadata.TryGetValue("contentType", out var contentType))
				return contentType.GetString(Encoding.UTF8);

			return "application/octet-stream";
		}
	}
}
