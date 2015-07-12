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
    /// Main user interface
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Background timer to update tooltip info
        /// </summary>
        private System.Timers.Timer tClickTimer;
        /// <summary>
        /// Background timer to display times in GUI
        /// </summary>
        private System.Windows.Forms.Timer tTickTimer;
        /// <summary>
        /// Last start of the tClickTimer
        /// </summary>
        DateTime starttClickTimer = new DateTime();
        /// <summary>
        /// Time difference between Now and starttClickTimer
        /// </summary>
        TimeSpan elapsedtClickTimer = new TimeSpan();
        /// <summary>
        /// Coordinates to be clicked
        /// </summary>
        private ClickLocations clicks;
        /// <summary>
        /// Monitor start time of application
        /// </summary>
//        Stopwatch swTest = new Stopwatch();
        /// <summary>
        /// Background timer to perform autoclicking
        /// </summary>
        private System.Timers.Timer tAutoClickTimer;
        /// <summary>
        /// Timer for duration of autoclicker
        /// </summary>
        private System.Timers.Timer tForcedDelayAutoClickTimer;
        /// <summary>
        /// Timer for wait delay for autoclicker
        /// </summary>
        private System.Timers.Timer tForcedDelayWaitAutoClickTimer;
        /// <summary>
        /// do we need to force a wait on next check
        /// </summary>
        private bool bForceWait = true;
        /// <summary>
        /// Last start of the tAutoClickTimer
        /// </summary>
        DateTime starttAutoTimer = new DateTime();
        /// <summary>
        /// Time difference between Now and starttAutoTimer
        /// </summary>
        TimeSpan elapsedtAutoTimer = new TimeSpan();
        /// <summary>
        /// Maximum number of clicks per seconds for tAutoClickTimer
        /// </summary>
        const int iMaxClicksPerSecond = 40; // 40 clicks per second max
        /// <summary>
        /// Global hotkey setting
        /// </summary>
        private GlobalHotkey ghkAutoCLicker;
        /// <summary>
        /// True when registering hotkey was successfull
        /// </summary>
        private bool bHotkeyRegistered = false;
        /// <summary>
        /// True when we want to have a wait delay in between autoclicker
        /// </summary>
        private bool bWaitDelay = true;
        /// <summary>
        /// Background timer to click AutoProgress every now and then
        /// </summary>
        private System.Timers.Timer tAutoProgressTimer;
        /// <summary>
        /// Time in seconds when to click AutoProgress mode
        /// </summary>
        const int iAutoProgressTime = 30; // every 30s to not stay without progression for too long?
        /// <summary>
        /// Time in seconds to keep the combo clicker alive
        /// </summary>
        const int iPreserveCombo = 3;
        /// <summary>
        /// Background timer to keep the combo alive
        /// </summary>
        private System.Timers.Timer tPreserveComboTimer;
        
#if DEBUG
        /// <summary>
        /// Main timer hours
        /// </summary>
        int iTimerHours = 0;
        /// <summary>
        /// Main timer minutes
        /// </summary>
        int iTimerMinutes = 0;
        /// <summary>
        /// Main timer seconds
        /// </summary>
        int iTimerSeconds = 30;
        /// <summary>
        /// Timer AutoClick seconds
        /// </summary>
        const int iAutoTimerSeconds = 7;
        /// <summary>
        /// Timer AutoclickDelay seconds
        /// </summary>
        const int iAutoTimerDelaySeconds = 5;
#else
        /// <summary>
        /// Main timer hours
        /// </summary>
        int iTimerHours = 0;
        /// <summary>
        /// Main timer minutes
        /// </summary>
        int iTimerMinutes = 8; // do clickables every 8 minutes to not interrupt idle farming too much
        /// <summary>
        /// Main timer seconds
        /// </summary>
        int iTimerSeconds = 0;
        /// <summary>
        /// Timer AutoClick seconds
        /// </summary>
        const int iAutoTimerSeconds = 45; // run for 45 seconds then wait for user interactions
        /// <summary>
        /// Timer AutoclickDelay seconds
        /// </summary>
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
            
            LOG.Add("MAIN: Set PreserveComboTimer", 5);
            tPreserveComboTimer = new System.Timers.Timer();
            tPreserveComboTimer.AutoReset = true;
            tPreserveComboTimer.Elapsed += new ElapsedEventHandler(DoPreserveCombo);
            tPreserveComboTimer.Interval = 1000 * iPreserveCombo;
            
            // FIXME: is it correct to have DelayAutoClick everywhere?
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
            
            
            LOG.Add("MAIN: Set AutoProgressTimer", 5);
            tAutoProgressTimer = new System.Timers.Timer();
            tAutoProgressTimer.AutoReset = true;
            tAutoProgressTimer.Elapsed += new ElapsedEventHandler(DoAutoProgress);
            tAutoProgressTimer.Interval = 1000 * iAutoProgressTime;
            
            CheckBoxBackgroundCheckedChanged(); // set click locations according to background state, needs to be done before timers kick in!
            StartStopTimer();

            
