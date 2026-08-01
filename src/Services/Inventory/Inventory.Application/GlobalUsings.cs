global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

global using BuildingBlock.Application.Abstractions.CQRS;
global using BuildingBlock.Application.Abstractions.Persistence;

global using Inventory.Application.Abstractions.Persistence.InventoryCounts;
global using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
global using Inventory.Application.Abstractions.Persistence.InventoryLots;
global using Inventory.Application.Abstractions.Persistence.InventoryReservations;
global using Inventory.Application.Abstractions.Persistence.InventorySerials;
global using Inventory.Domain.Entities;
global using Inventory.Domain.Enums;

// "Inventory" collides with this project's own root namespace (Inventory.Application, Inventory.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using InventoryEntity = Inventory.Domain.Entities.Inventory;
