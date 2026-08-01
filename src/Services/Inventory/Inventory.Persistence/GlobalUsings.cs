global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;

global using BuildingBlock.Application.Abstractions.Common;
global using BuildingBlock.Criteria.Requests;
global using BuildingBlock.Persistence.Ef.Configurations;

global using Inventory.Domain.Entities;
global using Inventory.Domain.Entities.Inventories;
global using Inventory.Domain.Entities.InventoryTransactions;
global using Inventory.Domain.Entities.InventoryLots;
global using Inventory.Domain.Entities.InventoryReservations;
global using Inventory.Domain.Entities.InventorySerials;
global using Inventory.Domain.Entities.InventoryCounts;
global using Inventory.Domain.Entities.InventoryDocuments;
global using Inventory.Domain.Entities.Warehouses;
global using Inventory.Domain.Enums;

global using BuildingBlock.Domain.ValueObjects;
global using BuildingBlock.Persistence.Ef.DbContext;
global using BuildingBlock.Persistence.Ef.Inbox;
global using BuildingBlock.Persistence.Ef.Outbox;
