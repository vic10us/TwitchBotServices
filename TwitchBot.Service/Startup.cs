using System;
using AutoMapper;
using Ganss.XSS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OBS.WebSocket.Client;
using TwitchBot.Service.Extensions;
using TwitchBot.Service.Hubs;
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
            services.AddCors(); // Make sure you call this previous to AddMvc
            services.AddAutoMapper(typeof(Startup));
            
            services.AddSingleton<TwitchMemoryCache>();

            services.AddOptions();

            services.AddTransient<HtmlSanitizer>(c =>
            {
                var s = new HtmlSanitizer();
                var allowedTags = new[]
                {
                    "a", "abbr", "acronym", "address", "area", "article", "aside", "b", "bdi", "big", "blockquote", "br", "button", "caption", "center", "cite", "code", "col", "colgroup", "data", "datalist", "dd", "del", "details", "dfn", "dir", "div", "dl", "dt", "em", "fieldset", "figcaption", "figure", "font", "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "header", "hr", "i", "img", "input", "ins", "kbd", "keygen", "label", "legend", "li", "main", "map", "mark", "menu", "menuitem", "meter", "nav", "ol", "optgroup", "option", "output", "p", "pre", "progress", "q", "rp", "rt", "ruby", "s", "samp", "section", "select", "small", "span", "strike", "strong", "sub", "summary", "sup", "table", "tbody", "td", "textarea", "tfoot", "th", "thead", "time", "tr", "tt", "u", "ul", "var", "wbr"
                };
                s.AllowedTags.Clear();
                foreach (var allowedTag in allowedTags) s.AllowedTags.Add(allowedTag);
                var allowedAttributes = new[]
                {
                    "abbr", "accept", "accept-charset", "accesskey", "action", "align", "alt", "autocomplete", "autosave", "axis", "bgcolor", "border", "cellpadding", "cellspacing", "challenge", "char", "charoff", "charset", "checked", "cite", "clear", "color", "cols", "colspan", "compact", "contenteditable", "coords", "datetime", "dir", "disabled", "draggable", "dropzone", "enctype", "for", "frame", "headers", "height", "high", "href", "hreflang", "hspace", "ismap", "keytype", "label", "lang", "list", "longdesc", "low", "max", "maxlength", "media", "method", "min", "multiple", "name", "nohref", "noshade", "novalidate", "nowrap", "open", "optimum", "pattern", "placeholder", "prompt", "pubdate", "radiogroup", "readonly", "rel", "required", "rev", "reversed", "rows", "rowspan", "rules", "scope", "selected", "shape", "size", "span", "spellcheck", "src", "start", "step", "style", "summary", "tabindex", "target", "title", "type", "usemap", "valign", "value", "vspace", "width", "wrap"
                };
                s.AllowedAttributes.Clear();
                foreach (var allowedAttribute in allowedAttributes) s.AllowedAttributes.Add(allowedAttribute);
                return s;
            });
            services.AddSingleton<Services.TwitchBot>();
            services.Configure<TwitchConfig>(Configuration.GetSection("Twitch"));
            services.Configure<OBSConfig>(Configuration.GetSection("OBS"));

            services.AddSingleton(c =>
            {
                var obs = new OBSWebsocket { WSTimeout = TimeSpan.FromMinutes(10) };
                return obs;
            });

            services.AddTransient<TwitchAPI>(c =>
            {
                var config = c.GetService<IOptions<TwitchConfig>>().Value;
                IApiSettings apiSettings = new ApiSettings
                {
                    ClientId = config.Auth.ClientId,
                    Secret = config.Auth.ClientSecret
                };
                var api = new TwitchAPI(settings: apiSettings);
                // var api = new TwitchAPI();
                api.Settings.AccessToken = config.Auth.AccessToken;
                return api;
            });

            services.AddSingleton(c =>
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/TwitchHub")
                    .WithAutomaticReconnect()
                    .Build();

                return connection;
            });
            
            services.AddSerilog(Configuration);
            services.AddRazorPages();
            services.AddSignalR()
                .AddNewtonsoftJsonProtocol()
                .AddMessagePackProtocol();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
            });
        }
    }
}
