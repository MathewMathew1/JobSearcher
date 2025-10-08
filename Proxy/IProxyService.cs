
using Microsoft.Playwright;

namespace JobSearch.ProxyService
{
    public interface IProxyService
    {
        BrowserNewContextOptions GetContextOptions();
    }
}

