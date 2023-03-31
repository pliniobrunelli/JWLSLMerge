using JWLSLMerge.Data.Attributes;

namespace JWLSLMerge.Data.Models
{
    public class PlaylistItemMarker
    {
        [Ignore]
        public int PlaylistItemMarkerId { get; set; }
        public int PlaylistItemId { get; set; }
        public string Label { get; set; } = null!;
        public long StartTimeTicks { get; set; }
        public long DurationTicks { get; set; }
        public long EndTransitionDurationTicks { get; set; }

        [Ignore]
        public int NewPlaylistItemMarkerId { get; set; }
    }
}
