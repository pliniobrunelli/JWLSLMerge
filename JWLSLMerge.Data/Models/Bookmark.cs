using JWLSLMerge.Data.Attributes;

namespace JWLSLMerge.Data.Models
{
    public class Bookmark
    {
        [Ignore]
        public int BookmarkId { get; set; }
        public int LocationId { get; set; }
        public int PublicationLocationId { get; set; }
        public int Slot { get; set; }
        public string Title { get; set; } = null!;
        public string? Snippet { get; set; }
        public int BlockType { get; set; }
        public int? BlockIdentifier { get; set; }
    }
}
