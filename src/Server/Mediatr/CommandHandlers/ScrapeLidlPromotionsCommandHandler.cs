using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Extensions;
using Vetrina.Server.Mediatr.Commands;
using Vetrina.Server.Mediatr.Events;
using Vetrina.Shared;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Vetrina.Server.Mediatr.CommandHandlers
{
    public class ScrapeLidlPromotionsCommandHandler :
        BaseScrapingCommandHandler<ScrapeLidlPromotionsCommandHandler>,
        IRequestHandler<ScrapeLidlPromotionsCommand, CommandResult>
    {
        private readonly IWebDriverFactory webDriverFactory;
        private readonly IMediator mediator;
        private readonly ITransliterationService transliterationService;
        private readonly ILogger<ScrapeLidlPromotionsCommandHandler> logger;

        public ScrapeLidlPromotionsCommandHandler(
            IWebDriverFactory webDriverFactory,
            IMediator mediator,
            ITransliterationService transliterationService,
            ILogger<ScrapeLidlPromotionsCommandHandler> logger) : base(logger)
        {
            this.webDriverFactory = webDriverFactory;
            this.mediator = mediator;
            this.transliterationService = transliterationService;
            this.logger = logger;
        }

        public async Task<CommandResult> Handle(
            ScrapeLidlPromotionsCommand request,
            CancellationToken cancellationToken)
        {
            var webDriver = webDriverFactory.CreateWebDriver();
            var promotionalItems = new ConcurrentBag<Promotion>();

            try
            {
                webDriver.Navigate().GoToUrl(request.PromotionsPageUrl);

                try
                {
                    var confirmCookiePoliciesButton =
                        new WebDriverWait(webDriver, TimeSpan.FromSeconds(20))
                            .Until(ExpectedConditions
                                .ElementToBeClickable(
                                    By.CssSelector("#CybotCookiebotDialog > div > div.cookie-alert-extended-controls > button")));

                    confirmCookiePoliciesButton?.Click();
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Encountered error while trying to close 'ConfirmCookiePolicies' popup. Reason {e.Message}.");
                }

                // TODO: Replace with ExpectElement to be present.
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await ScrollToBottomOfPage(webDriver);

                var categoriesSelector =
                    request.PromotionWeek == PromotionWeek.ThisWeek
                        ? "#pageMain > div > section > div > ul > li:nth-child(1) > div:nth-child(2) > div > div > div.slider__body.slick-initialized.slick-slider > div > div > div > div > div > a"
                        : "#pageMain > div > section > div > ul > li:nth-child(2) > div:nth-child(2) > div > div > div.slider__body.slick-initialized.slick-slider > div > div > div > div > div > a";

                var categories =
                    webDriver.FindElementsToBePresent(
                        By.CssSelector(categoriesSelector)).ToList();

                var categoriesUrls = categories.Select(x => x.GetAttribute("href")).ToList();

                foreach (var category in categoriesUrls)
                {
                    // TODO: Add timeout policy for each category scraping process.
                    // We don't want one slow/large/faulty page to block the entire application.

                    try
                    {
                        webDriver.Navigate().GoToUrl(category);
                        await ScrollToBottomOfPage(webDriver);

                        var promotionPeriod = new PromotionPeriod();

                        var promotionPeriodElement =
                            new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                                .Until(ExpectedConditions
                                    .ElementExists(
                                        By.CssSelector("div.ribbon__text")));

                        var matches =
                            new Regex(@"(\d\d\.\d\d)", RegexOptions.CultureInvariant)
                                .Matches(promotionPeriodElement.Text);

                        if (matches.Any())
                        {
                            var from =
                                DateTime.ParseExact(
                                    matches.First().Value.Replace(".", @"/"),
                                    @"dd/MM",
                                    CultureInfo.InvariantCulture);

                            var to =
                                DateTime.ParseExact(
                                    matches.Last().Value.Replace(".", @"/"),
                                    @"dd/MM",
                                    CultureInfo.InvariantCulture);

                            promotionPeriod.From = from;
                            promotionPeriod.To = to;
                        }

                        var promotionalElements =
                            webDriver.FindElements(
                                By.CssSelector("article.product"));

                        foreach (var promotionalElement in promotionalElements)
                        {
                            var description =
                                promotionalElement.FindElementExists(
                                    By.CssSelector("div.product__text"))?.Text.Trim();

                            var additionalInformation =
                                promotionalElement.FindElementExists(
                                    By.CssSelector("div.pricebox__basic-quantity"))?.Text.Trim();

                            var officialPrice =
                                promotionalElement.FindElementExists(
                                        By.CssSelector("span.pricebox__recommended-retail-price"))?.Text
                                    .RemoveWhitespace();

                            var promotionalPrice =
                                promotionalElement.FindElementExists(
                                    By.CssSelector("span.pricebox__price"))?.Text.RemoveWhitespace();

                            var imageUrl =
                                promotionalElement.FindElementExists(
                                    By.CssSelector("picture.picture img"))?.GetAttribute("currentSrc");

                            var isPriceParseable =
                                double.TryParse(
                                    promotionalPrice.Trim().Replace(',', '.'),
                                    out var price);

                            var promotionalItem = new Promotion
                            {
                                DescriptionRaw = $"{description} ({additionalInformation})",
                                PriceRaw = promotionalPrice,
                                Price = isPriceParseable ? Math.Round(price, 2, MidpointRounding.ToZero) : 0,
                                OfficialPrice = officialPrice,
                                ImageUrl = imageUrl,
                                PromotionStartingFrom = promotionPeriod.From,
                                PromotionEndingAt = promotionPeriod.To,
                                Store = Store.Lidl,
                            };

                            promotionalItem.DescriptionSearch = await transliterationService.LatinToCyrillicAsync(promotionalItem.DescriptionRaw.ToLowerInvariant(), LanguageHint.Bulgarian);

                            promotionalItems.Add(promotionalItem);

                            logger.LogInformation(
                                $"{JsonConvert.SerializeObject(promotionalItem, Formatting.Indented)}");
                        }
                    }
                    catch (Exception)
                    {
                        // TODO: Add to list of failures.
                        // Different categories/pages might fail to be scraped for different reasons.
                    }
                }

                var scrapedPromotionsEvent = new ScrapedPromotionsEvent
                {
                    Store = Store.Lidl,
                    PromotionWeek = request.PromotionWeek,
                    Promotions = promotionalItems
                        .DistinctBy(x => $"{x.DescriptionRaw}-{x.PriceRaw}")
                        .ToList(),
                    EventDate = DateTime.UtcNow,
                    EventId = Guid.NewGuid(),
                    CorrelationId = request.CorrelationId,
                };

                await mediator.Publish(scrapedPromotionsEvent, cancellationToken);

                return new CommandResult(true, $"Successfully scraped {Store.Lidl} Promotions for {request.PromotionWeek}.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Failed to scrape {Store.Lidl} Promotions for { request.PromotionWeek}.");

                //await this.mediator.Publish(scrapingPromotionsFailedEvent, cancellationToken);

                return new CommandResult(false, $"Failed to scrape {Store.Lidl} Promotions for {request.PromotionWeek}.");
            }
            finally
            {
                webDriver.Dispose();
            }
        }
    }
}
