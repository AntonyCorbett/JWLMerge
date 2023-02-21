using CommunityToolkit.Mvvm.ComponentModel;

namespace JWLMerge.Models;

internal sealed class TagListItem : ObservableObject
{
    private bool _isChecked;

    public TagListItem(string name, int id)
    {
        Name = name;
        Id = id;
    }

    public string Name { get; }

    public int Id { get; }

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