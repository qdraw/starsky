using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public sealed class QueueItemTest
{
	[TestMethod]
	public void Defaults_AreExpected()
	{
		var model = new QueueItem();

		Assert.AreEqual(string.Empty, model.QueueName);
		Assert.AreEqual(string.Empty, model.JobType);
		Assert.AreEqual(QueueItemStatus.Pending, model.Status);
		Assert.IsTrue(model.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-1));
	}

	[TestMethod]
	public void SetProperties_RoundTrip()
	{
		var now = DateTime.UtcNow;
		var model = new QueueItem
		{
			Id = 1,
			QueueName = "Update",
			JobId = Guid.NewGuid(),
			JobType = "Update.v1",
			MetaData = "meta",
			TraceParentId = "trace",
			PriorityLane = 3,
			PayloadJson = "{}",
			Status = QueueItemStatus.Processing,
			CreatedAtUtc = now,
			ClaimedAtUtc = now,
			ProcessedAtUtc = now
		};

		Assert.AreEqual(1, model.Id);
		Assert.AreEqual("Update", model.QueueName);
		Assert.AreEqual("Update.v1", model.JobType);
		Assert.AreEqual("meta", model.MetaData);
		Assert.AreEqual("trace", model.TraceParentId);
		Assert.AreEqual(3, model.PriorityLane);
		Assert.AreEqual("{}", model.PayloadJson);
		Assert.AreEqual(QueueItemStatus.Processing, model.Status);
		Assert.AreEqual(now, model.CreatedAtUtc);
		Assert.AreEqual(now, model.ClaimedAtUtc);
		Assert.AreEqual(now, model.ProcessedAtUtc);
	}
}

