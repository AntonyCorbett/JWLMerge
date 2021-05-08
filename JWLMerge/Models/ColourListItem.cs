﻿namespace JWLMerge.Models
{
    using System.Windows.Media;
    using GalaSoft.MvvmLight;

    internal class ColourListItem : ObservableObject
    {
        private bool _isChecked;

        public string Name { get; set; }

        public int Id { get; set; }

        public Color Color { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override string ToString() => Name;
    }
}