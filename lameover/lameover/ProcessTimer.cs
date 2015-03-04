using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lameover
{
    public class ProcessTimer
    {
        public const uint IntervalSeconds = 5;
        private MainWindow mainWindow;

        public delegate void GetDiversionsCallback();
        public delegate void AddTimeCallback(List<string> processes);

        public ProcessTimer(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void InfiniteLoop()
        {
            while (true)
            {
                Thread.Sleep((int)(IntervalSeconds * 1000));

                List<string> blockedProcesses = mainWindow.BlockedDiversions.GetDiversionsLowerCase();
                List<string> processes = Process.GetProcesses().Select(p => p.ProcessName.ToLower()).ToList();
                var activeProcesses = processes.Intersect(blockedProcesses).ToList();
                
                mainWindow.BlockedDiversions.AddTime(activeProcesses, 5);
            }
        }
    }
}
