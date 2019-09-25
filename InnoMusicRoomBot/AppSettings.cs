using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace InnoMusicRoomBot
{
    public class AppSettings
    {
        private readonly RequestDelegate _next;

        public AppSettings(IHostingEnvironment env, RequestDelegate next, IConfiguration config)
        {
            Console.WriteLine("AppSettings AppSettings configure");
            _next = next;
            AppConfiguration = config;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("usersettings.json");
            // создаем конфигурацию
            AppConfiguration = builder.Build();
            isDevEnvironment = env.IsDevelopment();

            //Костыль, здесь этого быть не должно, но боты должны запускаться после настройки конфигурации. Хотя, это в каком то смысле тоже часть конфигурации
            Console.WriteLine("pre bot run");
            new MainBot();
            new AdminBot();
            Console.WriteLine("post bot run");
        }
        public static IConfiguration AppConfiguration { get; set; }
        public static bool isDevEnvironment { get; set; }

        public static string Name { get; set; } = "RunGiantBoomTestBot";
        public static string proxyLogin { get { return AppConfiguration["proxyLogin"]; } }
        public static string proxyPassword { get { return AppConfiguration["proxyPassword"]; } }
        public static string proxyAddress { get { return AppConfiguration["proxyAddress"]; } }
        public static string mainKey { get { return AppConfiguration["mainKey"]; } }
        public static string adminKey { get { return AppConfiguration["adminKey"]; } }
        public static string adminId { get { return AppConfiguration["adminId"]; } }
        public async Task Invoke(HttpContext context)
        {
            Console.WriteLine("ASDASDASD");
            await context.Response.WriteAsync($"<p>main page</p>");
        }
    }
}
