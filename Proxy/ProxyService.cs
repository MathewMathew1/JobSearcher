using System.Collections.Generic;
using Microsoft.Playwright;

namespace JobSearch.ProxyService
{
    public class BrightDataProxyService : IProxyService
    {
        private readonly string _proxyServer;
        private readonly string _username;
        private readonly string _password;
        private static readonly string[] _userAgents = new[]
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.5993.90 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_4) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.4 Safari/605.1.15",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:116.0) Gecko/20100101 Firefox/116.0",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.5845.187 Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1"
        };

        private static readonly (int w, int h)[] _viewports = new[]
        {
            (1920, 1080),
            (1600, 900),
            (1366, 768),
            (1440, 900),
            (1280, 800),
            (390, 844) // mobile
        };

        public BrightDataProxyService(IConfiguration config)
        {
            _proxyServer = config["Bright:Proxy:Server"];
            _username = config["Bright:Proxy:Username"];
            _password = config["Bright:Proxy:Password"];
        }

        public BrowserNewContextOptions GetContextOptions()
        {
            // choose user agent and viewport randomly
            var rng = Random.Shared;
            var ua = _userAgents[rng.Next(_userAgents.Length)];

            // optionally prefer mobile by selecting mobile UA + small viewport
            (int w, int h) viewport;
 
            viewport = _viewports[rng.Next(_viewports.Length)];
            

            // proxy object
            var proxy = new Proxy
            {
                Server = _proxyServer ?? "http://brd.superproxy.io:33335",
                Username = _username,
                Password = _password
            };

            // realistic Accept-Language header and other headers
            var headers = new Dictionary<string, string>
            {
                { "Accept-Language", "en-US,en;q=0.9" },
                // keep referer blank for first navigation or set to search page if you have it
            };

            return new BrowserNewContextOptions
            {
                UserAgent = ua,
               // ViewportSize = new ViewportSize { Width = viewport.w, Height = viewport.h },
                Locale = "en-US",
                IgnoreHTTPSErrors = true,
                //Proxy = proxy,
                ExtraHTTPHeaders = headers,
                // optional: set reduced timeout to surface blocks faster
                // DefaultNavigationTimeout = 30000
            };
        }
    }
}
