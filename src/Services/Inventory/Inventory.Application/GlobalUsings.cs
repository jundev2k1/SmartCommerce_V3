global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

global using BuildingBlock.Application.Abstractions.CQRS;
global using BuildingBlock.Application.Abstractions.Common;
global using BuildingBlock.Application.Abstractions.Persistence;
global using BuildingBlock.Criteria.Requests;
global using BuildingBlock.Contract;

global using Inventory.Application.Abstractions.Persistence.InventoryCounts;
global using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
global using Inventory.Application.Abstractions.Persistence.InventoryLots;
global using Inventory.Application.Abstractions.Persistence.InventoryReservations;
global using Inventory.Application.Abstractions.Persistence.InventorySerials;
global using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
global using Inventory.Domain.Entities;
global using Inventory.Domain.Entities.InventoryCounts;
global using Inventory.Domain.Entities.InventoryDocuments;
global using Inventory.Domain.Entities.InventoryLots;
global using Inventory.Domain.Entities.InventoryReservations;
global using Inventory.Domain.Entities.InventorySerials;
global using Inventory.Domain.Entities.InventoryTransactions;
global using Inventory.Domain.Entities.Inventories;
global using Inventory.Domain.Entities.Warehouses;
global using Inventory.Domain.Enums;
