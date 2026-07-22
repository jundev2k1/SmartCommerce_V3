global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;

global using Inventory.Domain.Entities;

// "Inventory" collides with this project's own root namespace (Inventory.Persistence, Inventory.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using InventoryEntity = Inventory.Domain.Entities.Inventory;
