using System.Text.Json.Serialization;

namespace SmartEcommerce.BuildingBlock.Criteria.Requests;

[JsonConverter(typeof(LowerCaseStringEnumConverter<SortDirection>))]
public enum SortDirection
{
    Asc,
    Desc,
}
