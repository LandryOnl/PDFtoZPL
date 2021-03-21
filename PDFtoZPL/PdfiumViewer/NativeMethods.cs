﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PDFtoZPL.PdfiumViewer
{
    internal static partial class NativeMethods
    {
        static NativeMethods()
        {
            // Load the platform dependent Pdfium.dll if it exists.
            LoadNativeLibrary(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
        }

        private static string? _pdfiumLibPath;

        private static bool LoadNativeLibrary(string path)
        {
            if (path == null)
                return false;

            _pdfiumLibPath = Path.Combine(path, "runtimes");

#if NETCOREAPP3_0_OR_GREATER
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, Environment.Is64BitProcess ? "win-x64" : "win-x86");
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "native");
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "pdfium.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, Environment.Is64BitProcess ? "linux-x64" : throw new NotSupportedException("Only x86-64 is supported on Linux."));
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "native");
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "pdfium.dll");
            }
            else
            {
                throw new NotSupportedException("Only win-x86, win-x64 and linux-x64 are supported.");
            } 

            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, ImportResolver);

            return File.Exists(_pdfiumLibPath) && NativeLibrary.Load(_pdfiumLibPath) != IntPtr.Zero;
#else
            _pdfiumLibPath = Path.Combine(_pdfiumLibPath, Environment.Is64BitProcess ? "win-x64" : "win-x86");
            _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "native");
            _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "pdfium.dll");

            return File.Exists(_pdfiumLibPath) && LoadLibrary(_pdfiumLibPath) != IntPtr.Zero;
#endif
        }

#if NETCOREAPP3_0_OR_GREATER
        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (_pdfiumLibPath == null || Path.GetFileName(_pdfiumLibPath) != libraryName)
                return IntPtr.Zero;

            return NativeLibrary.Load(_pdfiumLibPath);
        }
#else
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);
#endif
    }
}