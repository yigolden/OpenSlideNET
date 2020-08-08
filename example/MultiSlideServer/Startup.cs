using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using MultiSlideServer.Cache;
using OpenSlideNET;
using System;

namespace MultiSlideServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ImagesOption>(Configuration);

            services.AddSingleton<DeepZoomGeneratorCache>();
            services.AddSingleton<ImageProvider>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                if (!context.Request.Path.StartsWithSegments("/storage", out PathString remaining))
                {
                    await next();
                    return;
                }

                if (!TryParseDeepZoom(remaining, out var result))
                {
                    await next();
                    return;
                }

                if (result.format != "jpeg")
                {
                    await next();
                    return;
                }

                var provider = context.RequestServices.GetService<ImageProvider>();
                var response = context.Response;
                if (!provider.TryGetImagePath(result.name, out string path))
                {
                    response.StatusCode = 404;
                    await response.WriteAsync("FileNotFound.");
                    return;
                }

                RetainableDeepZoomGenerator dz = provider.RetainDeepZoomGenerator(result.name, path);
                try
                {
                    response.ContentType = "image/jpeg";
                    await dz.GetTileAsJpegToStreamAsync(result.level, result.col, result.row, response.Body);
                }
                finally
                {
                    dz.Release();
                }
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // OPTIMIZE: Use ReadOnlySpan in .NET Core 2.1
        // expression: /{level}/{col}_{row}.jpeg
        private static bool TryParseDeepZoom(string expression, out (string name, int level, int col, int row, string format) result)
        {
            if (expression.Length < 4 || expression[0] != '/')
            {
                result = default;
                return false;
            }

            // seg: {name}_files/{level}/{col}_{row}.jpeg
            StringSegment seg = new StringSegment(expression, 1, expression.Length - 1);
            int iPos = seg.IndexOf('/');
            if (iPos <= 0)
            {
                result = default;
                return false;
            }

            StringSegment segName = seg.Subsegment(0, iPos);
            if (segName.Length < 6 || !segName.EndsWith("_files", StringComparison.Ordinal))
            {
                result = default;
                return false;
            }
            string resultName = segName.Substring(0, segName.Length - 6);

            // seg: {level}/{col}_{row}.jpeg
            seg = seg.Subsegment(iPos + 1);
            iPos = seg.IndexOf('/');
            if (iPos <= 0)
            {
                result = default;
                return false;
            }

            if (!int.TryParse(seg.Substring(0, iPos), out var resultLevel))
            {
                result = default;
                return false;
            }

            // seg: {col}_{row}.jpeg
            seg = seg.Subsegment(iPos + 1);
            iPos = seg.IndexOf('_');
            if (seg.IndexOf('/') >= 0 || iPos <= 0)
            {
                result = default;
                return false;
            }

            if (!int.TryParse(seg.Substring(0, iPos), out var resultCol))
            {
                result = default;
                return false;
            }

            // seg: {row}.jpeg
            seg = seg.Subsegment(iPos + 1);
            iPos = seg.IndexOf('.');
            if (iPos <= 0)
            {
                result = default;
                return false;
            }

            if (!int.TryParse(seg.Substring(0, iPos), out var resultRow))
            {
                result = default;
                return false;
            }

            // seg: jpeg
            seg = seg.Subsegment(iPos + 1);

            result = (name: resultName, level: resultLevel, col: resultCol, row: resultRow, format: seg.ToString());
            return true;
        }
    }
}
