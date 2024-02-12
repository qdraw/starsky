// using System.Runtime.InteropServices;
//
// namespace starsky.foundation.native.Trash.Helpers;
//
// public class MacOsLaunch
// {
// 	private const string FoundationFramework = "/System/Library/Frameworks/Foundation.framework/Foundation";
// 	private const string AppKitFramework = "/System/Library/Frameworks/AppKit.framework/AppKit";
// 	
// 	[DllImport(AppKitFramework)]
// 	private static extern IntPtr NSSelectorFromString(IntPtr cfstr);
// 	
// 	[DllImport(FoundationFramework)]
// 	private static extern void CFRelease(IntPtr handle);
// 	
// 	public class ApplicationStartInfo
// 	{
// 		public ApplicationStartInfo (string application)
// 		{
// 			this.Application = application;
// 			this.Environment = new Dictionary<string, string> ();
// 		}
// 		
// 		public string Application { get; set; }
// 		public Dictionary<string,string> Environment { get; private set; }
// 		public string[] Args { get; set; }
// 		public bool Async { get; set; }
// 		public bool NewInstance { get; set; }
// 	}
//
// 	
// 	public static class LaunchServices
// 	{
// 		public static int OpenApplication (string application)
// 		{
// 			return OpenApplication (new ApplicationStartInfo (application));
// 		}
//
// 		internal static IntPtr GetSelector(string name)
// 		{
// 			var cfStrSelector = MacOsTrashBindingHelper.CreateCfString(name);
// 			var selector = NSSelectorFromString(cfStrSelector);
// 			CFRelease(cfStrSelector);
// 			return selector;
// 		}
// 		
// 		// This function can be replaced by NSWorkspace.LaunchApplication but it currently doesn't work
// 		// https://bugzilla.xamarin.com/show_bug.cgi?id=32540
//
// 		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
// 		static extern IntPtr IntPtr_objc_msgSend_IntPtr_UInt32_IntPtr_IntPtr (IntPtr receiver, IntPtr selector, IntPtr url, UInt32 options, IntPtr configuration, out IntPtr error);
// 		static readonly IntPtr launchApplicationAtURLOptionsConfigurationErrorSelector = ObjCRuntime.Selector.GetHandle ("launchApplicationAtURL:options:configuration:error:");
// 		public static int OpenApplication (ApplicationStartInfo application)
// 		{
// 			if (application == null)
// 				throw new ArgumentNullException ("application");
//
// 			if (string.IsNullOrEmpty (application.Application) || !Directory.Exists (application.Application))
// 				throw new ArgumentException ("Application is not valid");
//
// 			NSUrl appUrl = NSUrl.FromFilename (application.Application);
//
// 			// TODO: Once the above bug is fixed, we can replace the code below with
// 			//NSRunningApplication app = NSWorkspace.SharedWorkspace.LaunchApplication (appUrl, 0, new NSDictionary (), null);
//
// 			var config = new NSMutableDictionary ();
// 			if (application.Args != null && application.Args.Length > 0) {
// 				var args = new NSMutableArray ();
// 				foreach (string arg in application.Args) {
// 					args.Add (new NSString (arg));
// 				}
// 				config.Add (new NSString ("NSWorkspaceLaunchConfigurationArguments"), args);
// 			}
//
// 			if (application.Environment != null && application.Environment.Count > 0) {
// 				var envValueStrings = application.Environment.Values.Select (t => new NSString (t)).ToArray ();
// 				var envKeyStrings = application.Environment.Keys.Select (t => new NSString (t)).ToArray ();
//
// 				var envDict = new NSMutableDictionary ();
// 				for (int i = 0; i < envValueStrings.Length; i++) {
// 					envDict.Add (envKeyStrings[i], envValueStrings[i]);
// 				}
//
// 				config.Add (new NSString ("NSWorkspaceLaunchConfigurationEnvironment"), envDict);
// 			}
//
// 			UInt32 options = 0;
//
// 			if (application.Async)
// 				options |= (UInt32) LaunchOptions.NSWorkspaceLaunchAsync;
// 			if (application.NewInstance)
// 				options |= (UInt32) LaunchOptions.NSWorkspaceLaunchNewInstance;
//
// 			IntPtr error;
// 			var appHandle = IntPtr_objc_msgSend_IntPtr_UInt32_IntPtr_IntPtr (NSWorkspace.SharedWorkspace.Handle, launchApplicationAtURLOptionsConfigurationErrorSelector, appUrl.Handle, options, config.Handle, out error);
// 			if (appHandle == IntPtr.Zero)
// 				return -1;
//
// 			NSRunningApplication app = (NSRunningApplication)ObjCRuntime.Runtime.GetNSObject (appHandle);
//
// 			return app.ProcessIdentifier;
// 		}
//
// 		[Flags]
// 		enum LaunchOptions {
// 			NSWorkspaceLaunchAndPrint = 0x00000002,
// 			NSWorkspaceLaunchWithErrorPresentation = 0x00000040,
// 			NSWorkspaceLaunchInhibitingBackgroundOnly = 0x00000080,
// 			NSWorkspaceLaunchWithoutAddingToRecents = 0x00000100,
// 			NSWorkspaceLaunchWithoutActivation = 0x00000200,
// 			NSWorkspaceLaunchAsync = 0x00010000,
// 			NSWorkspaceLaunchAllowingClassicStartup = 0x00020000,
// 			NSWorkspaceLaunchPreferringClassic = 0x00040000,
// 			NSWorkspaceLaunchNewInstance = 0x00080000,
// 			NSWorkspaceLaunchAndHide = 0x00100000,
// 			NSWorkspaceLaunchAndHideOthers = 0x00200000,
// 			NSWorkspaceLaunchDefault = NSWorkspaceLaunchAsync | NSWorkspaceLaunchAllowingClassicStartup
// 		};
// 	}
// }
