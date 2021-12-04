using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Extensions;
using Vetrina.Server.Mediatr.Commands;
using Vetrina.Server.Mediatr.Events;
using Vetrina.Server.Mediatr.Shared;
using Vetrina.Shared;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Vetrina.Server.Mediatr.CommandHandlers
{
    public class ScrapeKauflandPromotionsCommandHandler :
        BaseScrapingCommandHandler<ScrapeKauflandPromotionsCommandHandler>,
        IRequestHandler<ScrapeKauflandPromotionsCommand, CommandResult>
    {
        private readonly IWebDriverFactory webDriverFactory;
        private readonly IMediator mediator;
        private readonly ITransliterationService transliterationService;
        private readonly ILogger<ScrapeKauflandPromotionsCommandHandler> logger;

        public ScrapeKauflandPromotionsCommandHandler(
            IWebDriverFactory webDriverFactory,
            IMediator mediator,
            ITransliterationService transliterationService,
            ILogger<ScrapeKauflandPromotionsCommandHandler> logger) : base(logger)
        {
            this.webDriverFactory = webDriverFactory;
            this.mediator = mediator;
            this.transliterationService = transliterationService;
            this.logger = logger;
        }

        public async Task<CommandResult> Handle(
            ScrapeKauflandPromotionsCommand request,
            CancellationToken cancellationToken)
        {
            var webDriver = webDriverFactory.CreateWebDriver();
            var promotionalItems = new ConcurrentBag<Promotion>();

            try
            {
                webDriver.Navigate().GoToUrl(request.PromotionsPageUrl);

                TryCloseCookiePoliciesPopup(webDriver);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await ScrollToBottomOfPage(webDriver);

                var categories =
                    webDriver.FindElementsToBePresent(
                        By.CssSelector("#offers-overview-1 > ul > li > a")).ToList();

                var specialThemes =
                    webDriver.FindElementsToBePresent(
                        By.CssSelector("#offers-overview-2 > ul > li > a")).ToList();

                categories.AddRange(specialThemes);

                var categoriesUrls = categories.Select(x => x.GetAttribute("href")).ToList();

                foreach (var category in categoriesUrls)
                {
                    // TODO: Add timeout policy for each category scraping process.
                    // We don't want one slow/large/faulty page to block the entire application.
                    var temporaryWebDriver = webDriverFactory.CreateWebDriver();

                    try
                    {
                        temporaryWebDriver.Navigate().GoToUrl(category);

                        TryCloseCookiePoliciesPopup(temporaryWebDriver);

                        await ScrollToBottomOfPage(temporaryWebDriver);

                        var promotionPeriod = new PromotionPeriod();

                        try
                        {
                            var promotionPeriodElement =
                                new WebDriverWait(temporaryWebDriver, TimeSpan.FromSeconds(10))
                                    .Until(ExpectedConditions
                                        .ElementExists(
                                            By.CssSelector("div.a-icon-tile-headline__subheadline > div")));

                            var matches =
                                new Regex(@"(\d\d\.\d\d\.\d\d\d\d)", RegexOptions.CultureInvariant)
                                    .Matches(promotionPeriodElement.Text);

                            if (matches.Any())
                            {
                                var from =
                                    DateTime.ParseExact(
                                        matches.First().Value.Replace(".", @"/"),
                                        @"dd/MM/yyyy",
                                        CultureInfo.InvariantCulture);

                                var to =
                                    DateTime.ParseExact(
                                        matches.Last().Value.Replace(".", @"/"),
                                        @"dd/MM/yyyy",
                                        CultureInfo.InvariantCulture);

                                promotionPeriod.From = from;
                                promotionPeriod.To = to;
                            }

                        }
                        catch (Exception e)
                        {
                            logger.LogError(e,
                                $"Failed to retrieve promotions period from page: {category}");

                            // Extract (Assume) promotion period from the request.PromotionWeek value.
                            promotionPeriod.From =
                                request.PromotionWeek == PromotionWeek.ThisWeek
                                    ? DateTime.UtcNow.StartOfWeek()
                                    : DateTime.UtcNow.AddDays(7).StartOfWeek();

                            promotionPeriod.To =
                                request.PromotionWeek == PromotionWeek.ThisWeek
                                    ? DateTime.UtcNow.EndOfWeek()
                                    : DateTime.UtcNow.AddDays(7).EndOfWeek();
                        }

                        var promotionalElements =
                            temporaryWebDriver.FindElements(
                                By.CssSelector("div.o-overview-list__list-item"));

                        foreach (var promotionalElement in promotionalElements)
                        {
                            var description =
                                promotionalElement.FindElementExists(
                                    By.CssSelector("div.m-offer-tile__text"))?.Text;

                            var discount =
                                promotionalElement.FindElementExists(
                                        By.CssSelector("div.a-pricetag__discount"))?.Text
                                    .Replace("-", string.Empty)
                                    .Replace("%", string.Empty);

                            var officialPrice =
                                promotionalElement.FindElementExists(
                                    By.CssSelector("div.a-pricetag__old-price"))?.Text.RemoveWhitespace();

                            var promotionalPrice =
                                promotionalElement.FindElementExists(
                                    By.CssSelector("div.a-pricetag__price"))?.Text.RemoveWhitespace();

                            var imageUrl =
                                promotionalElement.FindElementExists(
                                    By.CssSelector("figure > img"))?.GetAttribute("currentSrc");

                            var promotionalItem = new Promotion
                            {
                                DescriptionRaw = description,
                                DiscountPercentage = discount,
                                Price = promotionalPrice,
                                OfficialPrice = officialPrice,
                                ImageUrl = imageUrl,
                                PromotionStartingFrom = promotionPeriod.From,
                                PromotionEndingAt = promotionPeriod.To,
                                Store = Store.Kaufland
                            };

                            promotionalItems.Add(promotionalItem);

                            logger.LogInformation(
                                $"{JsonConvert.SerializeObject(promotionalItem, Formatting.Indented)}");
                        }
                    }
                    catch (Exception e)
                    {
                        // TODO: Add to list of failures.
                        // Different categories/pages might fail to be scraped for different reasons.
                    }
                    finally
                    {
                        temporaryWebDriver.Dispose();
                    }
                }

                var scrapedPromotionsEvent = new ScrapedPromotionsEvent
                {
                    Store = Store.Kaufland,
                    PromotionWeek = request.PromotionWeek,
                    PromotionalItems = promotionalItems
                        .Pipe(async x => x.DescriptionSearch = await transliterationService.LatinToCyrillicAsync(x.DescriptionRaw.ToLowerInvariant(), LanguageHint.Bulgarian))
                        .DistinctBy(x => $"{x.DescriptionRaw}-{x.Price}")
                        .ToList(),
                    EventDate = DateTime.UtcNow,
                    EventId = Guid.NewGuid(),
                    CorrelationId = request.CorrelationId
                };

                await mediator.Publish(scrapedPromotionsEvent, cancellationToken);

                return new CommandResult(true, $"Successfully scraped {Store.Kaufland} Promotions for {request.PromotionWeek}.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Failed to scrape {Store.Kaufland} Promotions for { request.PromotionWeek}.");

                //await this.mediator.Publish(scrapingPromotionsFailedEvent, cancellationToken);

                return new CommandResult(false, $"Failed to scrape {Store.Kaufland} Promotions for {request.PromotionWeek}.");
            }
            finally
            {
                webDriver.Dispose();
            }
        }

        private void TryCloseCookiePoliciesPopup(IWebDriver webDriver)
        {
            try
            {
                var confirmCookiePoliciesButton =
                    new WebDriverWait(webDriver, TimeSpan.FromSeconds(7))
                        .Until(ExpectedConditions
                            .ElementToBeClickable(
                                By.CssSelector("#CybotCookiebotDialog > div > div.cookie-alert-extended-controls > button")));

                confirmCookiePoliciesButton?.Click();
            }
            catch (Exception e)
            {
                // Pass through.
                logger.LogWarning("Couldn't find a 'Confirm cookie policies' popup. Skip close popup step.");
            }
        }
    }
}
