using CommunityToolkit.Mvvm.ComponentModel;

namespace JWLMerge.Models;

public class MergeParameters : ObservableObject
{
    private bool _includeBookmarks;
    private bool _includeNotes;
    private bool _includeUnderlining;
    private bool _includeTags;
    private bool _includeInputFields;

    public MergeParameters()
    {
        IncludeBookmarks = true;
        IncludeNotes = true;
        IncludeUnderlining = true;
        IncludeTags = true;
        IncludeInputFields = true;
    }

    public bool IncludeInputFields
    {
        get => _includeInputFields;
        set
        {
            if (_includeInputFields != value)
            {
                _includeInputFields = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IncludeBookmarks
    {
        get => _includeBookmarks;
        set
        {
            if (_includeBookmarks != value)
            {
                _includeBookmarks = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IncludeNotes
    {
        get => _includeNotes;
        set
        {
            if (_includeNotes != value)
            {
                _includeNotes = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IncludeUnderlining
    {
        get => _includeUnderlining;
        set
        {
            if (_includeUnderlining != value)
            {
                _includeUnderlining = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IncludeTags
    {
        get => _includeTags;
        set
        {
            if (_includeTags != value)
            {
                _includeTags = value;
                OnPropertyChanged();
            }
        }
    }

    public bool AnyIncludes()
    {
        return IncludeTags || IncludeBookmarks || IncludeNotes || IncludeUnderlining || IncludeInputFields;
    }

    public bool AnyExcludes()
    {
        return !IncludeTags || !IncludeBookmarks || !IncludeNotes || !IncludeUnderlining || !IncludeInputFields;
    }
}