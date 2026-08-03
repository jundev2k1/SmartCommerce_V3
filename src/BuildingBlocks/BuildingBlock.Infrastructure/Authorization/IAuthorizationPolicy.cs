namespace SmartEcommerce.BuildingBlock.Infrastructure.Authorization;

/// <summary>
/// Marker interface for authorization policies.
/// Policies read claims from the JWT (set by API Gateway) and make authorization decisions.
/// </summary>
public interface IAuthorizationPolicy;
