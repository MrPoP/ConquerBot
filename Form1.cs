using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConquerBot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string EXEName = "";
        private string DLLName = "";
        private Thread mThread;
        public bool ProcessAllocated = false;
        private byte[] Buffers;
        private void Form1_Load(object sender, EventArgs e)
        {
            mThread = new Thread(Processs);
            mThread.IsBackground = true;
            mThread.Start();
        }
        private void Processs()
        {
            while (true)
            {
                try
                {
                    Process[] Procs = Process.GetProcessesByName(EXEName, Environment.MachineName);
                    if (Procs.Length > 0)
                    {
                        var LocProcess = Procs[0];
                        Program.RootPath = LocProcess.MainModule.FileName;
                        #region Getting CryptoKey
                        Buffers = File.ReadAllBytes(Program.RootPath);
                        for (uint i = 0; i < this.Buffers.Length; i++)
                        {
                            if (((i + 8) <= this.Buffers.Length) && (Encoding.ASCII.GetString(this.Buffers, (int)i, 8) == "TQServer"))
                            {
                                Program.CryptoGraphyKey = Encoding.ASCII.GetString(this.Buffers, (int)((i + 8) + 4), 0x10);
                                break;
                            }
                        }
                        #endregion
                        #region InjectingDLL
                        if (InjectDLL(LocProcess.MainWindowHandle, DLLName))
                        {
                            label1.Text = "Process Allocated Successfully.";
                            Program.RootPath.Remove(Program.RootPath.Length - (EXEName.Length + 1));
                            ProcessAllocated = true;
                            this.Hide();
                            Application.Run(new Connecting());
                        }
                        #endregion
                        else
                        {
                            throw new Exception("Can't Inject.");
                        }
                    }
                    else
                    {
                        Thread.Sleep(2000);
                    }
                }
                catch { MessageBox.Show("Error 0x0002."); label1.Text = "Failed."; mThread.Abort(); Environment.Exit(0); }
            }
        }
        #region CON
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        internal static extern int WaitForSingleObject(IntPtr handle, int milliseconds);
        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);
        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, string lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);
        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public static class VAE_Enums
        {
            public enum AllocationType
            {
                MEM_COMMIT = 0x1000,
                MEM_RESERVE = 0x2000,
                MEM_RESET = 0x80000
            }

            public enum ProtectionConstants
            {
                PAGE_EXECUTE = 0x10,
                PAGE_EXECUTE_READ = 0x20,
                PAGE_EXECUTE_READWRITE = 0x40,
                PAGE_EXECUTE_WRITECOPY = 0x80,
                PAGE_NOACCESS = 1
            }
        }
        #endregion
        public static bool InjectDLL(IntPtr hProcess, string strDLLName)
        {
            IntPtr ptr;
            string str;
            int num = strDLLName.Length + 1;
            IntPtr lpBaseAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)num, 0x1000, 0x40);
            if ((lpBaseAddress == IntPtr.Zero) && (lpBaseAddress == IntPtr.Zero))
            {
                str = "Unable to allocate memory to target process.\n";
                str = str + "Error code: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                return false;
            }
            WriteProcessMemory(hProcess, lpBaseAddress, strDLLName, (UIntPtr)num, out ptr);
            if (System.Runtime.InteropServices.Marshal.GetLastWin32Error() != 0x514)
            {
                str = "Please run it as an administrator";
                str = str + "Error code: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            }
            else if (System.Runtime.InteropServices.Marshal.GetLastWin32Error() != 0)
            {
                str = "Unable to write memory to process.";
                str = str + "Error code: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                return false;
            }
            UIntPtr procAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (procAddress == ((UIntPtr)0))
            {
                str = "Unable to find address of \"LoadLibraryA\".\n";
                MessageBox.Show(str + "Error code: " + Marshal.GetLastWin32Error());
                return false;
            }
            IntPtr handle = CreateRemoteThread(hProcess, IntPtr.Zero, 0, procAddress, lpBaseAddress, 0, out ptr);
            if (handle == IntPtr.Zero)
            {
                str = "Unable to load dll into memory.";
                str = str + "Error code: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                return false;
            }
            long num2 = WaitForSingleObject(handle, 0x2710);
            switch (num2)
            {
                case 0x80L:
                case 0x102L:
                case 0xffffffffL:
                    CloseHandle(handle);
                    return false;
            }
            Thread.Sleep(0x3e8);
            VirtualFreeEx(hProcess, lpBaseAddress, (UIntPtr)0, 0x8000);
            CloseHandle(handle);
            return true;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
