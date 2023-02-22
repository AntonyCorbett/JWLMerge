namespace JWLMerge.EventTracking;

internal enum EventName
{
    Unknown,
    ExportNotes,
    ImportNotes,
    RemoveUnderliningByPubColour,
    RemoveUnderliningByColour,
    RemoveNotesByTag,
    RedactNotes,
    RemoveFavs,
    Merge,
    ShowDetails,
    WrongVer,
    WrongManifestVer,
    LoggingFailed
}