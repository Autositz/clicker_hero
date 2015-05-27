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

namespace clicker_hero
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        
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
        /// Background timer to update tooltip info
        /// </summary>
        private System.Timers.Timer tClickTimer;
        private System.Windows.Forms.Timer tTickTimer;
        DateTime starttClickTimer = new DateTime();
        TimeSpan elapsedtClickTimer = new TimeSpan();
        private ClickLocations clicks;
        Stopwatch swTest = new Stopwatch();
        
#if DEBUG
        int iTimerHours = 0;
        int iTimerMinutes = 0;
        int iTimerSeconds = 20;
#else
        int iTimerHours = 0;
        int iTimerMinutes = 8;
        int iTimerSeconds = 0;
#endif
        
        public MainForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            LOG.Add("MAIN: Set ClickTimer");
            tClickTimer = new System.Timers.Timer();
            SetNewTimer();
            tClickTimer.AutoReset = true;
            tClickTimer.Elapsed += new ElapsedEventHandler(CheckNextTime);
            LOG.Add("MAIN: Set TickTimer");
            tTickTimer = new System.Windows.Forms.Timer();
            tTickTimer.Tick += new EventHandler(tTickTimer_Tick);
            tTickTimer.Interval = 1;
            tTickTimer.Enabled = true;
            
            StartStopTimer();

            
//            labelTimer.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2"));
            textBox1.Text = iTimerHours.ToString();
            textBox2.Text = iTimerMinutes.ToString();
            textBox3.Text = iTimerSeconds.ToString();
            
            clicks = new ClickLocations();
            DoIt();
        }

        private async void DoIt()
        {
            LOG.Add("DOIT: Start");
            swTest.Restart();
            starttClickTimer = DateTime.Now;
            
            foreach (Process p in Process.GetProcessesByName("clicker heroes")) {
                IntPtr handle = p.MainWindowHandle;
                RECT rct = new RECT();
                GetWindowRect(handle, ref rct);
                Point po = new Point(rct.Left, rct.Top);
                
                if (checkBoxClickables.Checked) {
                    LOG.Add("DOIT: Doing Clickables");
                    test1.go(po, clicks.clicker1, handle);
                    await Wait(150);
                    test1.go(po, clicks.clicker2, handle);
                    await Wait(150);
                    test1.go(po, clicks.clicker3, handle);
                    await Wait(150);
                    test1.go(po, clicks.clicker4, handle);
                    await Wait(150);
                    test1.go(po, clicks.clicker5, handle);
                    await Wait(150);
                    test1.go(po, clicks.clicker6, handle);
                    await Wait(150);
                    LOG.Add("DOIT: Done Clickables");
                }
                
                if (checkBoxHeroLevel1.Checked)
                {
                    LOG.Add("DOIT: Hero Level 1");
                    test1.go(po, clicks.level1, handle);
                    await Wait(150);
                }
                if (checkBoxHeroLevel2.Checked)
                {
                    LOG.Add("DOIT: Hero Level 2");
                    test1.go(po, clicks.level2, handle);
                    await Wait(150);
                }
                if (checkBoxHeroLevel3.Checked)
                {
                    LOG.Add("DOIT: Hero Level 3");
                    test1.go(po, clicks.level3, handle);
                    await Wait(150);
                }
                if (checkBoxHeroLevel4.Checked) {
                    LOG.Add("DOIT: Hero Level 4");
                    test1.go(po, clicks.level4, handle);
                    await Wait(150);
                }
            }
            
            
        }
        
        async Task Wait(int waittime)
        {
            await Task.Delay(waittime);
        }
        
        public void CheckNextTime(object sender, EventArgs e)
        {
            LOG.Add("CHECKNEXTTIME: Call from timer");
            DoIt();
        }
        
        private void tTickTimer_Tick(object sender, EventArgs e)
        {
            elapsedtClickTimer = DateTime.Now - starttClickTimer;
//            labelElapsed.Text = swTest.Elapsed.ToString();
            labelElapsed.Text = elapsedtClickTimer.ToString("hh\\:mm\\:ss\\.fff");
        }
        
        void TimerChanged(object sender, EventArgs e)
        {
            LOG.Add("TIMERCHANGED: Timer has changed");
            SetTimer();
        }
        
        void SetTimer()
        {
            LOG.Add("SETTIMER: Parsing Textbox to variables");
            int i;
            if (int.TryParse(textBox1.Text, out i))
            {
                iTimerHours = i;
            }
            if (int.TryParse(textBox2.Text, out i))
            {
                iTimerMinutes = i;
            }
            if (int.TryParse(textBox3.Text, out i))
            {
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
            
            LOG.Add("SETNEWTIMER: New timer set to " + iTime + " sec");
            tClickTimer.Interval = 1000 * iTime;
            starttClickTimer = DateTime.Now;
//            labelTimerSet.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2")) + Environment.NewLine + tClickTimer.Interval;
            labelTimerSet.Text = string.Format("{0}:{1}:{2}", iTimerHours.ToString("D2"), iTimerMinutes.ToString("D2"), iTimerSeconds.ToString("D2"));
        }
        
        void StartStopTimer()
        {
            LOG.Add("STARTSTOPTIMER: Wrapper 1");
            StartStopTimer(new object(), new EventArgs(), false);
        }
        
        void StartStopTimer(object sender, EventArgs e)
        {
            LOG.Add("STARTSTOPTIMER: Wrapper 2");
            StartStopTimer(sender, e, true);
        }
        
        void StartStopTimer(object sender, EventArgs e, bool data)
        {
            if (checkBoxTimerActive.Checked)
            {
                LOG.Add("STARTSTOPTIMER: STARTING");
                tClickTimer.Start();
                starttClickTimer = DateTime.Now;
                tTickTimer.Enabled = true;
                
            } else
            {
                LOG.Add("STARTSTOPTIMER: STOPPING");
                tTickTimer.Enabled = false;
                tClickTimer.Stop();
            }
        }
    }
    
    public static class test1
    {
        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
        
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);
        
        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);
        
        public static void go(Point pStart, Point pLocation)
        {
            LOG.Add("GO: Wrapper");
            go(pStart, pLocation, new IntPtr(-1));
        }
        
        public static void go(Point pStart, Point pLocation, IntPtr handle)
        {
            Point p = new Point(Convert.ToInt32(pStart.X + pLocation.X), Convert.ToInt32(pStart.Y + pLocation.Y));
            
            if (handle.ToInt32() != -1)
            {
                LOG.Add("GO: Bringing app to front");
                SetForegroundWindow(handle.ToInt32());
            }
            LOG.Add("GO: Moving Cursor to: " + p.ToString());
            Cursor.Position = p;
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new IntPtr());
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new IntPtr());
        }
    }
    
    public static class LOG
    {
        public static void Add(string msg)
        {
            Debug.WriteLineIf(GlobalVar.DEBUG, DateTime.Now.ToString("HH\\:mm\\:ss\\.fff") + " " + msg);
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
    }
    
        
    /// <summary>
    /// Holds static global variables
    /// </summary>
    public static class GlobalVar
    {
        public const bool DEBUG = true; // enable or disable debug messages
    }
}
