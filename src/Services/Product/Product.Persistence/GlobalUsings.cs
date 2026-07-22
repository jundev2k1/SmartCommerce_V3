global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;

global using Product.Domain.Entities;

// "Product" collides with this project's own root namespace (Product.Persistence, Product.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using ProductEntity = Product.Domain.Entities.Product;
