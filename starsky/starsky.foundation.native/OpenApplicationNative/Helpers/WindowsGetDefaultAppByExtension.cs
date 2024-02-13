using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace starsky.foundation.native.OpenApplicationNative.Helpers;

public static class WindowsGetDefaultAppByExtension
{


	[DllImport("shlwapi.dll", CharSet = CharSet.Ansi)]
	static extern int AssocQueryStringA(AssocF flags, ASSOCSTR str, string pszAssoc, string pszExtra, StringBuilder pszOut, ref uint pcchOut);

	//[Flags]
	//public enum ASSOCF : uint
	//{
	//	INIT = 0x00000001,
	//	REMAPRUNDLL = 0x00000002,
	//	NOFIXUPS = 0x00000004,
	//	IGNOREBASECLASS = 0x00000008,
	//	INIT_IGNOREUNKNOWN = 0x00000010,
	//	INIT_FIXED_PROGID = 0x00000020,
	//	IS_PROTOCOL = 0x00000040,
	//	INIT_FOR_FILE = 0x00000080
	//}

	public enum ASSOCSTR
	{
		COMMAND = 1,
		EXECUTABLE,
		FRIENDLYDOCNAME,
		FRIENDLYAPPNAME,
		NOOPEN,
		SHELLNEWVALUE,
		DDECOMMAND,
		DDEIFEXEC,
		DDEAPPLICATION,
		DDETOPIC,
		INFOTIP,
		QUICKTIP,
		TILEINFO,
		CONTENTTYPE,
		DEFAULTICON,
		SHELLEXTENSION,
		DROPTARGET,
		DELEGATEEXECUTE,
		SUPPORTED_URI_PROTOCOLS,
		PROGID,
		APPID,
		APPPUBLISHER,
		APPICONREFERENCE,
		MAX
	}

	public static void GetDefaultApp3()
	{
		string pszAssoc = ".png"; // Example file extension
		string pszExtra = null; // Example extra parameter

		StringBuilder pszOut = new StringBuilder(1024); // Adjust buffer size accordingly
		uint pcchOut = ( uint )pszOut.Capacity;

		int result = AssocQueryStringA(AssocF.ASSOCF_INIT_FOR_FILE, ASSOCSTR.EXECUTABLE, pszAssoc, pszExtra, pszOut, ref pcchOut);

		if ( result == 0 ) // S_OK
		{
			// Handle successful execution
			Console.WriteLine("Associated executable: " + pszOut.ToString());
		}
		else if ( result == 1 ) // S_FALSE
		{
			// Handle the case when pszOut is NULL, pcchOut contains the required buffer size
			pszOut = new StringBuilder(( int )pcchOut);
			result = AssocQueryStringA(AssocF.ASSOCF_INIT_FOR_FILE, ASSOCSTR.EXECUTABLE, pszAssoc, pszExtra, pszOut, ref pcchOut);

			if ( result == 0 )
			{
				Console.WriteLine("Associated executable: " + pszOut.ToString());
			}
			else
			{
				Console.WriteLine("Error: " + result);
			}
		}
		else if ( result == unchecked(( int )0x80004003) ) // E_POINTER
		{
			// Handle the case when the buffer is too small
			Console.WriteLine("Buffer too small.");
		}
		else
		{
			// Handle other errors
			Console.WriteLine("Error: " + result);
		}
	}



[DllImport("shlwapi.dll", CharSet = CharSet.Ansi)]
	static extern int AssocQueryKeyA(AssocF flags, ASSOCKEY key, string pszAssoc, string pszExtra, out IntPtr phkeyOut);

	//[Flags]
	//public enum ASSOCF : uint
	//{
	//	INIT = 0x00000001,
	//	REMAPRUNDLL = 0x00000002,
	//	NOFIXUPS = 0x00000004,
	//	IGNOREBASECLASS = 0x00000008,
	//	INIT_IGNOREUNKNOWN = 0x00000010,
	//	INIT_FIXED_PROGID = 0x00000020,
	//	IS_PROTOCOL = 0x00000040,
	//	INIT_FOR_FILE = 0x00000080
	//}

	public enum ASSOCKEY
	{
		SHELLNEW = 1,
		SHELLEXECCLASS,
		APP,
		CLASS
	}

