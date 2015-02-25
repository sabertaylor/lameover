using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lameover
{
    public class Diversions
    {
        public ObservableCollection<Diversion> Processes;
        public uint MaxMinutes = 60;
        
        public Diversions()
        {
            Processes = new ObservableCollection<Diversion>()
            {
                new Diversion()
                {
                    Process = "Total",
                    Parent = this
                }            
            };
        }

        public void AddDiversion(string process, uint seconds)
        {
            lock (this)
            {
                Processes.Insert(Processes.Count - 1, new Diversion()
                {
                    Parent = this,
                    Process = process,
                    ElapsedSeconds = seconds
                });
            }
        }

        public void RemoveDiversion(int index)
        {
            lock (this)
            {
                Processes.RemoveAt(index);
            }
        }

        public void SetMaxTime(uint minutes)
        {
            lock (this)
            {
                MaxMinutes = minutes;
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
                        if (diversion.Process == name)
                        {
                            diversion.ElapsedSeconds += seconds;
                        }
                    }
                }
            }
        }

        public List<string> GetDiversions()
        {
            lock (this)
            {
                return Processes.Select(diversion => diversion.Process).ToList();
            }
        }
    }

    public class Diversion : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Diversions Parent;
        public string Process { get; set; }
        public uint Completion
        {
            set
            {
                ElapsedSeconds = 1;
            }
            get
            {
                return (uint)(100.0 * ElapsedSeconds / 60 / Parent.MaxMinutes);
            }
        }
        private uint elapsedSeconds = 0;
        public uint ElapsedSeconds
        {
            get
            {
                return elapsedSeconds;
            }
            set
            {
                elapsedSeconds = value;

                if (Process == "Total")
                {
                    if (Completion >= 100)
                    {
                        if (ElapsedSeconds % 60 == 0)
                        {
                            if (!blewWhistle)
                            {
                                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"media\34600__reinsamba__sambawhistle1.wav");
                                player.Play();
                                blewWhistle = true;
                            }
                            else
                            {
                                System.Media.SystemSounds.Exclamation.Play();
                            }

                            System.Windows.MessageBox.Show("lameover says: You're over time for diversions.");
                        }
                    }
                }
                else
                {
                    uint totalElapsed = 0;
                    foreach (var diversion in Parent.Processes)
                    {
                        if (diversion.Process == "Total")
                        {
                            continue;
                        }
                        totalElapsed += diversion.elapsedSeconds;
                    }
                    Parent.Processes[Parent.Processes.Count - 1].ElapsedSeconds = totalElapsed;
                }

                OnPropertyChanged(new PropertyChangedEventArgs("Completion"));
            }
        }

        private bool blewWhistle = false;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}
