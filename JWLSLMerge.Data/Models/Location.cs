using JWLSLMerge.Data.Attributes;

namespace JWLSLMerge.Data.Models
{
    public class Location
    {
        [Ignore]
        public int LocationId { get; set; }
        public int? BookNumber { get; set; }
        public int? ChapterNumber { get; set; }
        public int? DocumentId { get; set; }
        public int? Track { get; set; }
        public int IssueTagNumber { get; set; }
        public string? KeySymbol { get; set; }
        public int? MepsLanguage { get; set; }
        public int Type { get; set; }
        public string? Title { get; set; }

        [Ignore]
        public int NewLocationId { get; set; }
    }
}
