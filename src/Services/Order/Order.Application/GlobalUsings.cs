global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using BuildingBlock.Application.Abstractions.CQRS;
global using BuildingBlock.Application.Abstractions.Events;
global using BuildingBlock.Application.Abstractions.Outbox;
global using BuildingBlock.Application.Abstractions.Persistence;
global using BuildingBlock.Application.Abstractions.Services;
global using BuildingBlock.Application.Exceptions;
global using BuildingBlock.Domain.Enums;
global using BuildingBlock.Domain.Exceptions;
global using BuildingBlock.SharedKernel.Constants;

global using Mapster;

global using Order.Domain.Entities.Catalogs;
global using Order.Domain.Entities.Orders;
global using Order.Domain.Enums;
global using Order.Domain.ValueObjects;
global using OrderEntity = Order.Domain.Entities.Orders.Order;