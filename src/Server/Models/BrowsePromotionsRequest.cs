using MediatR;
using Vetrina.Shared;

namespace Vetrina.Server.Models
{
    public class BrowsePromotionsRequest : IRequest<Result<BrowsePromotionsResponse>>
    {
        public Store? Store { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }
    }
}
