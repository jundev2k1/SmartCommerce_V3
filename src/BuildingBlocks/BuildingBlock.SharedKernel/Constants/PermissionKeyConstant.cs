using System.Collections.Frozen;

namespace SmartEcommerce.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Every permission key the platform recognizes, grouped by resource. Permission keys are
/// code-first (declared here, seeded into Auth's PermissionDefinition catalog, referenced by
/// authorization attributes/policies) - never free-form user input - so PermissionKey validates
/// against SupportedValues instead of a runtime format check.
/// </summary>
public static class PermissionKeyConstant
{
    public static class Product
    {
        public const string Create = "product:create";
        public const string Update = "product:update";
        public const string Delete = "product:delete";
    }

    public static class Inventory
    {
        public const string View = "inventory:view";
        public const string Update = "inventory:update";
    }

    public static class Order
    {
        public const string Create = "order:create";
        public const string View = "order:view";
        public const string Cancel = "order:cancel";
    }

    public static readonly FrozenSet<string> SupportedValues = new[]
    {
        Product.Create, Product.Update, Product.Delete,
        Inventory.View, Inventory.Update,
        Order.Create, Order.View, Order.Cancel,
    }.ToFrozenSet();
}
