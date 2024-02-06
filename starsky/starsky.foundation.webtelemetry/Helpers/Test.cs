// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Diagnostics;
// using Microsoft.Extensions.Hosting;
//
// namespace starsky.foundation.webtelemetry.Helpers;
//
// // public class Observer<T> : IObserver<T>
// // {
// //     public Observer(Action<KeyValuePair<string, object>> onNext, Action onCompleted)
// //     {
// //         _onNext = onNext ?? new Action<KeyValuePair<string, object>>(_ => { });
// //         _onCompleted = onCompleted ?? new Action(() => { });
// //     }
// //     public void OnCompleted() { _onCompleted(); }
// //     public void OnError(Exception error) { }
// //
// //     public void OnNext(KeyValuePair<string, object> value)
// //     {
// // 	    if (value.Key == RelationalEventId.ConnectionOpening.Name)
// // 	    {
// // 		    var payload = (ConnectionEventData)value.Value;
// // 		    Console.WriteLine($"EF is opening a connection to {payload.Connection.ConnectionString} ");
// // 	    }
// // 	    _onNext(value.Value as T);
// //     }
// //     private Action<T> _onNext;
// //     private Action _onCompleted;
// // }
//
// public class ProductionisingObserver : IObserver<KeyValuePair<string, object?>>
// {
// 	public void OnCompleted() {}
// 	public void OnError(Exception error) {}
//
// 	public void OnNext(KeyValuePair<string, object?> value)
// 	{
// 		if(value.Key == "HostBuilding")
// 		{
// 			var hostBuilder = (HostBuilder)value.Value;
// 			hostBuilder.UseEnvironment("Production");
// 		}
// 	}
// }
//
//
// public class MyListener
// {
// 	
// 	DbLoggerCategory.Database.Command
//     IDisposable networkSubscription;
//     IDisposable listenerSubscription;
//     private readonly object allListeners = new();
//     
//     public void Listening()
//     {
//         Action<KeyValuePair<string, object>> whenHeard = delegate (KeyValuePair<string, object> data)
//         {
//             Console.WriteLine($"Data received: {data.Key}: {data.Value}");
//         };
//         Action<DiagnosticListener> onNewListener = delegate (DiagnosticListener listener)
//         {
//             Console.WriteLine($"New Listener discovered: {listener.Name}");
//             //Subscribe to the specific DiagnosticListener of interest.
//             if (listener.Name == "System.Net.Http")
//             {
//                 //Use lock to ensure the callback code is thread safe.
//                 lock (allListeners)
//                 {
//                     if (networkSubscription != null)
//                     {
//                         networkSubscription.Dispose();
//                     }
//                     IObserver<KeyValuePair<string, object>> iobserver = 
// 	                    new ProductionisingObserver<KeyValuePair<string, object>>(whenHeard, null);
//                     networkSubscription = listener.Subscribe(iobserver);
//                 }
//
//             }
//         };
//         //Subscribe to discover all DiagnosticListeners
//         var observer = new ProductionisingObserver<KeyValuePair<string, object>>(onNewListener, null);
//         //When a listener is created, invoke the onNext function which calls the delegate.
//         listenerSubscription = DiagnosticListener.AllListeners.Subscribe(observer);
//     }
//     // Typically you leave the listenerSubscription subscription active forever.
//     // However when you no longer want your callback to be called, you can
//     // call listenerSubscription.Dispose() to cancel your subscription to the IObservable.
// }


