using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Backends;
using starsky.foundation.worker.Models;
using starsky.foundation.worker.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.Services;

[TestClass]
public sealed class QueueBackendFactoryTest
{
	private static QueueBackendFactory CreateFactory(AppSettings appSettings)
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
		var provider = services.BuildServiceProvider();
		var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
		return new QueueBackendFactory(scopeFactory, new FakeIWebLogger(), appSettings);
	}

	[TestMethod]
	public void Create_EmptyQueueName_ThrowsArgumentException()
	{
		var factory = CreateFactory(new AppSettings());
		Assert.ThrowsExactly<ArgumentException>(() => factory.Create(string.Empty));
	}

	[TestMethod]
	public void Create_DefaultBackend_InMemory()
	{
		var appSettings = new AppSettings();
		appSettings.Queue.Default = QueueBackendType.InMemory;
		var factory = CreateFactory(appSettings);

		var backend = factory.Create(QueueNames.Update);
		Assert.IsInstanceOfType<InMemoryQueueBackend>(backend);
	}

	[TestMethod]
	public void Create_PerQueueOverride_Database()
	{
		var appSettings = new AppSettings();
		appSettings.Queue.Default = QueueBackendType.InMemory;
		appSettings.Queue.Queues[QueueNames.Update] = QueueBackendType.Database;
		var factory = CreateFactory(appSettings);

		var backend = factory.Create(QueueNames.Update);
		Assert.IsInstanceOfType<DatabaseQueueBackend>(backend);
	}

	[TestMethod]
	public void Create_PerQueueOverride_RabbitMq()
	{
		var appSettings = new AppSettings();
		appSettings.Queue.Default = QueueBackendType.InMemory;
		appSettings.Queue.Queues[QueueNames.Update] = QueueBackendType.RabbitMq;
		var factory = CreateFactory(appSettings);

		var backend = factory.Create(QueueNames.Update);
		Assert.IsInstanceOfType<RabbitMqQueueBackend>(backend);
	}

	[TestMethod]
	public void Create_SameQueueName_ReturnsCachedBackendInstance()
	{
		var appSettings = new AppSettings();
		var factory = CreateFactory(appSettings);

		var backend1 = factory.Create(QueueNames.Update);
		var backend2 = factory.Create(QueueNames.Update);

		Assert.AreSame(backend1, backend2);
	}
}


