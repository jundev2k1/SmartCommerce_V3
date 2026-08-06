global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Carter;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;

// No NovaCore.Promotion.Domain.Enums / .ValueObjects global usings yet - no entities exist
// until Phase 2 (Domain Model). Add them here the same way Payment.API does once the first
// endpoint needs them.
