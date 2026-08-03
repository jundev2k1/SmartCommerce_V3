using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartEcommerce.BuildingBlock.Criteria.Requests;

public sealed class LowerCaseStringEnumConverter<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    public LowerCaseStringEnumConverter() : base(JsonNamingPolicy.CamelCase)
    {
    }
}
