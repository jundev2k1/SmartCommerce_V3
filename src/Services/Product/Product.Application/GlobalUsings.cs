global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

global using SmartEcommerce.BuildingBlock.Application.Abstractions.CQRS;
global using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;

global using SmartEcommerce.Product.Domain.Entities.Categories;
global using SmartEcommerce.Product.Domain.Entities.Products;
global using SmartEcommerce.Product.Domain.Entities.Tags;
global using SmartEcommerce.Product.Domain.Enums;
global using SmartEcommerce.Product.Domain.ValueObjects;
global using ProductEntity = SmartEcommerce.Product.Domain.Entities.Products.Product;