/*
 * Created by SharpDevelop.
 * User: Autositz
 * Date: 25/05/2015
 * Time: 01:42
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using Hotkeys;

namespace clicker_hero
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Background timer to update tooltip info
        /// </summary>
        private System.Timers.Timer tClickTimer;
        private System.Windows.Forms.Timer tTickTimer;
        DateTime starttClickTimer = new DateTime();
        TimeSpan elapsedtClickTimer = new TimeSpan();
        private ClickLocations clicks;
//        Stopwatch swTest = new Stopwatch();
        private System.Timers.Timer tAutoClickTimer;
        private System.Timers.Timer tForcedDelayAutoClickTimer;
        private System.Timers.Timer tForcedDelayWaitAutoClickTimer;
        private bool bForceWait = true; // do we need to force a wait on next check
        DateTime starttAutoTimer = new DateTime();
        TimeSpan elapsedtAutoTimer = new TimeSpan();
        const int iMaxClicksPerSecond = 40; // 40 clicks per second max
        private GlobalHotkey ghkAutoCLicker;
        private bool bHotkeyRegistered = false;
        private bool bWaitDelay = true;
        
#if DEBUG
        int iTimerHours = 0;
        int iTimerMinutes = 0;
        int iTimerSeconds = 30;
        const int iAutoTimerSeconds = 7;
        const int iAutoTimerDelaySeconds = 5;
#else
        int iTimerHours = 0;
        int iTimerMinutes = 8; // do clickables every 8 minutes to not interrupt idle farming too much
        int iTimerSeconds = 0;
        const int iAutoTimerSeconds = 45; // run for 45 seconds then wait for user interactions
        const int iAutoTimerDelaySeconds = 6; // wait 6 seconds to allow user interactions
#endif
        
        public MainForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            this.CreateControl();
#if DEBUG
            this.Text += " DEBUG " + this.ProductVersion;
#else
            this.Text += " " + this.ProductVersion;
#endif
            
            // register global hotkeys
            ghkAutoCLicker = new GlobalHotkey(Constants.ALT + Constants.SHIFT, Keys.A, this);
            
            
            if (ghkAutoCLicker.Register()) {
#if RELEASE
                MessageBox.Show("Enable/Disable AutoClicker with" + Environment.NewLine +
                                "ALT + SHIFT + A" + Environment.NewLine,
                                "AutoClicker Information", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                bHotkeyRegistered = true;
#endif
            } else {
                MessageBox.Show("Unable to register global hotkey for AutoClicker!" + Environment.NewLine + 
                                "Autoclicker is clicking for " + iAutoTimerSeconds + " seconds and will then wait " + iAutoTimerDelaySeconds + " seconds before clicking again." + Environment.NewLine,
                                "AutoClicker Information", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            
            LOG.Add("MAIN: Set ClickTimer", 5);
            tClickTimer = new System.Timers.Timer();
            SetNewTimer();
            tClickTimer.AutoReset = true;
            tClickTimer.Elapsed += new ElapsedEventHandler(CheckNextTime);
            LOG.Add("MAIN: Set TickTimer", 5);
            tTickTimer = new System.Windows.Forms.Timer();
            tTickTimer.Tick += new EventHandler(tTickTimer_Tick);
            tTickTimer.Interval = 1;
            tTickTimer.Enabled = true;
            
            LOG.Add("MAIN: Set AutoClickTimer", 5);
            tAutoClickTimer = new System.Timers.Timer();
            tAutoClickTimer.AutoReset = true;
            tAutoClickTimer.Elapsed += new ElapsedEventHandler(DoAutoClick);
            tAutoClickTimer.Interval = 1000 / iMaxClicksPerSecond;
            
            LOG.Add("MAIN: Set ForcedDelayAutoClickTimer", 5);
            tForcedDelayAutoClickTimer = new System.Timers.Timer();
            tForcedDelayAutoClickTimer.AutoReset = false;
            tForcedDelayAutoClickTimer.Elapsed += new ElapsedEventHandler(DelayAutoClick);
            tForcedDelayAutoClickTimer.Interval = 1000 * iAutoTimerSeconds;
            LOG.Add("MAIN: Set ForcedDelayWaitAutoClickTimer", 5);
            tForcedDelayWaitAutoClickTimer = new System.Timers.Timer();
            tForcedDelayWaitAutoClickTimer.AutoReset = false;
            tForcedDelayWaitAutoClickTimer.Elapsed += new ElapsedEventHandler(DelayAutoClick);
            tForcedDelayWaitAutoClickTimer.Interval = 1000 * iAutoTimerDelaySeconds;
            
            StartStopTimer();

            
//            labelTimer.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2"));
            textBox1.Text = iTimerHours.ToString();
            textBox2.Text = iTimerMinutes.ToString();
            textBox3.Text = iTimerSeconds.ToString();
            
            CheckBoxBackgroundCheckedChanged(); // set click locations according to background state
            DoIt();
        }
        
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Constants.WM_HOTKEY_MSG_ID)
            {
                switch (GlobalHotkey.GetKey(m.LParam)) {
                    case Keys.A:
                        FlipAutoClicker();
                        StartStopAutoTimer();
                        break;
                }
            }
            base.WndProc(ref m);
        }
        
        public void FlipAutoClicker()
        {
            checkBoxAutoClicker.Checked = !checkBoxAutoClicker.Checked;
        }
        
        public void DelayAutoClick(object sender, EventArgs e)
        {
            if (bForceWait && bWaitDelay) {
                LOG.Add("DELAYAUTOCLICK: WAITING", 3);
                tAutoClickTimer.Stop();
//                LOG.Add("DELAYAUTOCLICK: 1");
//                labelAutoClicker.Text = "Waiting..."; // execution stops here but no exception raised...
//                LOG.Add("DELAYAUTOCLICK: 2");
                tForcedDelayWaitAutoClickTimer.Start();
                starttAutoTimer = DateTime.Now;
                LOG.Add("DELAYAUTOCLICK: WaitTimer started", 4);
                bForceWait = false;
            } else {
                LOG.Add("DELAYAUTOCLICK: RUNNING", 3);
                tForcedDelayAutoClickTimer.Start();
//                labelAutoClicker.Text = "Running...";
                tAutoClickTimer.Start();
                starttAutoTimer = DateTime.Now;
                LOG.Add("DELAYAUTOCLICK: AutoClickTimer started", 4);
                bForceWait = true;
            }
        }
        
        private async void DoIt()
        {
            LOG.Add("DOIT: Start", 2);
//            swTest.Restart();
            starttClickTimer = DateTime.Now;
            
            if (!checkBoxTimerActive.Checked)
                return;
            
            IntPtr handle = new IntPtr(-1);
            Point po = new Point();
            GetHandleCoords("clicker heroes", ref handle, ref po);
            
            if (handle.ToInt32() != -1) {
                LOG.Add("DOIT: We got a window", 4);
                Error("");
                if (checkBoxHeroLevel1.Checked) {
                    LOG.Add("DOIT: Hero Level 1", 4);
                    go(po, clicks.level1, handle);
                    await Wait(150);
                }
                if (checkBoxHeroLevel2.Checked) {
                    LOG.Add("DOIT: Hero Level 2", 4);
                    go(po, clicks.level2, handle);
                    await Wait(150);
                }
                if (checkBoxHeroLevel3.Checked) {
                    LOG.Add("DOIT: Hero Level 3", 4);
                    go(po, clicks.level3, handle);
                    await Wait(150);
                }
                if (checkBoxHeroLevel4.Checked) {
                    LOG.Add("DOIT: Hero Level 4", 4);
                    go(po, clicks.level4, handle);
                    await Wait(150);
                }
                if (checkBoxClickables.Checked) {
                    LOG.Add("DOIT: Doing Clickables", 4);
                    go(po, clicks.clicker1, handle);
                    await Wait(150);
                    go(po, clicks.clicker2, handle);
                    await Wait(150);
                    go(po, clicks.clicker3, handle);
                    await Wait(150);
                    go(po, clicks.clicker4, handle);
                    await Wait(150);
                    go(po, clicks.clicker5, handle);
                    await Wait(150);
                    go(po, clicks.clicker6, handle);
                    await Wait(150);
                    LOG.Add("DOIT: Done Clickables", 4);
                }
            } else {
                LOG.Add("DOIT: No Window handle found", 4);
                Error("Clicker Heroes not found or not running...");
            }
        }
        
        async Task Wait(int waittime)
        {
            await Task.Delay(waittime);
        }

        void DoAutoClick(object sender, EventArgs e)
        {
            IntPtr handle = new IntPtr(-1);
            Point po = new Point();
            GetHandleCoords("clicker heroes", ref handle, ref po);
            
            if (checkBoxAutoClicker.Checked) {
                LOG.Add("DOAUTOCLICK: Autoclicking", 6);
                go(po, clicks.clickerAuto, handle);
            }
        }
        
        public void CheckNextTime(object sender, EventArgs e)
        {
            LOG.Add("CHECKNEXTTIME: Call from timer", 3);
            DoIt();
        }
        
        private void tTickTimer_Tick(object sender, EventArgs e)
        {
            string preText = "";
            double dInterval = 0;
            elapsedtClickTimer = DateTime.Now - starttClickTimer;
            elapsedtAutoTimer = DateTime.Now - starttAutoTimer;
//            labelElapsed.Text = swTest.Elapsed.ToString();
            labelElapsed.Text = elapsedtClickTimer.ToString("hh\\:mm\\:ss\\.fff");
            if (checkBoxAutoClicker.Checked) {
                if (bForceWait) {
                    preText = "clicking...";
                    dInterval = tForcedDelayAutoClickTimer.Interval;
                } else {
                    preText = "waiting...";
                    dInterval = tForcedDelayWaitAutoClickTimer.Interval;
                }
                LOG.Add("TTICKTIMER_TICK: " + preText + TimeSpan.FromMilliseconds(dInterval).ToString("hh\\:mm\\:ss\\.fff") + " - " + elapsedtAutoTimer.ToString("hh\\:mm\\:ss\\.fff"), 9);
                labelAutoClicker.Text = preText + Environment.NewLine + (TimeSpan.FromMilliseconds(dInterval) - elapsedtAutoTimer).ToString("hh\\:mm\\:ss\\.fff");
            }
        }
        
        void TimerChanged(object sender, EventArgs e)
        {
            LOG.Add("TIMERCHANGED: Timer has changed", 5);
            SetTimer();
        }
        
        void SetTimer()
        {
            LOG.Add("SETTIMER: Parsing Textbox to variables", 5);
            int i;
            if (int.TryParse(textBox1.Text, out i)) {
                iTimerHours = i;
            }
            if (int.TryParse(textBox2.Text, out i)) {
                iTimerMinutes = i;
            }
            if (int.TryParse(textBox3.Text, out i)) {
                iTimerSeconds = i;
            }
            SetNewTimer();
        }

        void SetNewTimer()
        {
            int iTime = 0; // time to count in seconds
            if (iTimerHours > 0)
                iTime += 60 * 60 * iTimerHours;
            if (iTimerMinutes > 0)
                iTime += 60 * iTimerMinutes;
            if (iTimerSeconds > 0)
                iTime += iTimerSeconds;
            
            LOG.Add("SETNEWTIMER: New timer set to " + iTime + " sec", 5);
            if (iTime < 10) {
                LOG.Add("SETNEWTIMER: OVERRIDE to 10 sec", 5);
                iTime = 10; // override small values to remain responsive
            }
            tClickTimer.Interval = 1000 * iTime;
            starttClickTimer = DateTime.Now;
//            labelTimerSet.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2")) + Environment.NewLine + tClickTimer.Interval;
            labelTimerSet.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2"));
        }
        
        void StartStopTimer()
        {
            LOG.Add("STARTSTOPTIMER: Wrapper 1", 4);
            StartStopTimer(new object(), new EventArgs(), false);
        }
        
        void StartStopTimer(object sender, EventArgs e)
        {
            LOG.Add("STARTSTOPTIMER: Wrapper 2", 4);
            StartStopTimer(sender, e, true);
        }
        
        void StartStopTimer(object sender, EventArgs e, bool data)
        {
            if (checkBoxTimerActive.Checked) {
                LOG.Add("STARTSTOPTIMER: STARTING", 3);
                tClickTimer.Start();
                starttClickTimer = DateTime.Now;
                tTickTimer.Enabled = true;
                StartStopAutoTimer(); // no direct start as the checkbox might not be active
            } else {
                LOG.Add("STARTSTOPTIMER: STOPPING", 3);
                StopAutoTimer();
                tTickTimer.Enabled = false;
                tClickTimer.Stop();
            }
        }
        
        void StartStopAutoTimer()
        {
            LOG.Add("STARTSTOPAUTOTIMER: Wrapper 1", 4);
            StartStopAutoTimer(new object(), new EventArgs(), false);
        }
        
        void StartStopAutoTimer(object sender, EventArgs e)
        {
            LOG.Add("STARTSTOPAUTOTIMER: Wrapper 2", 4);
            StartStopAutoTimer(sender, e, true);
        }
        
        void StartStopAutoTimer(object sender, EventArgs e, bool data)
        {
            if (checkBoxAutoClicker.Checked && checkBoxTimerActive.Checked) {
                LOG.Add("STARTSTOPAUTOTIMER: STARTING", 3);
                StartAutoTimer();
                Error("AutoClicker started");
            } else {
                LOG.Add("STARTSTOPAUTOTIMER: STOPPING", 3);
                StopAutoTimer();
                Error("AutoClicker stopped");
            }
        }

        void StartAutoTimer()
        {
            tAutoClickTimer.Start();
            starttAutoTimer = DateTime.Now;
            tForcedDelayAutoClickTimer.Start();
            labelAutoClicker.Visible = true;
            checkBoxAutoClicker.Checked = true;
            bForceWait = true; // enable to force a wait on next switch no matter the current state
        }

        void StopAutoTimer()
        {
            labelAutoClicker.Visible = false;
            checkBoxAutoClicker.Checked = false;
            tForcedDelayWaitAutoClickTimer.Stop();
            tForcedDelayAutoClickTimer.Stop();
            tAutoClickTimer.Stop();
        }
        
#region MOUSE_EVENT
        /// <summary>
        /// stuff to send input to other window
        /// </summary>
        
        
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        
        private void GetHandleCoords(string processname, ref IntPtr handle, ref Point po)
        {
//            LOG.Add("GETHANDLE: Start");
            foreach (Process p in Process.GetProcessesByName(processname)) {
                handle = p.MainWindowHandle;
                RECT rct = new RECT();
                GetWindowRect(handle, ref rct);
                po = new Point(rct.Left, rct.Top);
                LOG.Add("GETHANDLE: Found at " + po, 5);
            }
        }
        
        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
        
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);
        
        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);
        
        public void go(Point pStart, Point pLocation)
        {
            LOG.Add("GO: Wrapper", 4);
            go(pStart, pLocation, new IntPtr(-1));
        }
        
        public void go(Point pStart, Point pLocation, IntPtr handle)
        {
            LOG.Add("GO: Start", 4);
            
            if (handle.ToInt32() != -1) {
                Error("");
                if (checkBoxBackground.Checked) {
                    LOG.Add("GO: Sending to background app", 4);
//                    ControlClickWindow("Clicker Heroes", "left", pLocation.X, pLocation.Y, false); // SEND_INPUT
                    _PostMessage(handle, pLocation); // POSTMESSAGE
                } else {
                    Point p = new Point(Convert.ToInt32(pStart.X + pLocation.X), Convert.ToInt32(pStart.Y + pLocation.Y));
                    LOG.Add("GO: Bringing app to front", 4);
                    SetForegroundWindow(handle.ToInt32());
                    LOG.Add("GO: Moving Cursor to: " + p.ToString(), 9);
                    Cursor.Position = p;
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new IntPtr());
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new IntPtr());
                }
            } else {
                LOG.Add("GO: No Window handle found", 4);
                Error("Clicker Heroes not found or not running");
            }
        }
#endregion MOUSE_EVENT
#region SEND_INPUT
        // taken from http://www.blizzhackers.cc/viewtopic.php?t=396398
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        /// <summary>
        /// Virtual Messages
        /// </summary>
        public enum WMessages : int
        {
            WM_LBUTTONDOWN = 0x201, //Left mousebutton down
            WM_LBUTTONUP = 0x202,  //Left mousebutton up
            WM_LBUTTONDBLCLK = 0x203, //Left mousebutton doubleclick
            WM_RBUTTONDOWN = 0x204, //Right mousebutton down
            WM_RBUTTONUP = 0x205,   //Right mousebutton up
            WM_RBUTTONDBLCLK = 0x206, //Right mousebutton doubleclick
            WM_KEYDOWN = 0x100,  //Key down
            WM_KEYUP = 0x101,   //Key up
        }
    
        /// <summary>
        /// Virtual Keys
        /// </summary>
        public enum VKeys : int
        {
            VK_LBUTTON = 0x01,   //Left mouse button
            VK_RBUTTON = 0x02,   //Right mouse button
            VK_CANCEL = 0x03,   //Control-break processing
            VK_MBUTTON = 0x04,   //Middle mouse button (three-button mouse)
            VK_BACK = 0x08,   //BACKSPACE key
            VK_TAB = 0x09,   //TAB key
            VK_CLEAR = 0x0C,   //CLEAR key
            VK_RETURN = 0x0D,   //ENTER key
            VK_SHIFT = 0x10,   //SHIFT key
            VK_CONTROL = 0x11,   //CTRL key
            VK_MENU = 0x12,   //ALT key
            VK_PAUSE = 0x13,   //PAUSE key
            VK_CAPITAL = 0x14,   //CAPS LOCK key
            VK_ESCAPE = 0x1B,   //ESC key
            VK_SPACE = 0x20,   //SPACEBAR
            VK_PRIOR = 0x21,   //PAGE UP key
            VK_NEXT = 0x22,   //PAGE DOWN key
            VK_END = 0x23,   //END key
            VK_HOME = 0x24,   //HOME key
            VK_LEFT = 0x25,   //LEFT ARROW key
            VK_UP = 0x26,   //UP ARROW key
            VK_RIGHT = 0x27,   //RIGHT ARROW key
            VK_DOWN = 0x28,   //DOWN ARROW key
            VK_SELECT = 0x29,   //SELECT key
            VK_PRINT = 0x2A,   //PRINT key
            VK_EXECUTE = 0x2B,   //EXECUTE key
            VK_SNAPSHOT = 0x2C,   //PRINT SCREEN key
            VK_INSERT = 0x2D,   //INS key
            VK_DELETE = 0x2E,   //DEL key
            VK_HELP = 0x2F,   //HELP key
            VK_0 = 0x30,   //0 key
            VK_1 = 0x31,   //1 key
            VK_2 = 0x32,   //2 key
            VK_3 = 0x33,   //3 key
            VK_4 = 0x34,   //4 key
            VK_5 = 0x35,   //5 key
            VK_6 = 0x36,    //6 key
            VK_7 = 0x37,    //7 key
            VK_8 = 0x38,   //8 key
            VK_9 = 0x39,    //9 key
            VK_A = 0x41,   //A key
            VK_B = 0x42,   //B key
            VK_C = 0x43,   //C key
            VK_D = 0x44,   //D key
            VK_E = 0x45,   //E key
            VK_F = 0x46,   //F key
            VK_G = 0x47,   //G key
            VK_H = 0x48,   //H key
            VK_I = 0x49,    //I key
            VK_J = 0x4A,   //J key
            VK_K = 0x4B,   //K key
            VK_L = 0x4C,   //L key
            VK_M = 0x4D,   //M key
            VK_N = 0x4E,    //N key
            VK_O = 0x4F,   //O key
            VK_P = 0x50,    //P key
            VK_Q = 0x51,   //Q key
            VK_R = 0x52,   //R key
            VK_S = 0x53,   //S key
            VK_T = 0x54,   //T key
            VK_U = 0x55,   //U key
            VK_V = 0x56,   //V key
            VK_W = 0x57,   //W key
            VK_X = 0x58,   //X key
            VK_Y = 0x59,   //Y key
            VK_Z = 0x5A,    //Z key
            VK_NUMPAD0 = 0x60,   //Numeric keypad 0 key
            VK_NUMPAD1 = 0x61,   //Numeric keypad 1 key
            VK_NUMPAD2 = 0x62,   //Numeric keypad 2 key
            VK_NUMPAD3 = 0x63,   //Numeric keypad 3 key
            VK_NUMPAD4 = 0x64,   //Numeric keypad 4 key
            VK_NUMPAD5 = 0x65,   //Numeric keypad 5 key
            VK_NUMPAD6 = 0x66,   //Numeric keypad 6 key
            VK_NUMPAD7 = 0x67,   //Numeric keypad 7 key
            VK_NUMPAD8 = 0x68,   //Numeric keypad 8 key
            VK_NUMPAD9 = 0x69,   //Numeric keypad 9 key
            VK_SEPARATOR = 0x6C,   //Separator key
            VK_SUBTRACT = 0x6D,   //Subtract key
            VK_DECIMAL = 0x6E,   //Decimal key
            VK_DIVIDE = 0x6F,   //Divide key
            VK_F1 = 0x70,   //F1 key
            VK_F2 = 0x71,   //F2 key
            VK_F3 = 0x72,   //F3 key
            VK_F4 = 0x73,   //F4 key
            VK_F5 = 0x74,   //F5 key
            VK_F6 = 0x75,   //F6 key
            VK_F7 = 0x76,   //F7 key
            VK_F8 = 0x77,   //F8 key
            VK_F9 = 0x78,   //F9 key
            VK_F10 = 0x79,   //F10 key
            VK_F11 = 0x7A,   //F11 key
            VK_F12 = 0x7B,   //F12 key
            VK_SCROLL = 0x91,   //SCROLL LOCK key
            VK_LSHIFT = 0xA0,   //Left SHIFT key
            VK_RSHIFT = 0xA1,   //Right SHIFT key
            VK_LCONTROL = 0xA2,   //Left CONTROL key
            VK_RCONTROL = 0xA3,    //Right CONTROL key
            VK_LMENU = 0xA4,      //Left MENU key
            VK_RMENU = 0xA5,   //Right MENU key
            VK_PLAY = 0xFA,   //Play key
            VK_ZOOM   = 0xFB, //Zoom key
        }
        
        /// <summary>
        /// Sends a message to the specified handle
        /// </summary>
        public void _SendMessage(IntPtr handle, int Msg, int wParam, int lParam)
        {
            SendMessage(handle, Msg, wParam, lParam);
        }
       
        /// <summary>
        /// MakeLParam Macro
        /// </summary>
        public int MakeLParam(int LoWord, int HiWord)
        {
            return ((HiWord << 16) | (LoWord & 0xffff));
        }

        /// <summary>
        /// returns handle of specified window name
        /// </summary>
        public IntPtr FindWindow(string wndName)
        {
            return FindWindow(null, wndName);
        }

        public void ControlClickWindow(string wndName, string button, int x, int y, bool doubleklick)
        {
            IntPtr hWnd = FindWindow(null, wndName);
            int LParam = MakeLParam(x, y);

            int btnDown = 0;
            int btnUp = 0;

            if (button == "left")
            {
                btnDown = (int)WMessages.WM_LBUTTONDOWN;
                btnUp = (int)WMessages.WM_LBUTTONUP;
            }

            if (button == "right")
            {
                btnDown = (int)WMessages.WM_RBUTTONDOWN;
                btnUp = (int)WMessages.WM_RBUTTONUP;
            }


            if (doubleklick == true)
            {
                _SendMessage(hWnd, btnDown, 0, LParam);
                _SendMessage(hWnd, btnUp, 0, LParam);
                _SendMessage(hWnd, btnDown, 0, LParam);
                _SendMessage(hWnd, btnUp, 0, LParam);
            }

            if (doubleklick == false)

            { _SendMessage(hWnd, btnDown, 0, LParam);
              _SendMessage(hWnd, btnUp, 0, LParam); }
               
        }
#endregion SEND_INPUT
#region POSTMESSAGE
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        
        public void _PostMessage(IntPtr handle, Point p)
        {
            PostMessage(handle, WM_LBUTTONDOWN, 1, MakeLParam(p.X, p.Y));
            PostMessage(handle, WM_LBUTTONUP, 1, MakeLParam(p.X, p.Y));
        }
#endregion POSTMESSAGE
        
        public bool Error(string msg)
        {
            if (this.InvokeRequired)
            {
                LOG.Add("ERROR: NEEDS INVOKE", 6);
                return (bool)this.Invoke ((Func<string,bool>)Error, msg);
            }
            if (msg != "") {
                msg = DateTime.Now.ToString("HH:mm:ss") + " - " + msg;
            }
            labelErrors.Text = msg;
            
            return true;
        }
        void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ghkAutoCLicker.Unregister())
                MessageBox.Show("Error while unregistering hotkey.",
                                "AutoClicker Information", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
        }
        
        void CheckBoxBackgroundCheckedChanged()
        {
            CheckBoxBackgroundCheckedChanged(new object(), new EventArgs());
        }
        
        void CheckBoxBackgroundCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxBackground.Checked) {
                clicks = new ClickLocations(true);
            } else {
                clicks = new ClickLocations(false);
            }
        }
        
        void CheckBoxWaitDelayCheckedChanged(object sender, EventArgs e)
        {
            LOG.Add("CHECKBOXWAITDELAYCHECKEDCHANGED: START", 3);
            if (checkBoxWaitDelay.Checked && !checkBoxBackground.Checked) {
                if (bHotkeyRegistered) {
                    DialogResult userchoice = MessageBox.Show("Enable/Disable AutoClicker with" + Environment.NewLine +
                                    "ALT + SHIFT + A" + Environment.NewLine,
                                    "AutoClicker Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    if (userchoice == DialogResult.OK) {
                        LOG.Add("CHECKBOXWAITDELAYCHECKEDCHANGED: User OK", 6);
                        bWaitDelay = false;
                    } else {
                        LOG.Add("CHECKBOXWAITDELAYCHECKEDCHANGED: User Cancel", 6);
                        bWaitDelay = true;
                        checkBoxWaitDelay.Checked = false;
                    }
                } else {
                    LOG.Add("CHECKBOXWAITDELAYCHECKEDCHANGED: NO HOTKEY", 6);
                    bWaitDelay = true;
                    checkBoxWaitDelay.Checked = false;
                    MessageBox.Show("AutoClickDelay should not be deactivated while no hotkey is set!" + Environment.NewLine +
                                    "Please restart the program to set the hotkey.",
                                    "AutoClicker Information", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            } else {
                // TODO: something is fishy here, i am missing a check when to activate and deactivate the delay in combination with background activity and hotkey availability
                LOG.Add("CHECKBOXWAITDELAYCHECKEDCHANGED: Default wait", 6);
                bWaitDelay = true;
            }
        }
    }
    
    public static class LOG
    {
        public static void Add(string msg)
        {
            Add(msg, 0);
        }
        
        public static void Add(string msg, int level)
        {
            Debug.WriteLineIf((GlobalVar.DEBUG && level <= GlobalVar.DEBUGLEVEL), DateTime.Now.ToString("HH\\:mm\\:ss\\.fff") + " " + msg);
        }
    }
    
    public class ClickLocations
    {
        // active window clicks
        public Point clicker1 { get; set; }
        public Point clicker2 { get; set; }
        public Point clicker3 { get; set; }
        public Point clicker4 { get; set; }
        public Point clicker5 { get; set; }
        public Point clicker6 { get; set; }
        public Point level1 { get; set; }
        public Point level2 { get; set; }
        public Point level3 { get; set; }
        public Point level4 { get; set; }
        public Point clickerAuto { get; set; }
        
        public ClickLocations(bool background)
        {
            if (background) {
                // background clicks
                clicker1 = new Point(525, 460);
                clicker2 = new Point(745, 410);
                clicker3 = new Point(755, 355);
                clicker4 = new Point(870, 490);
                clicker5 = new Point(1003, 427);
                clicker6 = new Point(1050, 415);
                level1 = new Point(95, 235);
                level2 = new Point(95, 345);
                level3 = new Point(95, 450);
                level4 = new Point(95, 558);
                clickerAuto = new Point(970, 117);
            } else {
                // active window clicks
                clicker1 = new Point(530, 485);
                clicker2 = new Point(750, 435);
                clicker3 = new Point(760, 380);
                clicker4 = new Point(875, 515);
                clicker5 = new Point(1008, 452);
                clicker6 = new Point(1055, 440);
                level1 = new Point(100, 260);
                level2 = new Point(100, 370);
                level3 = new Point(100, 475);
                level4 = new Point(100, 583);
                clickerAuto = new Point(975, 145);
            }
        }
    }
    
    
    /// <summary>
    /// Holds static global variables
    /// </summary>
    public static class GlobalVar
    {
        public const bool DEBUG = true; // enable or disable debug messages
        public const int DEBUGLEVEL = 3; // enable or disable debug messages
    }
}
