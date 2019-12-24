using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Simple.Middleware
{
	public class SimpleExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<SimpleExceptionHandlerMiddleware> _logger;

		public SimpleExceptionHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
		{
			_next = next;
			_logger = loggerFactory.CreateLogger<SimpleExceptionHandlerMiddleware>();
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next.Invoke(context);
			}
			catch (Exception exc)
			{
				_logger.LogError(exc, exc.Message);
				context.Response.StatusCode = 500;
				await context.Response.WriteAsync("An internal server error has occurred .. !!!!", context.RequestAborted);
			}
		}
	}
}
