namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    public class InputField
    {
        public int LocationId { get; set; }

        public string TextTag { get; set; }

        public string Value { get; set; }

        public InputField Clone()
        {
            return (InputField)MemberwiseClone();
        }
    }
}
