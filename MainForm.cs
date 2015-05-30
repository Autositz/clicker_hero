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
        Stopwatch swTest = new Stopwatch();
        private System.Timers.Timer tAutoClickTimer;
        private System.Timers.Timer tForcedDelayAutoClickTimer;
        private System.Timers.Timer tForcedDelayWaitAutoClickTimer;
        private bool bForceWait = true; // do we need to force a wait on next check
        DateTime starttAutoTimer = new DateTime();
        TimeSpan elapsedtAutoTimer = new TimeSpan();
        int iMaxClicksPerSecond = 40; // 40 clicks per second max
        private GlobalHotkey ghkAutoCLicker;
        
#if DEBUG
        int iTimerHours = 0;
        int iTimerMinutes = 0;
        int iTimerSeconds = 30;
        int iAutoTimerSeconds = 7;
        int iAutoTimerDelaySeconds = 5;
#else
        int iTimerHours = 0;
        int iTimerMinutes = 8; // do clickables every 8 minutes to not interrupt idle farming too much
        int iTimerSeconds = 0;
        int iAutoTimerSeconds = 45; // run for 45 seconds then wait for user interactions
        int iAutoTimerDelaySeconds = 6; // wait 6 seconds to allow user interactions
#endif
        
        public MainForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            this.CreateControl();
            this.Text += " " + this.ProductVersion;
            
            // register global hotkeys
            ghkAutoCLicker = new GlobalHotkey(Constants.ALT + Constants.SHIFT, Keys.A, this);
            
            
            if (ghkAutoCLicker.Register()) {
                MessageBox.Show("Enable/Disable AutoClicker with" + Environment.NewLine +
                                "ALT + SHIFT + A" + Environment.NewLine,
                                "AutoClicker Information", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
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
            
            clicks = new ClickLocations();
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
            if (bForceWait) {
                LOG.Add("DELAYAUTOCLICK: WAITING", 4);
                tAutoClickTimer.Stop();
//                LOG.Add("DELAYAUTOCLICK: 1");
//                labelAutoClicker.Text = "Waiting..."; // execution stops here but no exception raised...
//                LOG.Add("DELAYAUTOCLICK: 2");
                tForcedDelayWaitAutoClickTimer.Start();
                starttAutoTimer = DateTime.Now;
                LOG.Add("DELAYAUTOCLICK: WaitTimer started", 4);
            } else {
                LOG.Add("DELAYAUTOCLICK: RUNNING", 4);
                tForcedDelayAutoClickTimer.Start();
//                labelAutoClicker.Text = "Running...";
                tAutoClickTimer.Start();
                starttAutoTimer = DateTime.Now;
                LOG.Add("DELAYAUTOCLICK: AutoClickTimer started", 4);
            }
            bForceWait = !bForceWait;
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
            LOG.Add("GO: Start", 3);
            Point p = new Point(Convert.ToInt32(pStart.X + pLocation.X), Convert.ToInt32(pStart.Y + pLocation.Y));
            
            if (handle.ToInt32() != -1) {
                LOG.Add("GO: Bringing app to front", 4);
                Error("");
                SetForegroundWindow(handle.ToInt32());
                LOG.Add("GO: Moving Cursor to: " + p.ToString(), 9);
                Cursor.Position = p;
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new IntPtr());
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new IntPtr());
            } else {
                LOG.Add("GO: No Window handle found", 4);
                Error("Clicker Heroes not found or not running");
            }
        }
#endregion MOUSE_EVENT
        
        public bool Error(string msg)
        {
            if (this.InvokeRequired)
            {
                LOG.Add("ERROR: NEEDS INVOKE", 3);
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
        public Point clicker1 = new Point(530, 485);
        public Point clicker2 = new Point(750, 435);
        public Point clicker3 = new Point(760, 380);
        public Point clicker4 = new Point(875, 515);
        public Point clicker5 = new Point(1008, 452);
        public Point clicker6 = new Point(1055, 440);
        public Point level1 = new Point(100, 260);
        public Point level2 = new Point(100, 370);
        public Point level3 = new Point(100, 475);
        public Point level4 = new Point(100, 583);
        public Point clickerAuto = new Point(850, 300);
    }
    
        
    /// <summary>
    /// Holds static global variables
    /// </summary>
    public static class GlobalVar
    {
        public const bool DEBUG = true; // enable or disable debug messages
        public const int DEBUGLEVEL = 4; // enable or disable debug messages
    }
}
