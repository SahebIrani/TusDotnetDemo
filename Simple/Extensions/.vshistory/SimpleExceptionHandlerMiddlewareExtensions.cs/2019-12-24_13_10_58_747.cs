using Microsoft.AspNetCore.Builder;

namespace Simple.Extensions.vshistory.SimpleExceptionHandlerMiddlewareExtensions.cs
{
	public static class SimpleExceptionHandlerMiddlewareExtensions
	{
		public static IApplicationBuilder UseSimpleExceptionHandler(this IApplicationBuilder builder) =>
			builder.UseMiddleware<SimpleExceptionHandlerMiddleware>();
	}
}
