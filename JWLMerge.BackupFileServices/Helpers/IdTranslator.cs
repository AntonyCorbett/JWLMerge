using System.Collections.Generic;

namespace JWLMerge.BackupFileServices.Helpers;

/// <summary>
/// Used by the <see cref="Merger"/> to map old and new id values./>
/// </summary>
internal sealed class IdTranslator
{
    private readonly Dictionary<int, int> _ids;

    public IdTranslator()
    {
        _ids = new Dictionary<int, int>();
    }

    public int GetTranslatedId(int oldId)
    {
        return _ids.TryGetValue(oldId, out var translatedId) ? translatedId : 0;
    }

    public void Add(int oldId, int translatedId)
    {
        _ids[oldId] = translatedId;
    }

    public void Clear()
    {
        _ids.Clear();
    }
}