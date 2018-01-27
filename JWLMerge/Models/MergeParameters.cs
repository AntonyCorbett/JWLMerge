namespace JWLMerge.Models
{
    using GalaSoft.MvvmLight;

    public class MergeParameters : ViewModelBase
    {
        private bool _includeBookmarks;

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

        private bool _includeNotes;

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

        private bool _includeUnderlining;

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

        private bool _includeTags;

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

        public MergeParameters()
        {
            IncludeBookmarks = true;
            IncludeNotes = true;
            IncludeUnderlining = true;
            IncludeTags = true;
        }

        public bool AnyIncludes()
        {
            return IncludeTags || IncludeBookmarks || IncludeNotes || IncludeUnderlining;
        }
    }
}
