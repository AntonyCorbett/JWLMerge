namespace JWLMerge.BackupFileServices.Models.DatabaseModels;

public class InputField
{
    public int LocationId { get; set; }

    public string TextTag { get; set; } = null!;

    public string Value { get; set; } = null!;

    public InputField Clone()
    {
        return (InputField)MemberwiseClone();
    }
}