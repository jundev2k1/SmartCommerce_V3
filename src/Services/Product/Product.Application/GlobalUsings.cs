global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

global using BuildingBlock.Application.Abstractions.CQRS;
global using BuildingBlock.Application.Abstractions.Persistence;

global using Product.Domain.Entities.Categories;
global using Product.Domain.Entities.Products;
global using Product.Domain.Entities.Tags;
global using Product.Domain.Enums;
global using Product.Domain.ValueObjects;
global using ProductEntity = Product.Domain.Entities.Products.Product;