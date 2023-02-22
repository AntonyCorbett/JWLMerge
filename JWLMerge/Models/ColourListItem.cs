using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace JWLMerge.Models;

internal sealed class ColourListItem : ObservableObject
{
    private bool _isChecked;

    public ColourListItem(string name, int id, Color color)
    {
        Name = name;
        Id = id;
        Color = color;
    }

    public string Name { get; }

    public int Id { get; }

    public Color Color { get; }

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