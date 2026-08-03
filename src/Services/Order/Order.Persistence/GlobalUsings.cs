global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;

global using SmartEcommerce.Order.Domain.Entities.Catalogs;
global using SmartEcommerce.Order.Domain.Entities.Orders;

// "Order" collides with this project's own root namespace (SmartEcommerce.Order.Persistence, SmartEcommerce.Order.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using OrderEntity = SmartEcommerce.Order.Domain.Entities.Orders.Order;
