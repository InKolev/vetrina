using OpenQA.Selenium;

namespace Vetrina.Server.Abstractions
{
    public interface IWebDriverFactory
    {
        IWebDriver CreateWebDriver();
    }
}