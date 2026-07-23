global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using BuildingBlock.Application.Abstractions.CQRS;
global using BuildingBlock.Application.Abstractions.Events;
global using BuildingBlock.Application.Abstractions.Persistence;
global using BuildingBlock.Domain.Enums;
global using BuildingBlock.Domain.Exceptions;
global using BuildingBlock.SharedKernel.Constants;
global using BuildingBlock.SharedKernel.Extensions;

global using Order.Domain.Entities;
global using Order.Domain.Enums;
// "Order" collides with this project's own root namespace (Order.Application, Order.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using OrderEntity = Order.Domain.Entities.Order;