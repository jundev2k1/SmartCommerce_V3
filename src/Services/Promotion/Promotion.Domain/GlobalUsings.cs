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

// The former CouponCode entity/Value Object collision (Phase 2.2) no longer exists - the
// per-aggregate Code Value Object was consolidated into the shared EntityCode during the Phase
// 2.5 Domain Standardization Review, so no CouponCodeEntity alias is needed anymore.
