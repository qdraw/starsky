using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;

namespace starskytest.FakeMocks;

public class FakeIServiceScopeFactory : IServiceScopeFactory
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public FakeIServiceScopeFactory(string? name = null)
	{
		var dbName = name ?? nameof(FakeIServiceScopeFactory);
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(dbName));
		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	public IServiceScope CreateScope()
	{
		return _serviceScopeFactory.CreateScope();
	}
}
