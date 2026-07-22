using BuildingBlock.Persistence.Ef.UnitOfWork;

namespace BuildingBlock.Persistence.Ef.Tests;

internal sealed class TestUnitOfWork(TestDbContext context) : EfUnitOfWork<TestDbContext>(context);