//            labelTimer.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2"));
            textBox1.Text = iTimerHours.ToString();
            textBox2.Text = iTimerMinutes.ToString();
            textBox3.Text = iTimerSeconds.ToString();
            
            DoIt();
        }
        
        /// <summary>
        /// Catch windows event messages used for GlobalHotkey ghkAutoCLicker
        /// </summary>
        /// <param name="m">Windows event message</param>
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
        
        /// <summary>
        /// Switch the AutoClicker checkbox
        /// </summary>
        public void FlipAutoClicker()
        {
            checkBoxAutoClicker.Checked = !checkBoxAutoClicker.Checked;
        }
        
        /// <summary>
        /// Timer event to switch AutoClicker on/off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DelayAutoClick(object sender, EventArgs e)
        {
            if (bForceWait && bWaitDelay) {
                LOG.Add("DELAYAUTOCLICK: WAITING", 3);
                tAutoClickTimer.Stop();
                // TODO: Fixed by setting labelAutoClicker to public?
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
        
        /// <summary>
        /// Timer event to switch AutoClicker on/off
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">ElapsedEventArgs</param>
        public void DoAutoProgress(object sender, EventArgs e)
        {
            // FIXME: shorter timer but still higher than boss timer to not stay in waiting more for too long when we are currently progressing in levels? (slows down progression by not advancing...)
            // HACK: Shouldn't this be ignored if progression gets clicked every minute because it will always progress every now and then and in case of emergency won't stay long without?
            LOG.Add("DOAUTOPROGRESS: Let's click the progress button", 4);
            IntPtr handle = new IntPtr(-1);
            Point po = new Point();
            GetHandleCoords("clicker heroes", ref handle, ref po);
            
            if (checkBoxAutoProgress.Checked) {
                LOG.Add("DOAUTOPROGRESS: Clicking Auto progression", 4);
                go(po, clicks.autoProgress, handle);
            }
            
        }
        
        /// <summary>
        /// Run all the clicking
        /// </summary>
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
        
        /// <summary>
        /// Waiting time for operations
        /// </summary>
        /// <param name="waittime">Time in ms</param>
        /// <returns></returns>
        async Task Wait(int waittime)
        {
            await Task.Delay(waittime);
        }
        
        /// <summary>
        /// Timer event to perform AutoClicking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        
        /// <summary>
        /// Timer event to keep Combo alive (needs to be done in less than 10s)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DoPreserveCombo(object sender, EventArgs e)
        {
            IntPtr handle = new IntPtr(-1);
            Point po = new Point();
            GetHandleCoords("clicker heroes", ref handle, ref po);
            
            if (checkBoxPreserveCombo.Checked) {
                LOG.Add("DOPRESERVECOMBO: PreserveCombo", 6);
                go(po, clicks.clickerAuto, handle);
            }
        }
        
        /// <summary>
        /// Timer event from default timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckNextTime(object sender, EventArgs e)
        {
            LOG.Add("CHECKNEXTTIME: Call from timer", 3);
            DoIt();
        }
        
        /// <summary>
        /// Timer event from short tick timer (display ms times)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        
        /// <summary>
        /// Set Timer button clicking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TimerChanged(object sender, EventArgs e)
        {// FIXME: Timer sometimes overruns the set limit
            LOG.Add("TIMERCHANGED: Timer has changed", 5);
            SetTimer();
        }
        
        /// <summary>
        /// Set the default timer to current textbox entries
        /// </summary>
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
            SetNewTimer(true);
        }
        
        /// <summary>
        /// Set the default timer without starting timer
        /// </summary>
        void SetNewTimer()
        {
            SetNewTimer(false);
        }
        
        /// <summary>
        /// Set the default timer and start it when requested
        /// </summary>
        void SetNewTimer(bool bStart)
        {
            int iTime = 0; // time to count in seconds
            if (iTimerHours > 0)
                iTime += 60 * 60 * iTimerHours;
            if (iTimerMinutes > 0)
                iTime += 60 * iTimerMinutes;
            if (iTimerSeconds > 0)
                iTime += iTimerSeconds;
            
            LOG.Add("SETNEWTIMER: New timer set to " + iTime + " sec", 5);
            // FIXME: only enforce for foreground click, ignore for background clicks
            // FIXME: make a check when foreground/background gets changed and set timer accordingly
//            if (iTime < 10) {
//                LOG.Add("SETNEWTIMER: OVERRIDE to 10 sec", 5);
//                iTime = 10; // override small values to remain responsive
//            }
            tClickTimer.Interval = 1000 * iTime;
            
            // timer needs to get started when requested
            if (bStart) {
                tClickTimer.Start();
            }
            starttClickTimer = DateTime.Now;
//            labelTimerSet.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2")) + Environment.NewLine + tClickTimer.Interval;
            labelTimerSet.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2"));
        }
        
        /// <summary>
        /// Start and Stop the default timer manually
        /// </summary>
        void StartStopTimer()
        {
            LOG.Add("STARTSTOPTIMER: Wrapper 1", 4);
            StartStopTimer(new object(), new EventArgs(), false);
        }
        
        /// <summary>
        /// Start and Stop the default timer called from Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartStopTimer(object sender, EventArgs e)
        {
            LOG.Add("STARTSTOPTIMER: Wrapper 2", 4);
            StartStopTimer(sender, e, true);
        }
        
        /// <summary>
        /// Start and Stop the default timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="data">True when sent from an event handler</param>
        void StartStopTimer(object sender, EventArgs e, bool data)
        {
            if (checkBoxTimerActive.Checked) {
                LOG.Add("STARTSTOPTIMER: STARTING", 3);
                tClickTimer.Start();
                starttClickTimer = DateTime.Now;
                tTickTimer.Enabled = true;
                StartStopAutoTimer(); // no direct start as the checkbox might not be active
                StartStopAutoProgressTimer(); // no direct start as checkbox might not be active
            } else {
                LOG.Add("STARTSTOPTIMER: STOPPING", 3);
                tAutoProgressTimer.Stop();
                StopAutoTimer();
                tTickTimer.Enabled = false;
                tClickTimer.Stop();
            }
        }
        
        /// <summary>
        /// Start and Stop the AutoClick timer manually
        /// </summary>
        void StartStopAutoTimer()
        {
            LOG.Add("STARTSTOPAUTOTIMER: Wrapper 1", 4);
            StartStopAutoTimer(new object(), new EventArgs(), false);
        }
        
        /// <summary>
        /// Start and Stop the AutoClick timer called from event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartStopAutoTimer(object sender, EventArgs e)
        {
            LOG.Add("STARTSTOPAUTOTIMER: Wrapper 2", 4);
            StartStopAutoTimer(sender, e, true);
        }
        
        /// <summary>
        /// Start and Stop the AutoClick timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="data">True when sent from an event handler</param>
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
        
        /// <summary>
        /// Start and Stop the AutoClick timer manually
        /// </summary>
        void StartStopAutoProgressTimer()
        {
            LOG.Add("STARTSTOPAUTOPROGRESSTIMER: Wrapper 1", 4);
            StartStopAutoProgressTimer(new object(), new EventArgs(), false);
        }
        
        /// <summary>
        /// Start and Stop the AutoClick timer called from event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartStopAutoProgressTimer(object sender, EventArgs e)
        {
            LOG.Add("STARTSTOPAUTOPROGRESSTIMER: Wrapper 2", 4);
            StartStopAutoProgressTimer(sender, e, true);
        }
        
        /// <summary>
        /// Start and Stop the AutoClick timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="data">True when sent from an event handler</param>
        void StartStopAutoProgressTimer(object sender, EventArgs e, bool data)
        {
            if (checkBoxAutoProgress.Checked) {
                LOG.Add("STARTSTOPAUTOPROGRESSTIMER: STARTING", 3);
                tAutoProgressTimer.Start();
            } else {
                LOG.Add("STARTSTOPAUTOPROGRESSTIMER: STOPPING", 3);
                tAutoProgressTimer.Stop();
            }
        }
        
        /// <summary>
        /// Start the AutoClickTimer and disable PreserveComboTimer
        /// </summary>
        void StartAutoTimer()
        {
            tPreserveComboTimer.Stop();
            tAutoClickTimer.Start();
            starttAutoTimer = DateTime.Now;
            tForcedDelayAutoClickTimer.Start();
            labelAutoClicker.Visible = true;
            checkBoxAutoClicker.Checked = true;
            bForceWait = true; // enable to force a wait on next switch no matter the current state
        }
        
        /// <summary>
        /// Stop the AutoClickTimer and enable PreserveComboTimer
        /// </summary>
        void StopAutoTimer()
        {
            labelAutoClicker.Visible = false;
            checkBoxAutoClicker.Checked = false;
            tForcedDelayWaitAutoClickTimer.Stop();
            tForcedDelayAutoClickTimer.Stop();
            tAutoClickTimer.Stop();
            // needs to be stopped if TimerActive gets disabled and started when TimerActive is enabled
            if (checkBoxTimerActive.Checked) {
                tPreserveComboTimer.Start();
            } else {
                tPreserveComboTimer.Stop();
            }
        }
        
#region MOUSE_EVENT
        /// <summary>
        /// stuff to send input to other window
        /// </summary>
        
        /// <summary>
        /// Get window handle dimensions absolute to desktop location
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpRect"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Get window handle top left coordinates on desktop
        /// </summary>
        /// <param name="processname">Window name to look for</param>
        /// <param name="handle">REF window handle</param>
        /// <param name="po">REF x,y coordinates</param>
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
        
        /// <summary>
        /// Mouse Events
        /// </summary>
        
        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
        
        /// <summary>
        /// Set handle as active window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);
        
        /// <summary>
        /// Send a mouse event at provided coordinates
        /// </summary>
        /// <param name="dwFlags">Mouse event</param>
        /// <param name="dx">X</param>
        /// <param name="dy">Y</param>
        /// <param name="dwData"></param>
        /// <param name="dwExtraInfo"></param>
        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);
        
        /// <summary>
        /// Get coordinates relative to window handle and execute mouse event at provided coordinates without handle information
        /// </summary>
        /// <param name="pStart">absolute window coordinates</param>
        /// <param name="pLocation">relative click action coordinates in relation to window</param>
        public void go(Point pStart, Point pLocation)
        {
            LOG.Add("GO: Wrapper", 4);
            go(pStart, pLocation, new IntPtr(-1));
        }
        
        /// <summary>
        /// Get coordinates relative to window handle and execute mouse event at provided coordinates
        /// </summary>
        /// <param name="pStart">absolute window coordinates</param>
        /// <param name="pLocation">relative click action coordinates in relation to window</param>
        /// <param name="handle">window handle</param>
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
        
        /// <summary>
        /// Findow window handle
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        /// <summary>
        /// Send event directly to a handle
        /// </summary>
        /// <param name="hWnd">handle</param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Send background action to inactive window
        /// </summary>
        /// <param name="wndName">window name</param>
        /// <param name="button">mousebutton</param>
        /// <param name="x">X inside window</param>
        /// <param name="y">Y inside window</param>
        /// <param name="doubleklick">perform a doubleclick</param>
        public void ControlClickWindow(string wndName, string button, int x, int y, bool doubleklick)
        {
            IntPtr hWnd = FindWindow(null, wndName);
            int LParam = MakeLParam(x, y);
            
            int btnDown = 0;
            int btnUp = 0;
            
            if (button == "left") {
                btnDown = (int)WMessages.WM_LBUTTONDOWN;
                btnUp = (int)WMessages.WM_LBUTTONUP;
            }
            
            if (button == "right") {
                btnDown = (int)WMessages.WM_RBUTTONDOWN;
                btnUp = (int)WMessages.WM_RBUTTONUP;
            }


            if (doubleklick == true) {
                _SendMessage(hWnd, btnDown, 0, LParam);
                _SendMessage(hWnd, btnUp, 0, LParam);
                _SendMessage(hWnd, btnDown, 0, LParam);
                _SendMessage(hWnd, btnUp, 0, LParam);
            }

            if (doubleklick == false) {
                _SendMessage(hWnd, btnDown, 0, LParam);
                _SendMessage(hWnd, btnUp, 0, LParam);
            }
               
        }
