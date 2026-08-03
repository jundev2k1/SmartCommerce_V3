using SmartEcommerce.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace SmartEcommerce.BuildingBlock.Persistence.Ef.Tests;

internal sealed class TestUnitOfWork(TestDbContext context) : EfUnitOfWork<TestDbContext>(context);
