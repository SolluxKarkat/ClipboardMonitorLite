using System;
using System.Windows.Forms;

namespace ClipboardMonitorLite
{
    public class ClipboardCleaner
    {
        private ClipboardManager _clipManager;
        private int clearInterval;
        private Timer Timer;
        public ClipboardCleaner(ClipboardManager Clip, Timer timer)
        {
            _clipManager = Clip;
            Timer = timer;
        }

        public int ClearInterval
        {
            get
            {
                return clearInterval;
            }
            set
            {
                clearInterval = value;
                StartTimer(value);
            }
        }

        private void StartTimer(int time)
        {
            if (!time.Equals(0))
            {
                Timer.Interval = time * 6000; // Convert from minutes to milliseconds
                Timer.Start();
                Timer.Tick += OnElapsed;
            }
            else
                Timer.Stop();
        }
        private void OnElapsed(object sender, EventArgs e)
        {
            _clipManager.ClearHistory();
        }
    }
}
