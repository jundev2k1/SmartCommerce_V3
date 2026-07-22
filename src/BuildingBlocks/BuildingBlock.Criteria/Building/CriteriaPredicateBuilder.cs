using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

using BuildingBlock.Criteria.Definition;
using BuildingBlock.Criteria.Enums;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Criteria.Strategies;

namespace BuildingBlock.Criteria.Building;

/// <summary>
/// Turns a <see cref="CriteriaRequest"/> + <see cref="CriteriaDefinition{TEntity}"/> into a plain
/// <see cref="System.Linq.Expressions"/> predicate. This is pure BCL - no EF Core reference - so it's reusable as-is
/// by any LINQ-based provider (EF Core today; a future in-memory/Mongo LINQ provider tomorrow). Each filter's
/// predicate comes from the field's <see cref="IFieldQueryStrategy{TEntity}"/> when one is registered, otherwise
/// it's built generically from the field's <see cref="CriteriaFieldType"/>/operator - there is no per-field
/// special-casing here, including for Phone.
/// </summary>
public static class CriteriaPredicateBuilder<TEntity>
{
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
    private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
    private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;

    public static Expression<Func<TEntity, bool>>? Build(CriteriaDefinition<TEntity> definition, CriteriaRequest request)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression? body = null;

        foreach (var filter in request.Filters)
        {
            if (!definition.TryGetField(filter.Field, out var field))
                continue;

            var predicate = field.Strategy is not null
                ? field.Strategy.BuildPredicate(parameter, filter.Operator, filter.Value)
                : BuildStandardPredicate(parameter, field, filter.Operator, filter.Value);

            body = body is null ? predicate : Expression.AndAlso(body, predicate);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keywordPredicate = BuildKeywordPredicate(parameter, definition, request.Keyword);
            if (keywordPredicate is not null)
                body = body is null ? keywordPredicate : Expression.AndAlso(body, keywordPredicate);
        }

        return body is null ? null : Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    private static Expression BuildStandardPredicate(
        ParameterExpression parameter, CriteriaFieldMetadata<TEntity> field, CriteriaOperator op, JsonElement? value)
    {
        var property = ParameterRebinder.ReplaceParameter(field.Accessor, parameter);
        var propertyType = property.Type;

        switch (op)
        {
            case CriteriaOperator.IsNull:
                return Expression.Equal(property, Expression.Constant(null, propertyType));

            case CriteriaOperator.IsNotNull:
                return Expression.NotEqual(property, Expression.Constant(null, propertyType));

            case CriteriaOperator.Between:
                {
                    var values = CriteriaValueConverter.ConvertMany(value, propertyType, field.FieldType);
                    var lower = Expression.Constant(values[0], propertyType);
                    var upper = Expression.Constant(values[1], propertyType);
                    return Expression.AndAlso(Expression.GreaterThanOrEqual(property, lower), Expression.LessThanOrEqual(property, upper));
                }

            case CriteriaOperator.In:
            case CriteriaOperator.NotIn:
                {
                    var contains = BuildContainsList(property, propertyType, field, value);
                    return op == CriteriaOperator.In ? contains : Expression.Not(contains);
                }

            case CriteriaOperator.Contains:
                return Expression.Call(property, ContainsMethod, BuildConstant(property, propertyType, field, value));

            case CriteriaOperator.StartsWith:
                return Expression.Call(property, StartsWithMethod, BuildConstant(property, propertyType, field, value));

            case CriteriaOperator.EndsWith:
                return Expression.Call(property, EndsWithMethod, BuildConstant(property, propertyType, field, value));

            case CriteriaOperator.Eq:
                return Expression.Equal(property, BuildConstant(property, propertyType, field, value));

            case CriteriaOperator.Ne:
                return Expression.NotEqual(property, BuildConstant(property, propertyType, field, value));

            case CriteriaOperator.Gt:
                return Expression.GreaterThan(property, BuildConstant(property, propertyType, field, value));

            case CriteriaOperator.Gte:
                return Expression.GreaterThanOrEqual(property, BuildConstant(property, propertyType, field, value));

            case CriteriaOperator.Lt:
                return Expression.LessThan(property, BuildConstant(property, propertyType, field, value));

            case CriteriaOperator.Lte:
                return Expression.LessThanOrEqual(property, BuildConstant(property, propertyType, field, value));

            default:
                throw new NotSupportedException($"Operator '{op}' is not supported.");
        }
    }

    private static ConstantExpression BuildConstant(
        Expression property, Type propertyType, CriteriaFieldMetadata<TEntity> field, JsonElement? value)
        => Expression.Constant(CriteriaValueConverter.Convert(value, propertyType, field.FieldType), propertyType);

    private static MethodCallExpression BuildContainsList(
        Expression property, Type propertyType, CriteriaFieldMetadata<TEntity> field, JsonElement? value)
    {
        var values = CriteriaValueConverter.ConvertMany(value, propertyType, field.FieldType);
        var listType = typeof(List<>).MakeGenericType(propertyType);
        var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
        foreach (var item in values)
            list.Add(item);

        var containsMethod = listType.GetMethod(nameof(List<object>.Contains))!;
        return Expression.Call(Expression.Constant(list, listType), containsMethod, property);
    }

    private static Expression? BuildKeywordPredicate(
        ParameterExpression parameter, CriteriaDefinition<TEntity> definition, string keyword)
    {
        Expression? body = null;

        foreach (var field in definition.KeywordFields)
        {
            var property = ParameterRebinder.ReplaceParameter(field.Accessor, parameter);
            var call = Expression.Call(property, ContainsMethod, Expression.Constant(keyword));
            body = body is null ? call : Expression.OrElse(body, call);
        }

        return body;
    }
}
