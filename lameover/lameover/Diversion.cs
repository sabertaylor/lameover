﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        public uint MaxMinutes = 60;

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
                        if (diversion.Process.ToLower() == name.ToLower())
                        {
                            diversion.ElapsedSeconds += seconds;
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
    }
    
    public class Diversion : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [XmlIgnore]
        public Diversions Parent;
        private string minutes = "0m";
        public string Process { get; set; }
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
                ElapsedSeconds = 1;
            }
            get
            {
                return (uint)(100.0 * ElapsedSeconds / 60 / Parent.MaxMinutes);
            }
        }
        private uint elapsedSeconds = 0;
        [XmlIgnore]
        public uint ElapsedSeconds
        {
            get
            {
                return elapsedSeconds;
            }
            set
            {
                elapsedSeconds = value;
                if (value == 0)
                {
                    // On deserialization.
                    return;
                }

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
                    // Set total time.
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

                string newMinutesUsed = string.Format("{0}m", (elapsedSeconds / 60));
                if (newMinutesUsed != MinutesUsed)
                {
                    MinutesUsed = newMinutesUsed;
                    OnPropertyChanged(new PropertyChangedEventArgs("MinutesUsed"));
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
