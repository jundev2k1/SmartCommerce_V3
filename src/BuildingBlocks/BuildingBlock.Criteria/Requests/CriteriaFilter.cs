using System.Text.Json;

using SmartEcommerce.BuildingBlock.Criteria.Enums;

namespace SmartEcommerce.BuildingBlock.Criteria.Requests;

public sealed record CriteriaFilter(string Field, CriteriaOperator Operator, JsonElement? Value);
