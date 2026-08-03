global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

global using SmartEcommerce.BuildingBlock.Application.Abstractions.CQRS;
global using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
global using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;
global using SmartEcommerce.BuildingBlock.Criteria.Requests;
global using SmartEcommerce.BuildingBlock.Contract;

global using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryCounts;
global using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryDocuments;
global using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryLots;
global using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryReservations;
global using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventorySerials;
global using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryTransactions;
global using SmartEcommerce.Inventory.Domain.Entities;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryCounts;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryDocuments;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryLots;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryReservations;
global using SmartEcommerce.Inventory.Domain.Entities.InventorySerials;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryTransactions;
global using SmartEcommerce.Inventory.Domain.Entities.Inventories;
global using SmartEcommerce.Inventory.Domain.Entities.Warehouses;
global using SmartEcommerce.Inventory.Domain.Enums;
