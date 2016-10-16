using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
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
        public bool CheckForNewDay()
        {
            DateTime now = DateTime.Now.Date;

            if ((now - Today).TotalDays >= 1)
            {
                NewDayReset();

                Today = now;
                return true;
            }
            return false;
        }

        private void NewDayReset()
        {
            foreach (var process in Processes)
            {
                process.ElapsedSeconds = 0;
            }
        }
    }
    
    public class Diversion : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        public Diversions Parent;

        [XmlElement]
        public string Process { get; set; }

        private string minutes = "0m";
        
        [XmlIgnore]
        public string MinutesUsed
        {
            get
            {
                return minutes;
            }
            set
            {
                minutes = value;
            }
        }
        
        [XmlIgnore]
        public uint Completion
        {
            set
            {
            }
            get
            {
                return (uint)(100.0 * ElapsedSeconds / 60 / Parent.NagIntervalInMinutes);
            }
        }

        private uint elapsedSeconds;
        [XmlElement]
        public uint ElapsedSeconds
        {
            get
            {
                return elapsedSeconds;
            }
            set
            {
                elapsedSeconds = value;
                string newMinutesUsed = string.Format("{0}m", (elapsedSeconds / 60));
                if (newMinutesUsed != MinutesUsed)
                {
                    MinutesUsed = newMinutesUsed;
                    OnPropertyChanged(new PropertyChangedEventArgs("MinutesUsed"));
                }

                OnPropertyChanged(new PropertyChangedEventArgs("Completion"));
            }
        }

        public void AddSeconds(uint seconds)
        {
            ElapsedSeconds += seconds;

            if (Parent.CheckForNewDay())
            {
                return;
            }

            if (Process == "Total")
            {
                if (Completion >= 100)
                {
                    if (ElapsedSeconds % 60 == 0)
                    {
                        NagTheUser();
                    }
                }
            }
            else
            {
                // "Total"
                Parent.Processes[Parent.Processes.Count - 1].AddSeconds(seconds);
            }
        }

        public void NagTheUser()
        {
            System.Media.SystemSounds.Beep.Play();

            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.Speak(string.Format("You have been using all watched processes for {0} minutes.", (int)(ElapsedSeconds / 60)));
            }
        }

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}
