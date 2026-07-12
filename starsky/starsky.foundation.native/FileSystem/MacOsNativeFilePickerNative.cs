using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using starsky.foundation.native.FileSystem.Interfaces;

namespace starsky.foundation.native.FileSystem;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Interoperability",
	"SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to " +
	"generate P/Invoke marshalling code at compile time")]
internal sealed class MacOsNativeFilePickerNative : IMacOsNativeFilePickerNative
{
	private delegate void DispatchFunctionT(IntPtr context);

	private const string FoundationFramework =
		"/System/Library/Frameworks/Foundation.framework/Foundation";

	private const string ObjcFramework = "/usr/lib/libobjc.A.dylib";

	private const string AppKitFramework =
		"/System/Library/Frameworks/AppKit.framework/AppKit";

	private const nint NsModalResponseOk = 1;
	private const nuint NsUrlBookmarkCreationWithSecurityScope = 1u << 10;

	private static bool _appKitLoaded;
	private static IntPtr _lastPanelResult; // Thread-local storage for callback result
	private static DispatchFunctionT? _dispatchCallback;
	private static string? _lastErrorMessage;

	public IntPtr CreateOpenPanel()
	{
		if ( !_appKitLoaded )
		{
			try
			{
				_ = NativeLibrary.Load(AppKitFramework);
				_appKitLoaded = true;
				Debug.WriteLine("AppKit framework loaded successfully");
			}
			catch ( Exception ex )
			{
				Debug.WriteLine($"Failed to load AppKit: {ex.Message}");
				return IntPtr.Zero;
			}
		}

		_lastPanelResult = IntPtr.Zero;

		try
		{
			Debug.WriteLine("Attempting to get main queue via dispatch_get_main_queue()");
			var mainQueue = dispatch_get_main_queue();
			if ( mainQueue == IntPtr.Zero )
			{
				_lastErrorMessage = "dispatch_get_main_queue returned null";
				Debug.WriteLine(_lastErrorMessage);
				return IntPtr.Zero;
			}
			Debug.WriteLine($"Got main queue: {mainQueue:X}");

			// Create a static callback that will be invoked on main thread
			_dispatchCallback = CreatePanelCallback;
			Debug.WriteLine("Dispatching NSOpenPanel creation to main queue");
			dispatch_sync_f(mainQueue, IntPtr.Zero, _dispatchCallback);
			Debug.WriteLine($"Dispatch completed. Panel result: {_lastPanelResult:X}");
			if ( _lastPanelResult == IntPtr.Zero )
			{
				_lastErrorMessage ??= "NSOpenPanel creation returned null (unknown reason)";
			}
			else
			{
				_lastErrorMessage = null;
			}
			return _lastPanelResult;
		}
		catch ( SEHException ex )
		{
			// Catch unmanaged Objective-C exceptions
			Debug.WriteLine($"NSOpenPanel creation failed with SEH: {ex.Message}");
			return IntPtr.Zero;
		}
		catch ( Exception ex )
		{
			Debug.WriteLine($"NSOpenPanel creation failed: {ex.GetType().Name}: {ex.Message}");
			return IntPtr.Zero;
		}
	}

	private static void CreatePanelCallback(IntPtr context)
	{
		Debug.WriteLine("CreatePanelCallback invoked on main queue");
		var pool = objc_autoreleasePoolPush();
		try
		{
			Debug.WriteLine("Getting NSOpenPanel class");
			// Ensure NSApplication is initialized - some contexts (services) need an explicit init
			var appClass = objc_getClass("NSApplication");
			if ( appClass != IntPtr.Zero )
			{
				try
				{
					var sharedSel = GetSelectorInternal("sharedApplication");
					var sharedApp = objc_msgSend_retIntPtr(appClass, sharedSel);
					if ( sharedApp == IntPtr.Zero )
					{
						Debug.WriteLine("NSApplication.sharedApplication returned null, attempting alloc/init");
						var allocSel = GetSelectorInternal("alloc");
						var initSel = GetSelectorInternal("init");
						var appAlloc = objc_msgSend_retIntPtr(appClass, allocSel);
						if ( appAlloc != IntPtr.Zero )
						{
							var appInit = objc_msgSend_retIntPtr(appAlloc, initSel);
							if ( appInit != IntPtr.Zero )
							{
								Debug.WriteLine("Initialized NSApplication instance via alloc/init");
							}
							else
							{
								Debug.WriteLine("Failed to init NSApplication via alloc/init");
							}
						}
					}
					else
					{
						Debug.WriteLine("NSApplication.sharedApplication already exists");
					}
				}
				catch ( Exception ex )
				{
					Debug.WriteLine($"NSApplication init attempt threw: {ex.Message}");
				}
			}

			var panelClass = objc_getClass("NSOpenPanel");
			if ( panelClass == IntPtr.Zero )
			{
				_lastErrorMessage = "objc_getClass(\"NSOpenPanel\") returned null";
				Debug.WriteLine(_lastErrorMessage);
				_lastPanelResult = IntPtr.Zero;
				return;
			}
			Debug.WriteLine($"Got NSOpenPanel class: {panelClass:X}");

			Debug.WriteLine("Registering openPanel selector");
			var openPanelSelector = GetSelectorInternal("openPanel");
			if ( openPanelSelector == IntPtr.Zero )
			{
				_lastErrorMessage = "sel_registerName(\"openPanel\") returned null";
				Debug.WriteLine(_lastErrorMessage);
				_lastPanelResult = IntPtr.Zero;
				return;
			}
			Debug.WriteLine($"Got openPanel selector: {openPanelSelector:X}");

			Debug.WriteLine("Calling objc_msgSend for openPanel");
			_lastPanelResult = objc_msgSend_retIntPtr(panelClass, openPanelSelector);
			if ( _lastPanelResult == IntPtr.Zero )
			{
				_lastErrorMessage = "objc_msgSend returned null for openPanel";
				Debug.WriteLine(_lastErrorMessage);
			}
			else
			{
				_lastErrorMessage = null;
				Debug.WriteLine($"Successfully created NSOpenPanel: {_lastPanelResult:X}");
			}
		}
		catch ( Exception ex )
		{
			Debug.WriteLine($"CreatePanelCallback exception: {ex.GetType().Name}: {ex.Message}");
			_lastPanelResult = IntPtr.Zero;
		}
		finally
		{
			if ( pool != IntPtr.Zero )
			{
				objc_autoreleasePoolPop(pool);
			}
		}
	}

