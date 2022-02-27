using System;
using System.IO;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Vetrina.Server.Abstractions;

namespace Vetrina.Server.Services
{
    public class ChromeWebDriverFactory : IWebDriverFactory
    {
        public IWebDriver CreateWebDriver()
        {
            // TODO: Think about pooling the web driver instances for better performance (only if needed).
            var chromeDriverLocation = GetChromeDriverLocation();
            var chromeDriverOptions = GetChromeDriverOptions();
            var chromeDriverService = ChromeDriverService.CreateDefaultService(chromeDriverLocation);
            var chromeDriver = new ChromeDriver(chromeDriverService, chromeDriverOptions);

            chromeDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

            return chromeDriver;
        }

        private static string GetChromeDriverLocation()
        {
            return Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
        }

        private static ChromeOptions GetChromeDriverOptions()
        {
            var options = new ChromeOptions();

            //options.AddArgument("--headless");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("start-maximized");
            options.AddArgument("disable-infobars");
            options.AddArgument("--whitelisted-ips");

            options.PageLoadStrategy = PageLoadStrategy.Eager;

            return options;
        }
    }
}
