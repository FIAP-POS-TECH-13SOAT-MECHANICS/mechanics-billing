using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Mechanics.Infra.Data.TypeConverters;

public class GuidTypeConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object? value)
    {
        if (value is null)
            return new DynamoDBNull();

        if (value is not Guid guid)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be a valid Guid.");

        DynamoDBEntry entry = new Primitive { Value = guid.ToString() };

        return entry;
    }

    public object? FromEntry(DynamoDBEntry entry)
    {
        if (entry.AsDynamoDBNull() is not null)
            return null;

        if (entry is not Primitive { Value: string } primitive ||
            string.IsNullOrEmpty((string)primitive.Value) ||
            !Guid.TryParse((string)primitive.Value, out var guidValue))
            throw new ArgumentOutOfRangeException(nameof(entry), entry, "Invalid DynamoDB entry format for Guid conversion.");

        return guidValue;
    }
}
