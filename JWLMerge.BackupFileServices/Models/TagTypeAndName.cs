namespace JWLMerge.BackupFileServices.Models
{
    internal class TagTypeAndName
    {
        public TagTypeAndName(int type, string name)
        {
            TagType = type;
            Name = name;
        }

        public int TagType { get; }

        public string Name { get; }

        public override int GetHashCode()
        {
            return new { TagType, Name }.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;

                case TagTypeAndName o:
                    return TagType == o.TagType && 
                           Name == o.Name;
                default:
                    return false;
            }
        }
    }
}
