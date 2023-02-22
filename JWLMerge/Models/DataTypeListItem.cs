namespace JWLMerge.Models;

internal sealed class DataTypeListItem
{
    public DataTypeListItem(string caption, JwLibraryFileDataTypes dataType)
    {
        Caption = caption;
        DataType = dataType;
    }
        
    public string Caption { get; }
        
    public JwLibraryFileDataTypes DataType { get; }
}