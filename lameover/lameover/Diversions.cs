using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace lameover
{
    public class Diversions
    {
        [XmlArrayItem("Process")]
        public ObservableCollection<Diversion> Processes;
        [XmlElement]
        public uint NagIntervalInMinutes = 30;

        [XmlElement]
        public DateTime Today { get; set; }

        public Diversions()
        {
            Processes = new ObservableCollection<Diversion>();
        }

        public void AddDiversion(string process, uint seconds)
        {
            bool totalPresent = Processes.Count(p => p.Process == "Total") > 0;

            if (process == "Total" && totalPresent)
            {
                // 1 Total is enough.
                return;
            }

            // Some logic for keeping "Total" at end of the collection.
            lock (this)
            {
                var diversion = new Diversion()
                {
                    Parent = this,
                    Process = process,
                    ElapsedSeconds = seconds
                };

                if (process == "Total")
                {
                    Processes.Add(diversion);
                }
                else
                {
                    if (Processes.Count == 0)
                    {
                        Processes.Add(diversion);
                    }
                    else
                    {
                        if (totalPresent)
                        {
                            Processes.Insert(Processes.Count - 1, diversion);
                        }
                        else
                        {
                            Processes.Add(diversion);
                        }
                    }
                }
            }
        }

        public void RemoveDiversion(int index)
        {
            if (index == Processes.Count - 1)
            {
                // Can't remove the "Total"
                return;
            }

            lock (this)
            {
                Processes.RemoveAt(index);
            }
        }

        public void SetMaxTime(uint minutes)
        {
            lock (this)
            {
                NagIntervalInMinutes = minutes;
            }
        }

        public void AddTime(List<string> process, uint seconds)
        {
            lock (this)
            {
                if (ResetIfNewDay())
                {
                    return;
                }

                foreach (Diversion diversion in Processes)
                {
                    foreach (string name in process)
                    {
                        if (diversion.Process.ToLower() == name.ToLower())
                        {
                            diversion.AddSeconds(seconds);
                        }
                    }
                }
            }
        }

        public List<string> GetDiversionsLowerCase()
        {
            lock (this)
            {
                return Processes.Select(diversion => diversion.Process.ToLower()).ToList();
            }
        }

        /// <param name="now">DateTime without the Time part.</param>
        /// <returns>True if now is over a day further.</returns>
        public bool ResetIfNewDay()
        {
            DateTime now = DateTime.Now.Date;

            if ((now - Today).TotalDays >= 1)
            {
                ClearClocks();

                Today = now;
                return true;
            }
            return false;
        }

        private void ClearClocks()
        {
            foreach (var process in Processes)
            {
                process.ElapsedSeconds = 0;
            }
        }
    }
}