	public string? GetLastNativeError()
	{
		return _lastErrorMessage;
	}

	public void ConfigureOpenPanel(IntPtr panel, bool includeFiles = false)
	{
		objc_msgSend_retVoid_Bool(panel, GetSelectorInternal("setCanChooseFiles:"), includeFiles);
		objc_msgSend_retVoid_Bool(panel, GetSelectorInternal("setCanChooseDirectories:"), true);
		objc_msgSend_retVoid_Bool(panel, GetSelectorInternal("setAllowsMultipleSelection:"), false);
	}

	public bool RunModal(IntPtr panel)
	{
		if ( pthread_main_np() == 0 )
		{
			return false;
		}

		var result = objc_msgSend_retNInt(panel, GetSelectorInternal("runModal"));
		return result == NsModalResponseOk;
	}

	public IntPtr GetSelectedUrl(IntPtr panel)
	{
		return objc_msgSend_retIntPtr(panel, GetSelectorInternal("URL"));
	}

	public string GetPath(IntPtr url)
	{
		var pathCfStr = objc_msgSend_retIntPtr(url, GetSelectorInternal("path"));
		return CfStringToStringInternal(pathCfStr);
	}

	public IntPtr CreateBookmarkData(IntPtr url)
	{
		return objc_msgSend_retIntPtr_NUInt_IntPtr_IntPtr_IntPtr(
			url,
			GetSelectorInternal(
				"bookmarkDataWithOptions:includingResourceValuesForKeys:relativeToURL:error:"),
			NsUrlBookmarkCreationWithSecurityScope,
			IntPtr.Zero,
			IntPtr.Zero,
			IntPtr.Zero);
	}

	public byte[] NsDataGetBytes(IntPtr nsData)
	{
		var lengthIntPtr = objc_msgSend_retIntPtr(nsData, GetSelectorInternal("length"));
		var length = ( int ) lengthIntPtr.ToInt64();
		if ( length <= 0 )
		{
			return [];
		}

		var bytesPtr = objc_msgSend_retIntPtr(nsData, GetSelectorInternal("bytes"));
		var result = new byte[length];
		Marshal.Copy(bytesPtr, result, 0, length);
		return result;
	}

	private static string CfStringToStringInternal(IntPtr cfStr)
	{
		if ( cfStr == IntPtr.Zero )
		{
			return string.Empty;
		}

		var buf = new StringBuilder(1024);
		var ok = CFStringGetCString(cfStr, buf, buf.Capacity, 0x08000100);
		return ok ? buf.ToString() : string.Empty;
	}

	private static IntPtr GetSelectorInternal(string name)
	{
		return sel_registerName(name);
	}

	[DllImport(FoundationFramework)]
	private static extern bool CFStringGetCString(IntPtr theString, StringBuilder buffer,
		long bufferSize, uint encoding);

	[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
	[DllImport(ObjcFramework, CharSet = CharSet.Ansi)]
	private static extern IntPtr objc_getClass(string name);

	[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
	[DllImport(ObjcFramework, CharSet = CharSet.Ansi)]
	private static extern IntPtr sel_registerName(string name);

	[DllImport(ObjcFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr(IntPtr target, IntPtr selector);

	[DllImport(ObjcFramework, EntryPoint = "objc_msgSend")]
	private static extern nint objc_msgSend_retNInt(IntPtr target, IntPtr selector);

	[DllImport(ObjcFramework, EntryPoint = "objc_msgSend")]
	private static extern void objc_msgSend_retVoid_Bool(IntPtr target, IntPtr selector,
		[MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport(ObjcFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr_NUInt_IntPtr_IntPtr_IntPtr(IntPtr target,
		IntPtr selector,
		nuint param1, IntPtr param2, IntPtr param3, IntPtr param4);

	[DllImport("/usr/lib/libSystem.dylib")]
	private static extern int pthread_main_np();

	[DllImport("/usr/lib/libobjc.A.dylib")]
	private static extern IntPtr objc_autoreleasePoolPush();

	[DllImport("/usr/lib/libobjc.A.dylib")]
	private static extern void objc_autoreleasePoolPop(IntPtr pool);

	[DllImport("/usr/lib/libSystem.dylib")]
	private static extern IntPtr dispatch_get_main_queue();

	[DllImport("/usr/lib/libSystem.dylib")]
	private static extern void dispatch_sync_f(IntPtr queue, IntPtr context,
		[MarshalAs(UnmanagedType.FunctionPtr)] DispatchFunctionT callback);
}
