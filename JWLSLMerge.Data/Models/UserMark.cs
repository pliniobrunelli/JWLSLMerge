using JWLSLMerge.Data.Attributes;

namespace JWLSLMerge.Data.Models
{
    public class UserMark
    {
        [Ignore]
        public int UserMarkId { get; set; }
        public int ColorIndex { get; set; }
        public int LocationId { get; set; }
        public int StyleIndex { get; set; }
        public string UserMarkGuid { get; set; } = null!;
        public int Version { get; set; }

        [Ignore]
        public int NewUserMarkId { get; set; }
    }
}
