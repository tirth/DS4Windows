﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DS4Windows
{
    [System.ComponentModel.DesignerCategory("")]
    public class AdvancedColorDialog : ColorDialog
    {
        #region WinAPI
        internal class ApiWindow
        {
            public IntPtr hWnd;
            public string ClassName;
            public string MainWindowTitle;
        }
        internal class WindowsEnumerator
        {
            private delegate int EnumCallBackDelegate(IntPtr hwnd, int lParam);
            [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

            private static extern int EnumWindows(EnumCallBackDelegate lpEnumFunc, int lParam);
            [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

            private static extern int EnumChildWindows(IntPtr hWndParent, EnumCallBackDelegate lpEnumFunc, int lParam);
            [DllImport("user32", EntryPoint = "GetClassNameA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

            private static extern int GetClassName(IntPtr hwnd, System.Text.StringBuilder lpClassName, int nMaxCount);
            [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

            private static extern int IsWindowVisible(IntPtr hwnd);
            [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

            private static extern int GetParent(IntPtr hwnd);
            [DllImport("user32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

            private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
            [DllImport("user32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

            private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, System.Text.StringBuilder lParam);

            private List<ApiWindow> _listChildren = new List<ApiWindow>();

            private List<ApiWindow> _listTopLevel = new List<ApiWindow>();
            private string _topLevelClass = string.Empty;

            private string _childClass = string.Empty;
            public ApiWindow[] GetTopLevelWindows()
            {
                EnumWindows(EnumWindowProc, 0x0);
                return _listTopLevel.ToArray();
            }

            public ApiWindow[] GetTopLevelWindows(string className)
            {
                _topLevelClass = className;
                return GetTopLevelWindows();
            }

            public ApiWindow[] GetChildWindows(IntPtr hwnd)
            {
                _listChildren.Clear();
                EnumChildWindows(hwnd, EnumChildWindowProc, 0x0);
                return _listChildren.ToArray();
            }

            public ApiWindow[] GetChildWindows(IntPtr hwnd, string childClass)
            {
                _childClass = childClass;
                return GetChildWindows(hwnd);
            }

            private int EnumWindowProc(IntPtr hwnd, int lParam)
            {
                if (GetParent(hwnd) == 0 && IsWindowVisible(hwnd) != 0)
                {
                    var window = GetWindowIdentification(hwnd);
                    if (_topLevelClass.Length == 0 || window.ClassName.ToLower() == _topLevelClass.ToLower())
                    {
                        _listTopLevel.Add(window);
                    }
                }
                return 1;
            }

            private int EnumChildWindowProc(IntPtr hwnd, int lParam)
            {
                var window = GetWindowIdentification(hwnd);
                if (_childClass.Length == 0 || window.ClassName.ToLower() == _childClass.ToLower())
                {
                    _listChildren.Add(window);
                }
                return 1;
            }

            private ApiWindow GetWindowIdentification(IntPtr hwnd)
            {
                var classBuilder = new System.Text.StringBuilder(64);
                GetClassName(hwnd, classBuilder, 64);

                var window = new ApiWindow();
                window.ClassName = classBuilder.ToString();
                window.MainWindowTitle = WindowText(hwnd);
                window.hWnd = hwnd;
                return window;
            }

            public static string WindowText(IntPtr hwnd)
            {
                const int W_GETTEXT = 0xd;
                const int W_GETTEXTLENGTH = 0xe;

                var SB = new System.Text.StringBuilder();
                var length = SendMessage(hwnd, W_GETTEXTLENGTH, 0, 0);
                if (length > 0)
                {
                    SB = new System.Text.StringBuilder(length + 1);
                    SendMessage(hwnd, W_GETTEXT, SB.Capacity, SB);
                }
                return SB.ToString();
            }

        }
        #endregion
        private const int GA_ROOT = 2;
        private const int WM_CTLCOLOREDIT = 0x133;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_INITDIALOG = 0x0110;

        private List<ApiWindow> EditWindows;
        public delegate void ColorUpdateHandler(object sender, EventArgs e);
        public event ColorUpdateHandler OnUpdateColor;

        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hWnd, int gaFlags);

        // Overrides the base class hook procedure...
        //[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_INITDIALOG)
            {
                var mainWindow = GetAncestor(hWnd, GA_ROOT);
                if (!mainWindow.Equals(IntPtr.Zero))
                    EditWindows = new List<ApiWindow>(new WindowsEnumerator().GetChildWindows(mainWindow, "Edit"));
            }
            else if (msg == WM_CTLCOLOREDIT)
            {
                if (EditWindows == null)
                {
                    var mainWindow = GetAncestor(hWnd, GA_ROOT);
                    if (!mainWindow.Equals(IntPtr.Zero))
                        EditWindows = new List<ApiWindow>(new WindowsEnumerator().GetChildWindows(mainWindow, "Edit"));
                }
                if (EditWindows != null && EditWindows.Count == 6)
                {
                    byte red = 0, green = 0, blue = 0;
                    if (byte.TryParse(WindowsEnumerator.WindowText(EditWindows[3].hWnd), out red))
                        if (byte.TryParse(WindowsEnumerator.WindowText(EditWindows[4].hWnd), out green))
                            if (byte.TryParse(WindowsEnumerator.WindowText(EditWindows[5].hWnd), out blue))
                                OnUpdateColor(Color.FromArgb(red, green, blue), EventArgs.Empty);
                }
            }
            // Always call the base class hook procedure. 
            return base.HookProc(hWnd, msg, wParam, lParam);
        }
    }
}
