using System;

namespace Vetrina.Server.Domain
{
    public class SearchRecord
    {
        public SearchRecord()
        {
        }

        public SearchRecord(string keyword, string ipAddress, DateTime? eventDate = default)
        {
            Keyword = keyword;
            IpAddress = ipAddress;
            EventDate = eventDate ?? DateTime.UtcNow;
        }

        public int Id { get; set; }

        public string Keyword { get; set; }

        public string IpAddress { get; set; }

        public DateTime EventDate { get; set; }
    }
}