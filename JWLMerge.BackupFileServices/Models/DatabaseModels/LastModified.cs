using Newtonsoft.Json;

namespace JWLMerge.BackupFileServices.Models.DatabaseModels;

public class LastModified
{
    /// <summary>
    /// Time stamp when the database was last modified.
    /// </summary>
    [JsonProperty(PropertyName = "LastModified")]
    public string? TimeLastModified { get; set; }

    public void Reset()
    {
        TimeLastModified = null;
    }
}