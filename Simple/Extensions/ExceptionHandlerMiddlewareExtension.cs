using Microsoft.AspNetCore.Builder;

using Simple.Middleware;

namespace Simple.Extensions
{
	public static class ExceptionHandlerMiddlewareExtension
	{
		public static IApplicationBuilder UseSimpleExceptionHandler(this IApplicationBuilder builder) =>
			builder.UseMiddleware<SimpleExceptionHandlerMiddleware>();
	}
}
