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
		catch ( Exception ex ) when ( IsTransientDbException(ex) )
		{
			// Treat transient errors as retryable. If not the last attempt, delay and retry.
			logger.LogWarning(ex,
				"[ExecuteWithRetry] transient DB error on attempt {Attempt}/{MaxAttempts}: {Message}",
				attempt, maxAttempts, ex.Message);
			if ( attempt >= maxAttempts )
			{
				return ( false, default!, delayMs );
			}

			await Task.Delay(delayMs);
			delayMs *= 2;
			// Return false to indicate the operation did not succeed. On the final attempt this
			// allows the outer loop to finish and throw the final exhausted exception.
			return ( false, default!, delayMs );
		}
	}

	internal static bool IsTransientDbException(Exception? ex)
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
