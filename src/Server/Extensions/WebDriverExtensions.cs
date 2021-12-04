using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Vetrina.Server.Extensions
{
    public static class WebDriverExtensions
    {
        public static IEnumerable<IWebElement> FindElementsToBePresent(
            this IWebDriver webDriver, 
            By selector, 
            TimeSpan? maxWaitTime = null)
        {
            try
            {
                return new WebDriverWait(webDriver, maxWaitTime ?? TimeSpan.FromSeconds(10))
                    .Until(ExpectedConditions
                        .PresenceOfAllElementsLocatedBy(selector));
            }
            catch (Exception)
            {
                return new List<IWebElement>();
            }
        }

        public static IEnumerable<IWebElement> FindElementsToBeVisible(
            this IWebDriver webDriver, 
            By selector, 
            TimeSpan? maxWaitTime = null)
        {
            try
            {
                return new WebDriverWait(webDriver, maxWaitTime ?? TimeSpan.FromSeconds(10))
                    .Until(ExpectedConditions
                        .VisibilityOfAllElementsLocatedBy(selector));
            }
            catch (Exception)
            {
                return new List<IWebElement>();
            }
        }

        public static IWebElement FindElementExists(
            this IWebDriver webDriver,
            By selector,
            TimeSpan? maxWaitTime = null)
        {
            try
            {
                return new WebDriverWait(webDriver, maxWaitTime ?? TimeSpan.FromSeconds(10))
                    .Until(ExpectedConditions
                        .ElementExists(selector));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IWebElement FindElementExists(
            this IWebElement webElement,
            By selector)
        {
            try
            {
                return webElement.FindElement(selector);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IWebElement FindElementToBeClickable(
            this IWebDriver webDriver,
            By selector,
            TimeSpan? maxWaitTime = null)
        {
            try
            {
                return new WebDriverWait(webDriver, maxWaitTime ?? TimeSpan.FromSeconds(10))
                    .Until(ExpectedConditions
                        .ElementToBeClickable(selector));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IWebElement FindElementToBeVisible(
            this IWebDriver webDriver,
            By selector,
            TimeSpan? maxWaitTime = null)
        {
            try
            {
                return new WebDriverWait(webDriver, maxWaitTime ?? TimeSpan.FromSeconds(10))
                    .Until(ExpectedConditions
                        .ElementIsVisible(selector));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}