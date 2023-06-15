using System;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WinFormsNetCore7
{
    internal static class Program
    {
        private static NotifyIcon _notifyIcon;
        private static ContextMenuStrip _contextMenu;
        private static bool _isVisible = true;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //// To customize application configuration such as set high DPI settings or default font,
            //// see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //Application.Run(new Form1());

            // Check if the application is already running with administrative privileges
            if (!IsRunningWithAdminPrivileges())
            {
                // Restart the application with elevated privileges
                RestartWithElevatedPrivileges();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create the context menu items
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Hide", null, OnHideClick);
            _contextMenu.Items.Add("Exit", null, OnExitClick);

            // Create the notify icon
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = SystemIcons.Application;
            _notifyIcon.ContextMenuStrip = _contextMenu;
            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;

            ShowMainForm();

            Application.Run();

        }
        static bool IsRunningWithAdminPrivileges()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void RestartWithElevatedPrivileges()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Verb = "runas"; // Run the process with elevated privileges

            try
            {
                Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // User declined the elevation prompt or an error occurred
                // Handle the exception or display an error message
            }

            // Close the current instance of the application
            //Environment.Exit(0);
        }
        static void OnHideClick(object sender, EventArgs e)
        {
            // Hide the main form and show the notify icon
            MainForm mainForm = Application.OpenForms[0] as MainForm;
            mainForm.Hide();
            _isVisible = false;
        }

        static void OnExitClick(object sender, EventArgs e)
        {
            // Close the main form and exit the application
            MainForm mainForm = Application.OpenForms[0] as MainForm;
            mainForm.Close();
        }

        static void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            // Show the main form when the notify icon is double-clicked
            MainForm mainForm = Application.OpenForms[0] as MainForm;
            mainForm.Show();
            mainForm.WindowState = FormWindowState.Normal;
            _isVisible = true;
        }

        static void ShowMainForm()
        {
            // Show or create your main form here
            MainForm mainForm = new MainForm();

            if (_isVisible)
            {
                mainForm.FormClosed += OnMainFormClosed;
                mainForm.Resize += OnMainFormResize;
                mainForm.Show();
            }
            else
            {
                mainForm.Hide();
            }

            // Minimize the main form to the system tray
            mainForm.WindowState = FormWindowState.Minimized;
            mainForm.ShowInTaskbar = false;

            // Create a shortcut in the All Users Startup folder
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            string shortcutPath = Path.Combine(startupFolderPath, "YourApp.lnk");
            CreateShortcut(shortcutPath, Application.ExecutablePath);
        }

        static void CreateShortcut(string shortcutPath, string targetPath)
        {
            if (!File.Exists(shortcutPath))
            {
                // For DotNet Framework Begin
                //// Create a new shortcut
                //WshShell shell = new WshShell();
                //IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                //shortcut.TargetPath = targetPath;
                //shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                //shortcut.Description = "Your Application";
                //shortcut.Save();
                //For DotNet Framework End

                // Create a new shortcut
                var shellLink = (IShellLink)new ShellLink();

                // Set the target path and working directory
                shellLink.SetPath(targetPath);
                shellLink.SetWorkingDirectory(Path.GetDirectoryName(targetPath));

                // Save the shortcut
                var persistFile = (IPersistFile)shellLink;
                persistFile.Save(shortcutPath, false);
            }
        }

        static void OnMainFormClosed(object sender, FormClosedEventArgs e)
        {
            // When the main form is closed, exit the application
            Application.Exit();
        }

        static void OnMainFormResize(object sender, EventArgs e)
        {
            MainForm mainForm = sender as MainForm;

            if (mainForm.WindowState == FormWindowState.Minimized)
            {
                mainForm.Hide();
                _isVisible = false;
            }
        }

    }

    // Interop interfaces and classes

    [ComImport]
    [Guid("000214EE-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellLink
    {
        void GetPath([MarshalAs(UnmanagedType.LPWStr)] out string pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([MarshalAs(UnmanagedType.LPWStr)] out string pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] out string pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([MarshalAs(UnmanagedType.LPWStr)] out string pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([MarshalAs(UnmanagedType.LPWStr)] out string pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [Guid("0000010B-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        void IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    class ShellLink : IShellLink
    {
        public void GetPath(out string pszFile, int cchMaxPath, out IntPtr pfd, int fFlags) => throw new NotImplementedException();
        public void GetIDList(out IntPtr ppidl) => throw new NotImplementedException();
        public void SetIDList(IntPtr pidl) => throw new NotImplementedException();
        public void GetDescription(out string pszName, int cchMaxName) => throw new NotImplementedException();
        public void SetDescription(string pszName) => throw new NotImplementedException();
        public void GetWorkingDirectory(out string pszDir, int cchMaxPath) => throw new NotImplementedException();
        public void SetWorkingDirectory(string pszDir) => throw new NotImplementedException();
        public void GetArguments(out string pszArgs, int cchMaxPath) => throw new NotImplementedException();
        public void SetArguments(string pszArgs) => throw new NotImplementedException();
        public void GetHotkey(out short pwHotkey) => throw new NotImplementedException();
        public void SetHotkey(short wHotkey) => throw new NotImplementedException();
        public void GetShowCmd(out int piShowCmd) => throw new NotImplementedException();
        public void SetShowCmd(int iShowCmd) => throw new NotImplementedException();
        public void GetIconLocation(out string pszIconPath, int cchIconPath, out int piIcon) => throw new NotImplementedException();
        public void SetIconLocation(string pszIconPath, int iIcon) => throw new NotImplementedException();
        public void SetRelativePath(string pszPathRel, int dwReserved) => throw new NotImplementedException();
        public void Resolve(IntPtr hwnd, int fFlags) => throw new NotImplementedException();
        public void SetPath(string pszFile) => throw new NotImplementedException();
    }
}