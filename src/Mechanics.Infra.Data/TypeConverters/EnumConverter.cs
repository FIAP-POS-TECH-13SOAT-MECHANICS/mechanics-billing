using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Mechanics.Infra.Data.TypeConverters;

public class EnumConverter<T> : IPropertyConverter where T : struct, Enum
{
    public DynamoDBEntry ToEntry(object value) =>
        new Primitive(((T)value).ToString());

    public object FromEntry(DynamoDBEntry entry) =>
        Enum.Parse<T>(entry.AsString());
}
