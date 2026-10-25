// Stranded Deep Mod Settings - Windows folder selection backend (v0.5.0 candidate)
// C# 5 / .NET Framework, no Windows Forms dependency.
// IMPORTANT: Add to existing v0.4.3 host only after reviewing the current sources.
// Integration contract: TryBegin on Unity main thread; PumpCallbacks from host Update.
// Do not invoke a client setter unless Outcome == Success.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace StrandedDeepModSettings
{
    internal enum FolderPickerOutcome
    {
        Success = 0,
        Cancelled = 1,
        Error = 2
    }

    internal sealed class FolderPickerResult
    {
        internal FolderPickerOutcome Outcome;
        internal string SelectedPath;
        internal string Error;
    }

    internal static class WindowsFolderPicker
    {
        private static readonly object Sync = new object();
        private static readonly Queue<Completion> Completions = new Queue<Completion>();
        private static bool _busy;

        private sealed class Completion
        {
            internal IntPtr Owner;
            internal Action<FolderPickerResult> Callback;
            internal FolderPickerResult Result;
        }

        // Invoke from Unity's main thread. Only one system folder dialog can be open.
        internal static bool TryBegin(
            string initialDirectory,
            string dialogTitle,
            Action<FolderPickerResult> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return false;

            lock (Sync)
            {
                if (_busy)
                    return false;
                _busy = true;
            }

            // Capture the game's owner handle before starting the worker thread.
            IntPtr owner = GetGameWindow();
            try
            {
                Thread thread = new Thread(delegate()
                {
                    FolderPickerResult result;
                    try
                    {
                        result = ShowNativeDialog(owner, initialDirectory, dialogTitle);
                    }
                    catch (Exception ex)
                    {
                        result = new FolderPickerResult();
                        result.Outcome = FolderPickerOutcome.Error;
                        result.Error = ex.ToString();
                    }

                    lock (Sync)
                    {
                        Completions.Enqueue(new Completion {
                            Owner = owner,
                            Callback = callback,
                            Result = result
                        });
                    }
                });
                thread.Name = "StrandedDeep ModSettings FolderPicker";
                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                return true;
            }
            catch
            {
                lock (Sync) { _busy = false; }
                throw;
            }
        }

        // Invoke once each Update() on Unity main thread; all client callbacks run here.
        internal static void PumpCallbacks()
        {
            Completion completion = null;
            lock (Sync)
            {
                if (Completions.Count != 0)
                {
                    completion = Completions.Dequeue();
                    _busy = false;
                }
            }
            if (completion == null)
                return;

            if (completion.Owner != IntPtr.Zero)
                SetForegroundWindow(completion.Owner);

            // Caller restores EventSystem selection after this callback.
            try { completion.Callback(completion.Result); }
            catch (Exception ex) { Trace.WriteLine("FolderPicker callback failed: " + ex); }
        }

        internal static bool IsBusy
        {
            get { lock (Sync) { return _busy; } }
        }

        private static FolderPickerResult ShowNativeDialog(
            IntPtr owner,
            string initialDirectory,
            string dialogTitle)
        {
            const uint FOS_PICKFOLDERS = 0x00000020;
            const uint FOS_FORCEFILESYSTEM = 0x00000040;
            const uint FOS_PATHMUSTEXIST = 0x00000800;
            const uint SIGDN_FILESYSPATH = 0x80058000;
            const int ERROR_CANCELLED_HRESULT = unchecked((int)0x800704C7);
            Guid clsid = new Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7");
            Guid shellItemGuid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");
            object instance = null;
            IShellItem item = null;
            IShellItem originalFolder = null;
            int hrInit = CoInitializeEx(IntPtr.Zero, 0x2); // COINIT_APARTMENTTHREADED
            bool uninit = hrInit == 0 || hrInit == 1;
            try
            {
                if (hrInit < 0)
                    Marshal.ThrowExceptionForHR(hrInit);

                instance = Activator.CreateInstance(Type.GetTypeFromCLSID(clsid, true));
                IFileDialog dialog = (IFileDialog)instance;
                uint options;
                dialog.GetOptions(out options);
                dialog.SetOptions(options | FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM | FOS_PATHMUSTEXIST);
                if (!String.IsNullOrEmpty(dialogTitle))
                    dialog.SetTitle(dialogTitle);

                if (!String.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory))
                {
                    try
                    {
                        SHCreateItemFromParsingName(initialDirectory, IntPtr.Zero,
                            ref shellItemGuid, out originalFolder);
                        dialog.SetFolder(originalFolder);
                    }
                    catch
                    {
                        // Invalid/unavailable initial folder must not block Browse.
                    }
                }

                int hr = dialog.Show(owner);
                if (hr == ERROR_CANCELLED_HRESULT)
                    return new FolderPickerResult { Outcome = FolderPickerOutcome.Cancelled };
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                dialog.GetResult(out item);
                IntPtr rawPath = IntPtr.Zero;
                try
                {
                    item.GetDisplayName(SIGDN_FILESYSPATH, out rawPath);
                    string path = Marshal.PtrToStringUni(rawPath);
                    if (String.IsNullOrEmpty(path))
                        throw new InvalidOperationException("Shell picker returned an empty path.");
                    return new FolderPickerResult {
                        Outcome = FolderPickerOutcome.Success,
                        SelectedPath = path
                    };
                }
                finally
                {
                    if (rawPath != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(rawPath);
                }
            }
            finally
            {
                if (item != null) Marshal.FinalReleaseComObject(item);
                if (originalFolder != null) Marshal.FinalReleaseComObject(originalFolder);
                if (instance != null) Marshal.FinalReleaseComObject(instance);
                if (uninit) CoUninitialize();
            }
        }

        private static IntPtr GetGameWindow()
        {
            IntPtr current = GetActiveWindow();
            if (current != IntPtr.Zero)
                return current;
            using (Process process = Process.GetCurrentProcess())
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                    return process.MainWindowHandle;
            }
            return IntPtr.Zero;
        }

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr reserved, uint coinit);
        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            IntPtr bindCtx,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

        // The IFileDialog method order is COM vtable order; do not reorder methods.
        [ComImport]
        [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileDialog
        {
            [PreserveSig] int Show(IntPtr parent);
            void SetFileTypes(uint count, IntPtr filterSpecs);
            void SetFileTypeIndex(uint index);
            void GetFileTypeIndex(out uint index);
            void Advise(IntPtr eventsObject, out uint cookie);
            void Unadvise(uint cookie);
            void SetOptions(uint options);
            void GetOptions(out uint options);
            void SetDefaultFolder(IShellItem folder);
            void SetFolder(IShellItem folder);
            void GetFolder(out IShellItem folder);
            void GetCurrentSelection(out IShellItem item);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
            void GetResult(out IShellItem result);
            void AddPlace(IShellItem item, int fdap);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string extension);
            void Close(int hr);
            void SetClientGuid(ref Guid guid);
            void ClearClientData();
            void SetFilter(IntPtr filter);
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(IntPtr bc, ref Guid bhid, ref Guid riid, out IntPtr objectPtr);
            void GetParent(out IShellItem parent);
            void GetDisplayName(uint sigdnName, out IntPtr name);
            void GetAttributes(uint mask, out uint attrs);
            void Compare(IShellItem other, uint hint, out int order);
        }
    }
}
