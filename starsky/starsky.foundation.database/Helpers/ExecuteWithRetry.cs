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
		Exception? lastException = null;

		for ( var attempt = 1; attempt <= maxAttempts; attempt++ )
		{
			if ( scopeFactory == null )
			{
				var (success, result, delayMsOut1, ex1) =
					await ExecuteOnContext(context, operation, attempt, maxAttempts, delayMs);
				delayMs = delayMsOut1;
				if ( success )
				{
					return result;
				}

				// capture last exception if provided by the helper
				lastException = ex1 ?? lastException;

				// transient happened and we should retry
				continue;
			}

			// use a fresh scope/context per attempt
			using var scope = scopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var (scSuccess, scResult, delayMsOut2, scException) =
				await ExecuteOnContext(dbContext, operation, attempt,
					maxAttempts, delayMs);
			delayMs = delayMsOut2;
			if ( scSuccess )
			{
				return scResult;
			}

			lastException = scException ?? lastException;
		}

		// Include the last observed exception as InnerException to aid debugging.
		throw new InvalidOperationException("ExecuteWithRetryAsync exhausted retries", lastException);
	}

	private async Task<(bool success, T result, int delayMs, Exception? lastException)> ExecuteOnContext<T>(
		ApplicationDbContext dbCtx, Func<ApplicationDbContext, Task<T>> operation, int attempt,
		int maxAttempts,
		int delayMs)
	{
		try
		{
			var r = await operation(dbCtx);
			return ( true, r, delayMs, null );
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
				return ( false, default!, delayMs, ex );
			}

			await Task.Delay(delayMs);
			delayMs *= 2;
			// Return false to indicate the operation did not succeed. On the final attempt this
			// allows the outer loop to finish and throw the final exhausted exception.
			return ( false, default!, delayMs, ex );
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
