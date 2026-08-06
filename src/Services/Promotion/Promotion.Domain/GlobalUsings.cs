global using System;
global using System.Collections.Generic;
global using System.Linq;

global using NovaCore.BuildingBlock.Domain.Abstractions;
global using NovaCore.BuildingBlock.Domain.Exceptions;
global using NovaCore.BuildingBlock.Domain.ValueObjects;
global using NovaCore.BuildingBlock.SharedKernel.Extensions;

global using NovaCore.Promotion.Domain.Enums;
global using NovaCore.Promotion.Domain.Metadata;
global using NovaCore.Promotion.Domain.ValueObjects;

// "Promotion" collides with this project's own root namespace (NovaCore.Promotion.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs
// an alias (same issue Payment/Order/Product/User already document for their own root-namespaced
// entity).
global using PromotionEntity = NovaCore.Promotion.Domain.Entities.Promotions.Promotion;

// "CouponCode" collides between the CouponCode Value Object (NovaCore.Promotion.Domain.ValueObjects,
// globally used above) and the CouponCode entity (individually issued codes under a Coupon) - same
// root-namespace-style collision pattern, aliasing the entity side (the VO keeps the bare name,
// same precedent as PromotionEntity above).
global using CouponCodeEntity = NovaCore.Promotion.Domain.Entities.Coupons.CouponCode;
