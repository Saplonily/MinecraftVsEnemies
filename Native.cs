#if DEBUG

using System.Runtime.InteropServices;

namespace MVE;

internal static unsafe class Native
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct COMDLG_FILTERSPEC
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string pszName;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszSpec;
    };

#if DEBUG && !EXPORTDEBUG
    internal const string NativeDllName = @".\x64\Debug\NativeLib.dll";
#elif TRACE
    internal const string NativeDllName = @"NativeLib.dll";
#endif

    [DllImport(NativeDllName)]
    internal static extern int ComInit();

    [DllImport(NativeDllName)]
    internal static extern char* OpenDialog(ref COMDLG_FILTERSPEC fileTypes, int size);

    [DllImport(NativeDllName)]
    internal static extern void ComFree(void* ptr);

    [DllImport(NativeDllName, CharSet = CharSet.Unicode)]
    internal static extern int NativeMessageBox(string title, string content);
}

#endif