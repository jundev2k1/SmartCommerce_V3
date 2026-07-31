global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;

global using Order.Domain.Entities.Catalogs;
global using Order.Domain.Entities.Orders;

// "Order" collides with this project's own root namespace (Order.Persistence, Order.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using OrderEntity = Order.Domain.Entities.Orders.Order;
