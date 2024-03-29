using System;
using System.Net.Http.Headers;
using System.Reflection;
using Ganss.Xss;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OBS.WebSockets.Core;
using Serilog;
using TwitchBot.Service.Extensions;
using TwitchBot.Service.Features.Caching;
using TwitchBot.Service.Features.HealthChecks;
using TwitchBot.Service.Features.MediatR;
using TwitchBot.Service.Features.WLED;
using TwitchBot.Service.Hubs;
using TwitchBot.Service.Models;
using TwitchBot.Service.Services;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Interfaces;

namespace TwitchBot.Service
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
            services.AddSerilog(Configuration);
            services.AddCors(); // Make sure you call this previous to AddMvc
            services.AddAutoMapper(typeof(Startup));
            
            services.AddOptions();

            services.AddTransient(c =>
            {
                var s = new HtmlSanitizer();
                var allowedTags = new[]
                {
                    "a", "abbr", "acronym", "address", "area", "article", "aside", "b", "bdi", "big", "blockquote",
                    "br", "button", "caption", "center", "cite", "code", "col", "colgroup", "data", "datalist", "dd",
                    "del", "details", "dfn", "dir", "div", "dl", "dt", "em", "fieldset", "figcaption", "figure", "font",
                    "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "header", "hr", "i", "img", "input", "ins",
                    "kbd", "keygen", "label", "legend", "li", "main", "map", "mark", "menu", "menuitem", "meter", "nav",
                    "ol", "optgroup", "option", "output", "p", "pre", "progress", "q", "rp", "rt", "ruby", "s", "samp",
                    "section", "select", "small", "span", "strike", "strong", "sub", "summary", "sup", "table", "tbody",
                    "td", "textarea", "tfoot", "th", "thead", "time", "tr", "tt", "u", "ul", "var", "wbr"
                };
                s.AllowedTags.Clear();
                foreach (var allowedTag in allowedTags) s.AllowedTags.Add(allowedTag);
                var allowedAttributes = new[]
                {
                    "abbr", "accept", "accept-charset", "accesskey", "action", "align", "alt", "autocomplete",
                    "autosave", "axis", "bgcolor", "border", "cellpadding", "cellspacing", "challenge", "char",
                    "charoff", "charset", "checked", "cite", "clear", "color", "cols", "colspan", "compact",
                    "contenteditable", "coords", "datetime", "dir", "disabled", "draggable", "dropzone", "enctype",
                    "for", "frame", "headers", "height", "high", "href", "hreflang", "hspace", "ismap", "keytype",
                    "label", "lang", "list", "longdesc", "low", "max", "maxlength", "media", "method", "min",
                    "multiple", "name", "nohref", "noshade", "novalidate", "nowrap", "open", "optimum", "pattern",
                    "placeholder", "prompt", "pubdate", "radiogroup", "readonly", "rel", "required", "rev", "reversed",
                    "rows", "rowspan", "rules", "scope", "selected", "shape", "size", "span", "spellcheck", "src",
                    "start", "step", "style", "summary", "tabindex", "target", "title", "type", "usemap", "valign",
                    "value", "vspace", "width", "wrap"
                };
                s.AllowedAttributes.Clear();
                foreach (var allowedAttribute in allowedAttributes) s.AllowedAttributes.Add(allowedAttribute);
                return s;
            });
            services.AddTransient<Services.TwitchBot>();
            services.AddSingleton<TwitchClientServices>();
            services.AddTransient<TwitchPubSubService>();
            services.Configure<TwitchConfig>(Configuration.GetSection("Twitch"));
            services.Configure<OBSConfig>(Configuration.GetSection("OBS"));
            services.Configure<WLEDConfig>(Configuration.GetSection("WLED"));
            services.AddHttpClient<WLEDService>("DadJokeService", c =>
            {
                c.BaseAddress = new Uri(Configuration["DadJokes:BaseUrl"]);
            });
            services.AddHttpClient<WLEDService>("WLEDService", c =>
            {
                c.BaseAddress = new Uri(Configuration["WLED:BaseUrl"]);
            });
            services.AddHttpClient("TwitchClientServices", (s,c) =>
            {
                var config = s.GetService<IOptions<TwitchConfig>>()!.Value;
                c.BaseAddress = new Uri("https://api.twitch.tv");
                c.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {config.Auth.AccessToken}");
            });
            services.AddTransient<WLEDService>();
            services.AddTransient<TwitchMemoryCache>();
            services.AddTransient<ICacheClient>(cs => new CacheClient(Configuration.GetConnectionString("redis"), true));
            services.AddTransient(c =>
            {
                var obs = new OBSWebsocket { WSTimeout = TimeSpan.FromMinutes(10) };
                return obs;
            });
            services.AddTransient<OBSServices>();
            services.AddTransient<DadJokeService>();

            services.AddTransient(c =>
            {
                var config = c.GetService<IOptions<TwitchConfig>>()!.Value;
                IApiSettings apiSettings = new ApiSettings
                {
                    ClientId = config.Auth.ClientId,
                    Secret = config.Auth.ClientSecret
                };

                var api = new TwitchAPI(settings: apiSettings);
                api.Settings.AccessToken = config.Auth.AccessToken;

                return api;
            });

            services.AddTransient<INotifierMediatorService, NotifierMediatorService>();
            services.AddMediatR(c => {
                c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });
            // services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddStackExchangeRedisCache(options =>
            {
                // options.InstanceName = "TwitchBotService";
                var connectionString = Configuration.GetConnectionString("redis");
                options.Configuration = connectionString;
            });
            services.AddTransient<ICacheService, DistributedCacheService>();
            services.AddRazorPages();
            services.AddSignalR()
                .AddNewtonsoftJsonProtocol()
                .AddMessagePackProtocol();

            services.AddHealthChecks()
                .AddCheck<OBSWebSocketsHealthCheck>("obs_websockets_check")
                .AddCheck<TwitchClientHealthCheck>("twitch_client_check")
                .AddCheck<TwitchPubSubHealthCheck>("twitch_pubsub_check");

            var servicesProvider = services.BuildServiceProvider(validateScopes: true);
            using var scope = servicesProvider.CreateScope();
            _ = scope.ServiceProvider.GetRequiredService<TwitchClientServices>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseTwitchBot();

            app.UseCors(
                options => options
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(origin => true)
                    .AllowCredentials()
            );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<TwitchHub>("/twitchhub"); 
                // endpoints.MapHealthChecks("/health");
            });

            app.AddHealthMetrics();
        }
    }
}
