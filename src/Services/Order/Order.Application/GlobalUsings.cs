global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using SmartEcommerce.BuildingBlock.Application.Abstractions.CQRS;
global using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
global using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;
global using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;
global using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
global using SmartEcommerce.BuildingBlock.Application.Exceptions;
global using SmartEcommerce.BuildingBlock.Domain.Enums;
global using SmartEcommerce.BuildingBlock.Domain.Exceptions;
global using SmartEcommerce.BuildingBlock.Domain.ValueObjects;
global using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

global using Mapster;

global using SmartEcommerce.Order.Domain.Entities.Catalogs;
global using SmartEcommerce.Order.Domain.Entities.Orders;
global using SmartEcommerce.Order.Domain.Enums;
global using SmartEcommerce.Order.Domain.ValueObjects;
global using OrderEntity = SmartEcommerce.Order.Domain.Entities.Orders.Order;