	public static void GetDefaultApp2()
	{
		string pszAssoc = ".png"; // Example file extension
		string pszExtra = null; // Example extra parameter

		IntPtr phkeyOut;
		int result = AssocQueryKeyA(AssocF.ASSOCF_INIT_FOR_FILE, ASSOCKEY.CLASS, pszAssoc, pszExtra, out phkeyOut);

		if ( result == 0 ) // S_OK
		{
			// Handle successful execution
			Console.WriteLine("Associated key successfully retrieved.");
		}
		else
		{
			// Handle error
			Console.WriteLine("Error: " + result);
		}
	}



[DllImport("shell32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
	static extern IntPtr FindExecutableA(string lpFile, string lpDirectory, StringBuilder lpResult);

	static string? FindApplication(string docName)
	{
		const int MAX_PATH = 260;
		StringBuilder result = new StringBuilder(MAX_PATH);

		IntPtr hInstance = FindExecutableA(docName, null, result);

		Console.WriteLine(hInstance.ToInt32());

		if ( hInstance.ToInt32() > 32 )
		{
			return result.ToString();
		}
		else
		{
			return null; // Or any specific value to indicate failure
		}
	}

	public static void GetDefaultApp()
	{
		string documentName = "C:\\testcontent\\test.txt"; // Provide the path of the document here
		string applicationPath = FindApplication(documentName);

		if ( applicationPath != null )
		{
			Console.WriteLine("Associated application: " + applicationPath);
		}
		else
		{
			Console.WriteLine("No associated application found.");
		}
	}



	[DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
	static extern uint AssocQueryStringA(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);
	/// 

	/// The main entry point for the application.
	/// 

	[STAThread]
	public static void GetDefaultApp1()
	{
		var extension = ".doc";
		Debug.WriteLine(FileExtentionInfo(AssocStr.ASSOCSTR_COMMAND, extension), "Command");
		Debug.WriteLine(FileExtentionInfo(AssocStr.ASSOCSTR_EXECUTABLE, extension), "Executable");
		//Debug.WriteLine(FileExtentionInfo(AssocStr.FriendlyAppName, extension), "FriendlyAppName");
		//Debug.WriteLine(FileExtentionInfo(AssocStr.FriendlyDocName, extension), "FriendlyDocName");
		//Debug.WriteLine(FileExtentionInfo(AssocStr.NoOpen, extension), "NoOpen");
		//Debug.WriteLine(FileExtentionInfo(AssocStr.ShellNewValue, extension), "ShellNewValue");
		//  DDEApplication: WinWord
		//DDEIfExec: Ñﻴ߾
		//  DDETopic: System
		//  Executable: C:\Program Files (x86)\Microsoft Office\Office12\WINWORD.EXE
		//  FriendlyAppName: Microsoft Office Word
		//  FriendlyDocName: Microsoft Office Word 97 - 2003 Document
	}

	public static string FileExtentionInfo(AssocStr assocStr, string doctype)
	{
		uint pcchOut = 0;
		AssocQueryStringA(AssocF.ASSOCF_VERIFY, assocStr, doctype, null, null, ref pcchOut);
		StringBuilder pszOut = new StringBuilder(( int )pcchOut);
		AssocQueryStringA(AssocF.ASSOCF_VERIFY, assocStr, doctype, null, pszOut, ref pcchOut);
		return pszOut.ToString();
	}

	//[Flags]
	//public enum AssocF
	//{
	//	Init_NoRemapCLSID = 0x1,
	//	Init_ByExeName = 0x2,
	//	Open_ByExeName = 0x2,
	//	Init_DefaultToStar = 0x4,
	//	Init_DefaultToFolder = 0x8,
	//	NoUserSettings = 0x10,
	//	NoTruncate = 0x20,
	//	Verify = 0x40,
	//	RemapRunDll = 0x80,
	//	NoFixUps = 0x100,
	//	IgnoreBaseClass = 0x200
	//}

	//public enum AssocStr
	//{
	//	Command = 1,
	//	Executable,
	//	FriendlyDocName,
	//	FriendlyAppName,
	//	NoOpen,
	//	ShellNewValue,
	//	DDECommand,
	//	DDEIfExec,
	//	DDEApplication,
	//	DDETopic
	//}


	//public static void GetDefaultApp()
	//{
	//	Console.WriteLine(AssocQueryString(AssocStr.ASSOCSTR_EXECUTABLE, ".jpg"));
	//}

	public enum AssocStr
	{
		ASSOCSTR_COMMAND = 1,
		ASSOCSTR_EXECUTABLE,
		ASSOCSTR_FRIENDLYDOCNAME,
		ASSOCSTR_FRIENDLYAPPNAME,
		ASSOCSTR_NOOPEN,
		ASSOCSTR_SHELLNEWVALUE,
		ASSOCSTR_DDECOMMAND,
		ASSOCSTR_DDEIFEXEC,
		ASSOCSTR_DDEAPPLICATION,
		ASSOCSTR_DDETOPIC,
		ASSOCSTR_INFOTIP,
		ASSOCSTR_QUICKTIP,
		ASSOCSTR_TILEINFO,
		ASSOCSTR_CONTENTTYPE,
		ASSOCSTR_DEFAULTICON,
		ASSOCSTR_SHELLEXTENSION,
		ASSOCSTR_DROPTARGET,
		ASSOCSTR_DELEGATEEXECUTE,
		ASSOCSTR_SUPPORTED_URI_PROTOCOLS,
		ASSOCSTR_MAX
	}
	public enum AssocF
	{
		ASSOCF_NONE = 0x00000000,
		ASSOCF_INIT_NOREMAPCLSID = 0x00000001,
		ASSOCF_INIT_BYEXENAME = 0x00000002,
		ASSOCF_OPEN_BYEXENAME = 0x00000002,
		ASSOCF_INIT_DEFAULTTOSTAR = 0x00000004,
		ASSOCF_INIT_DEFAULTTOFOLDER = 0x00000008,
		ASSOCF_NOUSERSETTINGS = 0x00000010,
		ASSOCF_NOTRUNCATE = 0x00000020,
		ASSOCF_VERIFY = 0x00000040,
		ASSOCF_REMAPRUNDLL = 0x00000080,
		ASSOCF_NOFIXUPS = 0x00000100,
		ASSOCF_IGNOREBASECLASS = 0x00000200,
		ASSOCF_INIT_IGNOREUNKNOWN = 0x00000400,
		ASSOCF_INIT_FIXED_PROGID = 0x00000800,
		ASSOCF_IS_PROTOCOL = 0x00001000,
		ASSOCF_INIT_FOR_FILE = 0x00002000
	}

	//[DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
	//private static extern uint AssocQueryString(
	//	AssocF flags,
	//	AssocStr str,
	//	string pszAssoc,
	//	string pszExtra,
	//	[Out] StringBuilder pszOut,
	//	ref uint pcchOut
	//);


	//static string AssocQueryString(AssocStr association, string extension)
	//{
	//	const int S_OK = 0;
	//	const int S_FALSE = 1;

	//	uint length = 5;
	//	uint ret = AssocQueryString(AssocF.ASSOCF_NONE, association, extension, null, null, ref length);
	//	if ( ret != S_FALSE )
	//	{
	//		throw new InvalidOperationException("Could not determine associated string");
	//	}

	//	var sb = new StringBuilder(( int )length);
	//	ret = AssocQueryString(AssocF.ASSOCF_NONE, association, extension, null, sb, ref length);
	//	if ( ret != S_OK )
	//	{
	//		throw new InvalidOperationException("Could not determine associated string");
	//	}

	//	return sb.ToString();

	//}
	//private static string AssocQueryString(AssocStr association, string extension)
	//{
	//	const int S_OK = 0;
	//	const int S_FALSE = 1;

	//	uint length = 0;
	//	var ret = AssocQueryString(AssocF.None, association, extension, null, null, ref length);
	//	if (ret != S_FALSE)
	//	{
	//		throw new InvalidOperationException("Could not determine associated string");
	//	}

	//	var sb = new StringBuilder((int)length); // (length-1) will probably work too as the marshaller adds null termination
	//	ret = AssocQueryString(AssocF.None, association, extension, null, sb, ref length);
	//	if (ret != S_OK)
	//	{
	//		throw new InvalidOperationException("Could not determine associated string"); 
	//	}

	//	return sb.ToString();
	//}
}
