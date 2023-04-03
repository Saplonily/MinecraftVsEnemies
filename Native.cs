#define X64

#if GODOT_WINDOWS

using System.Runtime.InteropServices;
using System.Text;

namespace MVE;

internal static unsafe class Native
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct COMDLG_FILTERSPEC
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string pszName;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszSpec;
    };

#if DEBUG
#if X32
    internal const string NativeDllName = @"D:\Projects\MinecraftVsEnemies\Debug\NativeLib.dll";
#elif X64
    internal const string NativeDllName = @"D:\Projects\MinecraftVsEnemies\x64\Debug\NativeLib.dll";
#endif
#elif TRACE
    internal const string NativeDllName = @"NativeLib.dll";
#endif

    [DllImport(NativeDllName)]
    internal static extern int ComInit();

    [DllImport(NativeDllName)]
    internal static extern char* OpenDialog(ref COMDLG_FILTERSPEC fileTypes, int size);

    [DllImport(NativeDllName)]
    internal static extern void ComFree(void* ptr);
}

#endif