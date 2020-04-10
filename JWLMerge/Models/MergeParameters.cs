namespace JWLMerge.Models
{
    using GalaSoft.MvvmLight;

    public class MergeParameters : ViewModelBase
    {
        private bool _includeBookmarks;
        private bool _includeNotes;
        private bool _includeUnderlining;
        private bool _includeTags;

        public MergeParameters()
        {
            IncludeBookmarks = true;
            IncludeNotes = true;
            IncludeUnderlining = true;
            IncludeTags = true;
        }

        public bool IncludeBookmarks
        {
            get => _includeBookmarks;
            set
            {
                if (_includeBookmarks != value)
                {
                    _includeBookmarks = value;
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                }
            }
        }

        public bool AnyIncludes()
        {
            return IncludeTags || IncludeBookmarks || IncludeNotes || IncludeUnderlining;
        }

        public bool AnyExcludes()
        {
            return !IncludeTags || !IncludeBookmarks || !IncludeNotes || !IncludeUnderlining;
        }
    }
}
