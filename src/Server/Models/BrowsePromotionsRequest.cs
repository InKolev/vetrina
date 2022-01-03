using MediatR;
using Vetrina.Shared;

namespace Vetrina.Server.Models
{
    public class BrowsePromotionsRequest : IRequest<Result<BrowsePromotionsResponse>>
    {
        public Store? Store { get; set; }

        public int PageSize { get; set; } = 20;

        public int Page { get; set; } = 1;

        public int Skip { get; set; }

        public int Take { get; set; }
    }
}
