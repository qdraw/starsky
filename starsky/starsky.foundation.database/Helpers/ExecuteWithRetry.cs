using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Helpers;

public class ExecuteWithRetry(
	ApplicationDbContext context,
	IServiceScopeFactory? scopeFactory,
	IWebLogger logger)
{
	public async Task<T> ExecuteWithRetryAsync<T>(Func<ApplicationDbContext, Task<T>> operation)
	{
		const int maxAttempts = 3;
		var delayMs = 100; // initial backoff

		for ( var attempt = 1; attempt <= maxAttempts; attempt++ )
		{
			if ( scopeFactory == null )
			{
				var (success, result, delayMsOut1) =
					await ExecuteOnContext(context, operation, attempt, maxAttempts, delayMs);
				delayMs = delayMsOut1;
				if ( success )
				{
					return result;
				}

				// transient happened and we should retry
				continue;
			}

			// use a fresh scope/context per attempt
			using var scope = scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var (scSuccess, scResult, delayMsOut2) =
				await ExecuteOnContext(dbContext, operation, attempt,
					maxAttempts, delayMs);
			delayMs = delayMsOut2;
			if ( scSuccess )
			{
				return scResult;
			}

			// else transient and retry
		}

		throw new InvalidOperationException("ExecuteWithRetryAsync exhausted retries");
	}

	private async Task<(bool success, T result, int delayMs)> ExecuteOnContext<T>(
		ApplicationDbContext dbCtx, Func<ApplicationDbContext, Task<T>> operation, int attempt,
		int maxAttempts,
		int delayMs)
	{
		try
		{
			var r = await operation(dbCtx);
			return ( true, r, delayMs );
		}
		catch ( ObjectDisposedException )
		{
			throw;
		}
		catch ( Exception ex ) when ( IsTransientDbException(ex) && attempt < maxAttempts )
		{
			logger.LogWarning(ex,
				"[ThumbnailQuery] transient DB error on attempt {Attempt}/{MaxAttempts}: {Message}",
				attempt, maxAttempts, ex.Message);
			await Task.Delay(delayMs);
			delayMs *= 2;
			return ( false, default!, delayMs );
		}
	}

	private static bool IsTransientDbException(Exception? ex)
	{
		switch ( ex )
		{
			// Treat InvalidOperationException, MySqlConnector NullReferenceException, MySqlException as transient
			case null:
				return false;
			case InvalidOperationException:
			case MySqlException:
			// It may come from MySqlConnector internals; treat as transient
			case NullReferenceException:
				return true;
		}

		// check inner exceptions chain
		var inner = ex.InnerException;
		while ( inner != null )
		{
			if ( inner is MySqlException )
			{
				return true;
			}

			inner = inner.InnerException;
		}

		return false;
	}
}
