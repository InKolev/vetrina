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
using SeleniumExtras.WaitHelpers;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Extensions;
using Vetrina.Server.Mediatr.Commands;
using Vetrina.Server.Mediatr.Events;
using Vetrina.Shared;

namespace Vetrina.Server.Mediatr.CommandHandlers;

public class ScrapeBillaPromotionsCommandHandler :
    BaseScrapingCommandHandler<ScrapeBillaPromotionsCommandHandler>,
    IRequestHandler<ScrapeBillaPromotionsCommand, CommandResult>
{
    private readonly IWebDriverFactory webDriverFactory;
    private readonly IMediator mediator;
    private readonly ITransliterationService transliterationService;
    private readonly ILogger<ScrapeBillaPromotionsCommandHandler> logger;

    public ScrapeBillaPromotionsCommandHandler(
        IWebDriverFactory webDriverFactory,
        IMediator mediator,
        ITransliterationService transliterationService,
        ILogger<ScrapeBillaPromotionsCommandHandler> logger) : base(logger)
    {
        this.webDriverFactory = webDriverFactory;
        this.mediator = mediator;
        this.transliterationService = transliterationService;
        this.logger = logger;
    }

    public async Task<CommandResult> Handle(
        ScrapeBillaPromotionsCommand request,
        CancellationToken cancellationToken)
    {
        var webDriver = webDriverFactory.CreateWebDriver();
        var promotionalItems = new ConcurrentBag<Promotion>();

        try
        {
            webDriver.Navigate().GoToUrl(request.PromotionsPageUrl);

            new WebDriverWait(webDriver, TimeSpan.FromSeconds(15))
                .Until(ExpectedConditions
                    .ElementExists(
                        By.CssSelector("div.productSection")));

            await ScrollToBottomOfPage(webDriver);

            try
            {
                var promotionPeriod = new PromotionPeriod();

                var promotionPeriodElement =
                    new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                        .Until(ExpectedConditions
                            .ElementExists(
                                By.CssSelector("div.productSection > div:nth-child(2) > div.actualProduct")));

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

                var promotionalElements =
                    webDriver.FindElements(
                        By.CssSelector("div.product"));

                foreach (var promotionalElement in promotionalElements)
                {
                    var discount =
                        promotionalElement.FindElementExists(
                            By.CssSelector("div.discount"))?.Text.RemoveWhitespace();

                    var isActualProduct = double.TryParse(
                        discount?.RemoveWhitespace().Trim('%').Trim('-'),
                        out var discountPercentage) && discountPercentage != 0;

                    if (!isActualProduct)
                    {
                        continue;
                    }

                    var description =
                        promotionalElement.FindElementExists(
                            By.CssSelector("div.actualProduct"))?.Text.Trim();

                    var officialPrice =
                        promotionalElement.FindElementExists(
                            By.CssSelector("div:nth-child(3) > span.price"))?.Text.RemoveWhitespace();

                    var promotionalPrice =
                        promotionalElement.FindElementExists(
                            By.CssSelector("div:nth-child(6) > span.price"))?.Text.RemoveWhitespace();

                    //var imageUrl =
                    //    promotionalElement.FindElementExists(
                    //        By.CssSelector("picture.picture img"))?.GetAttribute("currentSrc");

                    //var additionalInformation =
                    //    promotionalElement.FindElementExists(
                    //        By.CssSelector("div.pricebox__basic-quantity"))?.Text.Trim();


                    var isPriceParseable =
                        double.TryParse(
                            promotionalPrice?.Trim().Replace(',', '.'),
                            out var price);

                    var promotionalItem = new Promotion
                    {
                        DescriptionRaw = $"{description}",
                        PriceRaw = promotionalPrice,
                        Price = isPriceParseable ? Math.Round(price, 2, MidpointRounding.ToZero) : 0,
                        OfficialPrice = officialPrice,
                        //ImageUrl = imageUrl,
                        DiscountPercentage = discount,
                        PromotionStartingFrom = promotionPeriod.From,
                        PromotionEndingAt = promotionPeriod.To,
                        Store = Store.Billa
                    };

                    promotionalItem.DescriptionSearch = await transliterationService.LatinToCyrillicAsync(promotionalItem.DescriptionRaw.ToLowerInvariant(), LanguageHint.Bulgarian);

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

            var scrapedPromotionsEvent = new ScrapedPromotionsEvent
            {
                Store = Store.Billa,
                PromotionWeek = request.PromotionWeek,
                Promotions = promotionalItems
                    .DistinctBy(x => $"{x.DescriptionRaw}-{x.PriceRaw}")
                    .ToList(),
                EventDate = DateTime.UtcNow,
                EventId = Guid.NewGuid(),
                CorrelationId = request.CorrelationId,
            };

            await mediator.Publish(scrapedPromotionsEvent, cancellationToken);

            return new CommandResult(true, $"Successfully scraped {Store.Billa} Promotions for {request.PromotionWeek}.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, $"Failed to scrape {Store.Billa} Promotions for { request.PromotionWeek}.");

            //await this.mediator.Publish(scrapingPromotionsFailedEvent, cancellationToken);

            return new CommandResult(false, $"Failed to scrape {Store.Billa} Promotions for {request.PromotionWeek}.");
        }
        finally
        {
            webDriver.Dispose();
        }
    }
}