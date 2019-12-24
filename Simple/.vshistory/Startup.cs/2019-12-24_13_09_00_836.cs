using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Simple.Authentication;
using Simple.Endpoints;
using Simple.Middleware;
using Simple.Services;

using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

namespace Simple
{
	public class Startup
	{
		public Startup(IConfiguration configuration) => Configuration = configuration;

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(CreateTusConfiguration);

			services.AddHostedService<ExpiredFilesCleanupService>();

			services.AddAuthentication("BasicAuthentication")
					.AddScheme<AuthenticationSchemeOptions,
					BasicAuthenticationHandler>("BasicAuthentication", null)
			;
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.Use((context, next) =>
			{
				context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = null;
				return next.Invoke();
			});

			if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

			app.UseSimpleExceptionHandler();

			app.UseAuthentication();

			app.UseDefaultFiles();
			app.UseStaticFiles();

			app.UseHttpsRedirection();

			app.UseTus(httpContext => Task.FromResult(httpContext.RequestServices.GetService<DefaultTusConfiguration>()));

			app.UseRouting();

			app.UseEndpoints(endpoints => endpoints.MapGet("/files/{fileId}", DownloadFileEndpoint.HandleRoute));
		}

		private DefaultTusConfiguration CreateTusConfiguration(IServiceProvider serviceProvider)
		{
			ILogger<Startup> logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Startup>();

			bool enableAuthorize = Configuration.GetValue<bool>("EnableOnAuthorize");

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
							ctx.HttpContext.Response.Headers.Add("WWW-Authenticate", new StringValues("Basic realm=tusdotnet-test-netcoreapp2.2"));
							ctx.FailRequest(HttpStatusCode.Unauthorized);
							return Task.CompletedTask;
						}

						if (ctx.HttpContext.User.Identity.Name != "test")
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
						if (ctx.FileConcatenation is FileConcatPartial) return Task.CompletedTask;

						if (!ctx.Metadata.ContainsKey("name")) ctx.FailRequest("name metadata must be specified. ");

						if (!ctx.Metadata.ContainsKey("contentType")) ctx.FailRequest("contentType metadata must be specified. ");

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
				// Set an expiration time where incomplete files can no longer be updated.
				// This value can either be absolute or sliding.
				// Absolute expiration will be saved per file on create
				// Sliding expiration will be saved per file on create and updated on each patch/update.
				Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(5))
			};
		}
	}
}
