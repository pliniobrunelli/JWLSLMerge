namespace JWLSLMerge.Data.Models
{
    public class Manifest
    {
        public string Name { get; set; } = null!;
        public string CreationDate { get; set; } = null!;
        public int Version { get; set; } = 1;
        public int Type { get; set; } = 0;
        public UserDataBackup UserDataBackup { get; set; } = null!;
    }

    public class UserDataBackup
    {
        public string LastModifiedDate { get; set; } = null!;
        public string DeviceName { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string Hash { get; set; } = null!;
        public int SchemaVersion { get; set; }
    }
}
