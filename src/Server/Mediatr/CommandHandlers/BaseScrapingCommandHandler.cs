using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Vetrina.Server.Mediatr.CommandHandlers
{
    public class BaseScrapingCommandHandler<T>
    {
        private readonly ILogger<T> logger;

        public BaseScrapingCommandHandler(ILogger<T> logger)
        {
            this.logger = logger;
        }

        protected async Task ScrollToBottomOfPageAndSlowlyBackUp(IWebDriver webDriver)
        {
            var javascriptExecutor = (IJavaScriptExecutor) webDriver;

            long documentHeight = 0;

            while (true)
            {
                logger.LogInformation("Scrolling to bottom of page...");

                const string scriptScrollDown = @"
                        window.scrollTo(0, document.body.scrollHeight - 150); 
                        return document.body.scrollHeight;";

                var documentHeightAfterScroll = (long) javascriptExecutor.ExecuteScript(scriptScrollDown);

                logger.LogInformation(
                    $"Document height after scroll {documentHeightAfterScroll}. {Environment.NewLine}" +
                    $"Document height before scroll: {documentHeight}.");

                if (documentHeightAfterScroll == documentHeight)
                {
                    // Slowly scroll back up to allow all lazy loaded elements to get rendered.
                    const string scriptScrollUp = @"window.scrollBy(0, -150); return document.body.scrollHeight;";

                    for (var i = 0; i < documentHeight / 150; i++)
                    {
                        javascriptExecutor.ExecuteScript(scriptScrollUp);
                        await Task.Delay(TimeSpan.FromMilliseconds(100));
                    }

                    break;
                }

                documentHeight = documentHeightAfterScroll;

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        protected async Task ScrollToBottomOfPage(IWebDriver webDriver)
        {
            var javascriptExecutor = (IJavaScriptExecutor)webDriver;

            var documentHeight = (long)javascriptExecutor.ExecuteScript("return document.body.scrollHeight;");
            var currentBufferToScroll = documentHeight;
            const int scrollStepInPixels = 150;

            var currentScrollHeight = 0;

            while (true)
            {
                logger.LogInformation("Scrolling to bottom of page...");

                for (var i = 0; i <= currentBufferToScroll / scrollStepInPixels; i++)
                {
                    javascriptExecutor.ExecuteScript($"window.scrollBy(0, {scrollStepInPixels});");
                    currentScrollHeight += scrollStepInPixels;

                    logger.LogInformation($"Scrolled to {currentScrollHeight}.");

                    await Task.Delay(TimeSpan.FromMilliseconds(150));
                }

                await Task.Delay(TimeSpan.FromSeconds(2));

                var newDocumentHeight = (long)javascriptExecutor.ExecuteScript("return document.body.scrollHeight;");

                logger.LogInformation(
                    $"Document height before scroll {documentHeight}. {Environment.NewLine}" +
                    $"Document height after scroll: {newDocumentHeight}.");

                if (newDocumentHeight == documentHeight)
                {
                    break;
                }

                currentBufferToScroll = newDocumentHeight - documentHeight;
                documentHeight = newDocumentHeight;
            }
        }
    }
}