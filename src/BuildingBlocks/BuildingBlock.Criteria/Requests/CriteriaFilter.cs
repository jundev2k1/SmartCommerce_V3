using System.Text.Json;

using BuildingBlock.Criteria.Enums;

namespace BuildingBlock.Criteria.Requests;

public sealed record CriteriaFilter(string Field, CriteriaOperator Operator, JsonElement? Value);
