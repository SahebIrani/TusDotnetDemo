using Microsoft.AspNetCore.Builder;

using Simple.Middleware;

namespace Simple.Extensions.vshistory.ExceptionHandlerMiddlewareExtension.cs
{
	public static class SimpleExceptionHandlerMiddlewareExtension
	{
		public static IApplicationBuilder UseSimpleExceptionHandler(this IApplicationBuilder builder) =>
			builder.UseMiddleware<SimpleExceptionHandlerMiddleware>();
	}
}
