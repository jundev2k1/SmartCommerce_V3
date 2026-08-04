using System.Collections.Frozen;

namespace SmartEcommerce.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Every permission key the platform recognizes, grouped by business capability. Permission keys
/// are code-first (declared here, seeded into Auth's PermissionDefinition catalog, referenced by
/// RequirePermissions() on endpoints) - never free-form user input - so PermissionKey validates
/// against SupportedValues instead of a runtime format check.
///
/// Root bypasses every check. Each module's "Full" key is an aggregate that implicitly grants
/// every other permission in that module - this is resolved centrally by
/// ClaimsPrincipalExtensions.HasAnyPermission, endpoints never need to declare it explicitly.
/// </summary>
public static class Permissions
{
    public const string Root = "system:root";

    public static class Product
    {
        public const string Manage = "product:manage";
        public const string Full = "product:full";
    }

    public static class Inventory
    {
        public const string Manage = "inventory:manage";
        public const string Full = "inventory:full";
    }

    public static class Warehouse
    {
        public const string Manage = "warehouse:manage";
        public const string Full = "warehouse:full";
    }

    public static class Order
    {
        public const string Manage = "order:manage";
        public const string Full = "order:full";
    }

    public static class Audit
    {
        public const string View = "audit:view";
        public const string Full = "audit:full";
    }

    public static class Notification
    {
        public const string Manage = "notification:manage";
        public const string Full = "notification:full";
    }

    public static class Users
    {
        public const string Manage = "users:manage";
        public const string Full = "users:full";
    }

    /// <summary>Platform-operational capabilities (e.g. dead-letter queue management) not owned by any single business module.</summary>
    public static class System
    {
        public const string Manage = "system:manage";
        public const string Full = "system:full";
    }

    public static readonly FrozenSet<string> SupportedValues = new[]
    {
        Root,
        Product.Manage, Product.Full,
        Inventory.Manage, Inventory.Full,
        Warehouse.Manage, Warehouse.Full,
        Order.Manage, Order.Full,
        Audit.View, Audit.Full,
        Notification.Manage, Notification.Full,
        Users.Manage, Users.Full,
        System.Manage, System.Full,
    }.ToFrozenSet();
}
