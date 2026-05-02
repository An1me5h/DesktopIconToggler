using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Timers;

namespace DesktopIconToggler
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayApp());
        }
    }

    public class TrayApp : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip contextMenu;
        private bool iconsVisible = true;
        private System.Timers.Timer desktopWatcher;
        private bool wasOnDesktop = false;
        private bool transparencyEnabled = true;

        // ── User32 imports ────────────────────────────────────────────────
        [DllImport("user32.dll")] static extern IntPtr FindWindow(string cls, string win);
        [DllImport("user32.dll")] static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string cls, string win);
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int cmd);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
        [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowsProc proc, IntPtr lp);
        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hWnd, int index);
        [DllImport("user32.dll")] static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder buf, int maxCount);

        // ── Windows 10/11 undocumented taskbar transparency API ──────────
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowCompositionAttribute(IntPtr hWnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        const int ACCENT_DISABLED = 0;
        const int ACCENT_ENABLE_TRANSPARENTGRADIENT = 2;
        const int WCA_ACCENT_POLICY = 19;
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lp);

        public TrayApp()
        {
            iconsVisible = GetDesktopIconsVisible();

            // Context menu
            contextMenu = new ContextMenuStrip();

            var toggleIconsItem = new ToolStripMenuItem("Toggle Desktop Icons", null, OnToggleIconsClick);
            toggleIconsItem.Font = new Font(toggleIconsItem.Font, FontStyle.Bold);
            contextMenu.Items.Add(toggleIconsItem);

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Show Icons", null, (s, e) => SetIcons(true));
            contextMenu.Items.Add("Hide Icons", null, (s, e) => SetIcons(false));

            contextMenu.Items.Add(new ToolStripSeparator());

            var transparencyToggle = new ToolStripMenuItem("Auto Taskbar Transparency: ON", null, OnTransparencyToggle);
            transparencyToggle.Name = "transparencyToggle";
            contextMenu.Items.Add(transparencyToggle);

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, OnExitClick);

            // Tray icon
            trayIcon = new NotifyIcon()
            {
                Icon = CreateIcon(iconsVisible),
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = GetTooltipText()
            };
            trayIcon.Click += OnTrayClick;

            // Start desktop watcher — checks every 500ms
            desktopWatcher = new System.Timers.Timer(500);
            desktopWatcher.Elapsed += OnDesktopWatcherTick;
            desktopWatcher.AutoReset = true;
            desktopWatcher.Start();
        }

        // ── Tray interactions ────────────────────────────────────────────
        private void OnTrayClick(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me && me.Button == MouseButtons.Left)
                ToggleIcons();
        }

        private void OnToggleIconsClick(object sender, EventArgs e) => ToggleIcons();

        private void OnTransparencyToggle(object sender, EventArgs e)
        {
            transparencyEnabled = !transparencyEnabled;
            var item = contextMenu.Items["transparencyToggle"] as ToolStripMenuItem;
            if (item != null)
                item.Text = $"Auto Taskbar Transparency: {(transparencyEnabled ? "ON" : "OFF")}";

            if (!transparencyEnabled)
                SetTaskbarTransparency(false); // restore to normal immediately
        }

        // ── Icon toggle ──────────────────────────────────────────────────
        private void ToggleIcons()
        {
            iconsVisible = !iconsVisible;
            SetDesktopIconsVisible(iconsVisible);
            trayIcon.Icon = CreateIcon(iconsVisible);
            trayIcon.Text = GetTooltipText();
            trayIcon.ShowBalloonTip(1500, "Desktop Icons",
                iconsVisible ? "Icons are now visible" : "Icons are now hidden",
                ToolTipIcon.Info);
        }

        private void SetIcons(bool visible)
        {
            if (iconsVisible == visible) return;
            iconsVisible = visible;
            SetDesktopIconsVisible(iconsVisible);
            trayIcon.Icon = CreateIcon(iconsVisible);
            trayIcon.Text = GetTooltipText();
        }

        // ── Desktop watcher ──────────────────────────────────────────────
        private void OnDesktopWatcherTick(object sender, ElapsedEventArgs e)
        {
            if (!transparencyEnabled) return;

            bool onDesktop = IsUserOnDesktop();

            if (onDesktop != wasOnDesktop)
            {
                wasOnDesktop = onDesktop;
                SetTaskbarTransparency(onDesktop);
            }
        }

        private bool IsUserOnDesktop()
        {
            bool anyWindowOpen = false;

            EnumWindows((hWnd, lp) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                // Skip tool windows (no taskbar button)
                int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                if ((exStyle & WS_EX_TOOLWINDOW) != 0) return true;

                // Skip known shell/system window classes
                var cls = new System.Text.StringBuilder(256);
                GetClassName(hWnd, cls, 256);
                string cn = cls.ToString();
                if (cn == "Progman" || cn == "WorkerW" || cn == "Shell_TrayWnd" ||
                    cn == "Shell_SecondaryTrayWnd" || cn == "DV2ControlHost" ||
                    cn == "MsgrIMEWindowClass" || cn == "SysShadow" || cn == "Button" ||
                    cn == "Windows.UI.Core.CoreWindow" || cn == "ApplicationFrameWindow" && !HasTitle(hWnd))
                    return true;

                // Must have a title to count as a real window
                if (!HasTitle(hWnd)) return true;

                anyWindowOpen = true;
                return false; // stop enumeration
            }, IntPtr.Zero);

            return !anyWindowOpen;
        }

        private bool HasTitle(IntPtr hWnd)
        {
            var sb = new System.Text.StringBuilder(256);
            GetWindowText(hWnd, sb, 256);
            return sb.Length > 0;
        }

        // ── Taskbar transparency ─────────────────────────────────────────
        private void SetTaskbarTransparency(bool transparent)
        {
            // Primary taskbar
            IntPtr taskbar = FindWindow("Shell_TrayWnd", null);
            if (taskbar != IntPtr.Zero)
                ApplyAccent(taskbar, transparent);

            // Secondary taskbars (multi-monitor)
            IntPtr secondary = IntPtr.Zero;
            while (true)
            {
                secondary = FindWindowEx(IntPtr.Zero, secondary, "Shell_SecondaryTrayWnd", null);
                if (secondary == IntPtr.Zero) break;
                ApplyAccent(secondary, transparent);
            }
        }

        private void ApplyAccent(IntPtr hWnd, bool transparent)
        {
            var accent = new AccentPolicy
            {
                AccentState = transparent ? ACCENT_ENABLE_TRANSPARENTGRADIENT : ACCENT_DISABLED,
                AccentFlags = transparent ? 2 : 0,
                GradientColor = 0x00FFFFFF,
                AnimationId = 0
            };

            int accentSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentSize);
            try
            {
                Marshal.StructureToPtr(accent, accentPtr, false);
                var data = new WindowCompositionAttributeData
                {
                    Attribute = WCA_ACCENT_POLICY,
                    Data = accentPtr,
                    SizeOfData = accentSize
                };
                SetWindowCompositionAttribute(hWnd, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(accentPtr);
            }
        }

        // ── Desktop icon helpers ─────────────────────────────────────────
        private static bool GetDesktopIconsVisible()
        {
            IntPtr lv = GetDesktopListView();
            return lv != IntPtr.Zero && IsWindowVisible(lv);
        }

        private static IntPtr GetDesktopListView()
        {
            IntPtr progman = FindWindow("Progman", null);
            IntPtr defView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
            IntPtr lv = FindWindowEx(defView, IntPtr.Zero, "SysListView32", null);
            if (lv != IntPtr.Zero) return lv;

            IntPtr workerW = IntPtr.Zero;
            do
            {
                workerW = FindWindowEx(IntPtr.Zero, workerW, "WorkerW", null);
                defView = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (defView != IntPtr.Zero)
                {
                    lv = FindWindowEx(defView, IntPtr.Zero, "SysListView32", null);
                    break;
                }
            } while (workerW != IntPtr.Zero);

            return lv;
        }

        private static void SetDesktopIconsVisible(bool visible)
        {
            IntPtr lv = GetDesktopListView();
            if (lv != IntPtr.Zero)
                ShowWindow(lv, visible ? SW_SHOW : SW_HIDE);
        }

        // ── Tray icon drawing ────────────────────────────────────────────
        private static Icon CreateIcon(bool visible)
        {
            using var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            if (visible)
            {
                Color c = Color.FromArgb(255, 100, 210, 255);
                int sq = 5, gap = 2;
                int[][] pos = { new[]{0,0}, new[]{1,0}, new[]{0,1}, new[]{1,1} };
                foreach (var p in pos)
                {
                    int x = 1 + p[0] * (sq + gap), y = 1 + p[1] * (sq + gap);
                    g.FillRectangle(new SolidBrush(c), x, y, sq, sq);
                    g.DrawRectangle(new Pen(Color.FromArgb(180, 50, 150, 200), 1), x, y, sq - 1, sq - 1);
                }
            }
            else
            {
                Color c = Color.FromArgb(120, 180, 180, 180);
                int sq = 5, gap = 2;
                int[][] pos = { new[]{0,0}, new[]{1,0}, new[]{0,1}, new[]{1,1} };
                foreach (var p in pos)
                {
                    int x = 1 + p[0] * (sq + gap), y = 1 + p[1] * (sq + gap);
                    g.FillRectangle(new SolidBrush(c), x, y, sq, sq);
                }
                using var pen = new Pen(Color.FromArgb(220, 220, 60, 60), 2);
                g.DrawLine(pen, 2, 2, 13, 13);
                g.DrawLine(pen, 13, 2, 2, 13);
            }

            return Icon.FromHandle(bmp.GetHicon());
        }

        private string GetTooltipText() =>
            iconsVisible
                ? "Desktop Toggler — Icons: Visible (click to hide)"
                : "Desktop Toggler — Icons: Hidden (click to show)";

        // ── Cleanup ──────────────────────────────────────────────────────
        private void OnExitClick(object sender, EventArgs e)
        {
            desktopWatcher.Stop();
            SetTaskbarTransparency(false); // restore taskbar on exit
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                desktopWatcher?.Dispose();
                trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
