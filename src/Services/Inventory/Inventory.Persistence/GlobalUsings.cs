global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;

global using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
global using SmartEcommerce.BuildingBlock.Criteria.Requests;
global using SmartEcommerce.BuildingBlock.Persistence.Ef.Configurations;

global using SmartEcommerce.Inventory.Domain.Entities;
global using SmartEcommerce.Inventory.Domain.Entities.Inventories;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryTransactions;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryLots;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryReservations;
global using SmartEcommerce.Inventory.Domain.Entities.InventorySerials;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryCounts;
global using SmartEcommerce.Inventory.Domain.Entities.InventoryDocuments;
global using SmartEcommerce.Inventory.Domain.Entities.Warehouses;
global using SmartEcommerce.Inventory.Domain.Enums;

global using SmartEcommerce.BuildingBlock.Domain.ValueObjects;
global using SmartEcommerce.BuildingBlock.Persistence.Ef.DbContext;
global using SmartEcommerce.BuildingBlock.Persistence.Ef.Inbox;
global using SmartEcommerce.BuildingBlock.Persistence.Ef.Outbox;
