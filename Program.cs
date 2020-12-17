using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PushMusic
{
    class Program
    {

        static async Task<int> Main(string[] args)
        {
            string music_api = "https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?g_tk=5381&uin=0&format=json&inCharset=utf-8&outCharset=utf-8&notice=0&platform=h5&needNewCode=1&tpl=3&page=detail&type=top&topid=36&_=1520777874472";

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddTransient<IMyService, MyService>();
                }).UseConsoleLifetime();

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var myService = services.GetRequiredService<IMyService>();

                    await myService.Execute(music_api);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occurred.");
                }
            }

            return 0;
        }
    }

    public interface IMyService
    {
        Task Execute(string music_api);
    }

    public class MyService : IMyService
    {
        private readonly IHttpClientFactory _clientFactory;

        public MyService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task Execute(string music_api)
        {
            // push server酱 或 公众号

            // 给订阅的


            var request = new HttpRequestMessage(HttpMethod.Get,
                music_api);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var resultStr = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<JObject>(resultStr);
                var top4 = data["songlist"].Take(4);
                var names = top4.Select(s=>s["data"]["albumname"].ToString()).ToArray();
                foreach(var item in names){
                    Console.WriteLine(item);
                }
            }
            else
            {
                //return $"StatusCode: {response.StatusCode}";
            }
        }
    }
}
