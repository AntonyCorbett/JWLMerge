namespace JWLMerge.Models
{
    internal class TagListItem
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public bool IsChecked { get; set; }

        public override string ToString() => Name;
    }
}
