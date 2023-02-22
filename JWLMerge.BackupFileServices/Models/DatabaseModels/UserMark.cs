namespace JWLMerge.BackupFileServices.Models.DatabaseModels;

public class UserMark
{
    /// <summary>
    /// The user mark identifier.
    /// </summary>
    public int UserMarkId { get; set; }

    /// <summary>
    /// The index of the marking (highlight) color.
    /// yellow = 1, green, blue, pink, orange, purple
    /// </summary>
    public int ColorIndex { get; set; }

    /// <summary>
    /// The location identifier.
    /// Refers to Location.LocationId
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// The style index (unused?)
    /// </summary>
    public int StyleIndex { get; set; }

    /// <summary>
    /// The guid. Useful in merging!
    /// </summary>
    public string UserMarkGuid { get; set; } = null!;

    /// <summary>
    /// The highlight version. Semantics unknown!
    /// </summary>
    public int Version { get; set; }

    public UserMark Clone()
    {
        return (UserMark)MemberwiseClone();
    }
}