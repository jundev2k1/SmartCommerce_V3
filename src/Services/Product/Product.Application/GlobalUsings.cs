global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

global using BuildingBlock.Application.Abstractions.CQRS;
global using BuildingBlock.Application.Abstractions.Persistence;

global using Product.Domain.Entities;
global using Product.Domain.Enums;

// "Product" collides with this project's own root namespace (Product.Application, Product.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using ProductEntity = Product.Domain.Entities.Product;
