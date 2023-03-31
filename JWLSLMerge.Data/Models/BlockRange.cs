using JWLSLMerge.Data.Attributes;

namespace JWLSLMerge.Data.Models
{
    public class BlockRange
    {
        [Ignore]
        public int BlockRangeId { get; set; }
        public int BlockType { get; set; }
        public int Identifier { get; set; }
        public int? StartToken { get; set; }
        public int? EndToken { get; set; }
        public int UserMarkId { get; set; }
    }
}
