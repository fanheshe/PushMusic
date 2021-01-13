using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
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

        public static async Task Main(string[] args)
        {
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

                    await myService.Execute();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occurred.");
                }
            }

            System.Console.ReadLine();
        }
    }

    public interface IMyService
    {
        Task Execute();
    }

    public class MyService : IMyService
    {
        private readonly IHttpClientFactory _clientFactory;

        public MyService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task Execute()
        {
            await PushMsg("提示", "监听开始");

            // push server酱 或 公众号

            // 给订阅的

            Task.Run(async () =>
            {
                do
                {
                    Thread.Sleep(10000);

                    string music_api = "http://daojia.jd.com/client?channel=wx_xcx&platform=5.0.0&platCode=mini&mpChannel=wx_xcx&appVersion=8.4.0&xcxVersion=8.3.7&appName=paidaojia&functionId=zone%2FgetNewChannelDetail&isForbiddenDialog=false&isNeedDealError=false&isNeedDealLogin=false&body=%7B%22city%22%3A%22%E7%9F%B3%E5%AE%B6%E5%BA%84%E5%B8%82%22%2C%22areaCode%22%3A142%2C%22longitude%22%3A114.55373%2C%22latitude%22%3A38.08728%2C%22coordType%22%3A2%2C%22channelId%22%3A%224053%22%2C%22refPageSource%22%3A%22%22%2C%22pageSource%22%3A%22newChannelPage%22%2C%22ctp%22%3A%22channel%22%2C%22ref%22%3A%22home%22%7D&afsImg=&lat_pos=38.08728&lng_pos=114.55373&lat=38.08728&lng=114.55373&city_id=142&deviceToken=0b81b867-2c37-47df-8c04-f59a6f60c426&deviceId=0b81b867-2c37-47df-8c04-f59a6f60c426&deviceModel=appmodel&business=undefined&traceId=0b81b867-2c37-47df-8c04-f59a6f60c4261610178452846";


                    var request = new HttpRequestMessage(HttpMethod.Get,
                        music_api);
                    request.Headers.Add("Accept", "*/*");
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 14_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko)");

                    var client = _clientFactory.CreateClient();
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultStr = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<JObject>(resultStr);

                        if (data["code"].ToString() == "0")
                        {
                            //await PushMsg("提示", "服务器好啦！");

                            // != "0"
                            // Console.WriteLine("监听中...");
                            // continue;

                            try
                            {
                                var results = data["result"]["data"][1]["data"];
                                foreach (var item in results)
                                {
                                    Console.WriteLine(item["storeName"] + " - " + (item["closeStatus"].ToString() == "0" ? "营业中" : "休息中"));
                                    if (item["storeName"].ToString().IndexOf("超市") != -1)
                                    {
                                        if (item["closeStatus"].ToString() == "0")
                                        {
                                            await PushMsg("粗来啦", "有货了有货了");
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                //await PushMsg("错误", "解析出错啦");
                            }
                        }


                        //System.Console.ReadLine(); 定时任务不能return ?
                    }
                } while (true);
            });
        }




        private async Task PushMsg(string text, string desp)
        {
            var pushMsg = $@"https://sc.ftqq.com/SCU136507T2147a536468e17f6df88888c92fdf9975fdb2b2448d00.send?text={text + new Random().Next()}&desp={desp + new Random().Next()}";

            var request = new HttpRequestMessage(HttpMethod.Get, pushMsg);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                //return $"StatusCode: {response.StatusCode}";
            }
        }

    }
}
