using System.Text.Json.Serialization;

namespace SmartEcommerce.BuildingBlock.Criteria.Enums;

[JsonConverter(typeof(CriteriaOperatorJsonConverter))]
public enum CriteriaOperator
{
    Eq,
    Ne,
    Gt,
    Gte,
    Lt,
    Lte,
    Contains,
    StartsWith,
    EndsWith,
    In,
    NotIn,
    Between,
    IsNull,
    IsNotNull,
}
