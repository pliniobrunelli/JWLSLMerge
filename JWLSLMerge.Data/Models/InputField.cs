using JWLSLMerge.Data.Attributes;

namespace JWLSLMerge.Data.Models
{
    public class InputField
    {
        public int LocationId { get; set; }
        public string TextTag { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
