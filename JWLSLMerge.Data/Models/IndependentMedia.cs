using JWLSLMerge.Data.Attributes;

namespace JWLSLMerge.Data.Models
{
    public class IndependentMedia
    {
        [Ignore]
        public int IndependentMediaId { get; set; }
        public string OriginalFilename { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string Hash { get; set; } = null!;

        [Ignore]
        public int NewIndependentMediaId { get; set; }
    }
}
