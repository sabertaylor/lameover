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
        private int usedSeconds = 0;

        public void InfiniteLoop()
        {
            while (true)
            {
                Thread.Sleep(5000);

                Process[] processes = Process.GetProcesses().OrderBy(p => p.ProcessName).ToArray();

                string blargh = string.Empty;
                for (int i = 0; i < processes.Count(); i++)
                {
                    Process process = processes[i];
                    blargh += string.Format("process: {0},  id: {1}{2}", process.ProcessName, process.Id, Environment.NewLine);
                }

                System.Windows.MessageBox.Show(blargh);
            }
        }
    }
}
