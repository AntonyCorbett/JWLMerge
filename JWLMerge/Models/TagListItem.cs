namespace JWLMerge.Models
{
    using Microsoft.Toolkit.Mvvm.ComponentModel;

    internal class TagListItem : ObservableObject
    {
        private bool _isChecked;

        public string Name { get; set; }

        public int Id { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public override string ToString() => Name;
    }
}
