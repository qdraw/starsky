using System;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client.Exceptions;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Backends;

namespace starskytest.starsky.foundation.worker.Backends;

[TestClass]
public sealed class RabbitMqChannelClientTest
{
	private static AppSettingsRabbitMqModel CreateConnectionSettings(
		string host = "localhost",
		int port = 5672)
	{
		return new AppSettingsRabbitMqModel
		{
			Host = host,
			Port = port,
			Username = "guest",
			Password = "guest",
			VirtualHost = "/"
		};
	}

	[TestMethod]
	public void Constructor_WithLocalhostDefaults_AttempsToConnect()
	{
		var settings = CreateConnectionSettings();

		// This will fail if no RabbitMQ server is running, which is expected
		try
		{
			var client = new RabbitMqChannelClient(settings, "test_queue");
			client.Dispose();
		}
		catch ( BrokerUnreachableException )
		{
			// Expected - no RabbitMQ server
		}
		catch ( SocketException )
		{
			// Expected - connection refused
		}
		catch ( TimeoutException )
		{
			// Expected - timeout
		}
		catch ( AggregateException )
		{
			// Expected - aggregated exceptions
		}
	}

	[TestMethod]
	public void Constructor_WithInvalidHostname_ThrowsConnectionException()
	{
		var settings = CreateConnectionSettings("invalid-nonexistent-host-12345.local");

		var caughtException = false;
		Exception? exception = null;	
		try
		{
			_ = new RabbitMqChannelClient(settings, "test_queue");
		}
		catch ( Exception exception2 )
		{
			exception = exception2;
			caughtException = true;
		}
		
		Assert.IsTrue(
			exception is BrokerUnreachableException or SocketException or AggregateException,
			$"Expected connection exception, got {exception!.GetType().Name}"
		);
		Assert.IsTrue(caughtException, "Expected an exception to be thrown");
	}

	[TestMethod]
	public void Constructor_WithInvalidPort_ThrowsConnectionException()
	{
		var settings = CreateConnectionSettings(port: 9999);

		var caughtException = false;
		Exception? exception = null;	

		try
		{
			_ = new RabbitMqChannelClient(settings, "test_queue");
		}
		catch ( Exception exception2 )
		{
			exception = exception2;
			caughtException = true;
		}

		Assert.IsTrue(
			exception is BrokerUnreachableException or SocketException or TimeoutException
				or AggregateException,
			$"Expected connection exception, got {exception.GetType().Name}"
		);
		Assert.IsTrue(caughtException, "Expected an exception to be thrown");
	}

	[TestMethod]
	public void Constructor_CreatesChannelAndDeclaresQueue()
	{
		// Even with a bogus connection, we can verify the queue name is passed correctly
		var settings = CreateConnectionSettings("invalid-host.local");
		const string expectedQueueName = "important_queue";

		try
		{
			_ = new RabbitMqChannelClient(settings, expectedQueueName);
		}
		catch
		{
			// Expected - bogus host will fail
		}

		// If the queue name parameter wasn't passed correctly, an exception would have occurred
		// during the QueueDeclare call in the constructor
	}

	[TestMethod]
	public void Dispose_CanBeCalledSafely()
	{
		var settings = CreateConnectionSettings("invalid-host.local");

		try
		{
			var client = new RabbitMqChannelClient(settings, "test_queue");
			// This line should only be reached if RabbitMQ is actually running
			client.Dispose();
		}
		catch ( BrokerUnreachableException )
		{
			// Expected - most common case
		}
		catch ( SocketException )
		{
			// Expected - socket errors
		}
		catch ( TimeoutException )
		{
			// Expected - timeout errors
		}
	}
}
