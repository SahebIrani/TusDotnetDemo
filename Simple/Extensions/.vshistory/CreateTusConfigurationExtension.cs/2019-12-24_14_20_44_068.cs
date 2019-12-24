using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

namespace Simple.Extensions
{
	public static class CreateTusConfigurationExtension
	{
		public static void AddCreateTusConfiguration(this IServiceCollection services)
			=> services.AddSingleton(CreateTusConfiguration);

		private static DefaultTusConfiguration CreateTusConfiguration(IServiceProvider serviceProvider)
		{
			ILogger<Startup> logger =
				serviceProvider.GetService<ILoggerFactory>().CreateLogger<Startup>();

			IConfiguration configuration = serviceProvider.GetService<IConfiguration>();

			bool enableAuthorize = configuration.GetValue<bool>("EnableOnAuthorize");

			return new DefaultTusConfiguration
			{
				UrlPath = "/files",
				Store = new TusDiskStore(@"C:\tusfiles\"),
				Events = new Events
				{
					OnAuthorizeAsync = ctx =>
					{
						if (!enableAuthorize) return Task.CompletedTask;

						if (!ctx.HttpContext.User.Identity.IsAuthenticated)
						{
							ctx.HttpContext.Response.Headers.Add("WWW-Authenticate",
								new StringValues("Basic realm=TusDotnetDemo in ASP.NET Core 3.1"));

							ctx.FailRequest(HttpStatusCode.Unauthorized);

							return Task.CompletedTask;
						}

						if (ctx.HttpContext.User.Identity.Name != "test" /*configuration.GetValue<string>("SinjulMSBH")*/)
						{
							ctx.FailRequest(HttpStatusCode.Forbidden, "'test' is the only allowed user");
							return Task.CompletedTask;
						}

						switch (ctx.Intent)
						{
							case IntentType.CreateFile:
								break;
							case IntentType.ConcatenateFiles:
								break;
							case IntentType.WriteFile:
								break;
							case IntentType.DeleteFile:
								break;
							case IntentType.GetFileInfo:
								break;
							case IntentType.GetOptions:
								break;
							default:
								break;
						}

						return Task.CompletedTask;
					},

					OnBeforeCreateAsync = ctx =>
					{
						if (ctx.FileConcatenation is FileConcatPartial)
							return Task.CompletedTask;

						if (!ctx.Metadata.ContainsKey("name"))
							ctx.FailRequest("name metadata must be specified. ");

						if (!ctx.Metadata.ContainsKey("contentType"))
							ctx.FailRequest("contentType metadata must be specified. ");

						return Task.CompletedTask;
					},
					OnCreateCompleteAsync = ctx =>
					{
						logger.LogInformation($"Created file {ctx.FileId} using {ctx.Store.GetType().FullName}");

						return Task.CompletedTask;
					},
					OnBeforeDeleteAsync = ctx =>
					{
						return Task.CompletedTask;
					},
					OnDeleteCompleteAsync = ctx =>
					{
						logger.LogInformation($"Deleted file {ctx.FileId} using {ctx.Store.GetType().FullName}");
						return Task.CompletedTask;
					},
					OnFileCompleteAsync = ctx =>
					{
						logger.LogInformation($"Upload of {ctx.FileId} completed using {ctx.Store.GetType().FullName}");
						//var file = await ctx.GetFileAsync();
						return Task.CompletedTask;
					}
				},

				Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(5))
			};
		}
	}
}
