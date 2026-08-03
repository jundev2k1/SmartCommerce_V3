using System.Linq.Expressions;

using SmartEcommerce.BuildingBlock.Criteria.Enums;
using SmartEcommerce.BuildingBlock.Criteria.Strategies;

namespace SmartEcommerce.BuildingBlock.Criteria.Definition;

public sealed record CriteriaFieldMetadata<TEntity>
{
    public required string LogicalName { get; init; }
    public required LambdaExpression Accessor { get; init; }
    public required CriteriaFieldType FieldType { get; init; }
    public required IReadOnlySet<CriteriaOperator> AllowedOperators { get; init; }
    public bool Sortable { get; init; }
    public bool KeywordSearchable { get; init; }
    public bool IgnoreCase { get; init; }
    public IFieldQueryStrategy<TEntity>? Strategy { get; init; }
}