#endregion SEND_INPUT
#region POSTMESSAGE
        
        /// <summary>
        /// Post a message to inactive window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_SYSKEYUP = 0x0105;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int VK_CONTROL = 0x11;
        
        /// <summary>
        /// Post a message to inactive window
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="p"></param>
        public void _PostMessage(IntPtr handle, Point p)
        {
            // TODO: put the check somewhere else?
            if (checkBoxCTRLClick.Checked)
                PostMessage(handle, WM_SYSKEYDOWN, VK_CONTROL, 0);
            
            PostMessage(handle, WM_LBUTTONDOWN, 1, MakeLParam(p.X, p.Y));
            PostMessage(handle, WM_LBUTTONUP, 1, MakeLParam(p.X, p.Y));
            
            // TODO: put the check somewhere else?
            if (checkBoxCTRLClick.Checked)
                PostMessage(handle, WM_SYSKEYUP, VK_CONTROL, 0); 
        }
#endregion POSTMESSAGE
        
        /// <summary>
        /// Display a message in the Error box
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Unhook everything on Form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ghkAutoCLicker.Unregister())
                MessageBox.Show("Error while unregistering hotkey.",
                                "AutoClicker Information", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
        }
        
        /// <summary>
        /// Switch click locations to either include or exclude borders
        /// </summary>
        void CheckBoxBackgroundCheckedChanged()
        {
            CheckBoxBackgroundCheckedChanged(new object(), new EventArgs());
        }
        
        /// <summary>
        /// Switch click locations to either include or exclude borders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckBoxBackgroundCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxBackground.Checked) {
                clicks = new ClickLocations(true);
            } else {
                clicks = new ClickLocations(false);
            }
        }
        
        /// <summary>
        /// Switch wait delay between AutoClicker on or off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckBoxWaitDelayCheckedChanged(object sender, EventArgs e)
        {//FIXME: wait delay still occuring after stop/start
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
    
    /// <summary>
    /// Debug Logging class
    /// </summary>
    public static class LOG
    {
        /// <summary>
        /// Add a message to the log
        /// </summary>
        /// <param name="msg">Message to add</param>
        public static void Add(string msg)
        {
            Add(msg, 0);
        }
        
        /// <summary>
        /// Add a message to the log
        /// </summary>
        /// <param name="msg">Message to add</param>
        /// <param name="level">Debug level</param>
        public static void Add(string msg, int level)
        {
            Debug.WriteLineIf((GlobalVar.DEBUG && level <= GlobalVar.DEBUGLEVEL), DateTime.Now.ToString("HH\\:mm\\:ss\\.fff") + " " + msg);
        }
    }
    
    /// <summary>
    /// Click locations
    /// </summary>
    public class ClickLocations
    {
        // active window clicks
        /// <summary>
        /// Clickable location 1
        /// </summary>
        public Point clicker1 { get; set; }
        /// <summary>
        /// Clickable location 2
        /// </summary>
        public Point clicker2 { get; set; }
        /// <summary>
        /// Clickable location 3
        /// </summary>
        public Point clicker3 { get; set; }
        /// <summary>
        /// Clickable location 4
        /// </summary>
        public Point clicker4 { get; set; }
        /// <summary>
        /// Clickable location 5
        /// </summary>
        public Point clicker5 { get; set; }
        /// <summary>
        /// Clickable location 6
        /// </summary>
        public Point clicker6 { get; set; }
        /// <summary>
        /// Level up button location 1
        /// </summary>
        public Point level1 { get; set; }
        /// <summary>
        /// Level up button location 2
        /// </summary>
        public Point level2 { get; set; }
        /// <summary>
        /// Level up button location 3
        /// </summary>
        public Point level3 { get; set; }
        /// <summary>
        /// Level up button location 4
        /// </summary>
        public Point level4 { get; set; }
        /// <summary>
        /// AutoClick location including power up
        /// </summary>
        public Point clickerAuto { get; set; }
        /// <summary>
        /// Autp progression button location
        /// </summary>
        public Point autoProgress { get; set; }
        
        /// <summary>
        /// Set click locations to include or exclude borders
        /// </summary>
        /// <param name="background">True if borders are excluded</param>
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
                autoProgress = new Point(1120, 255);
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
                autoProgress = new Point(1125, 280);
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
