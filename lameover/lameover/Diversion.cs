using System;
using System.ComponentModel;
using System.Speech.Synthesis;
using System.Xml.Serialization;

namespace lameover
{
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
                if (value != minutes)
                {
                    value = minutes;
                    OnPropertyChanged(new PropertyChangedEventArgs("MinutesUsed"));
                }
            }
        }

        private uint completion;
        [XmlIgnore]
        public uint Completion
        {
            get
            {
                return completion;
            }
            set
            {
                if (value != completion)
                {
                    value = completion;
                    OnPropertyChanged(new PropertyChangedEventArgs("Completion"));
                }
            }
        }

        [XmlIgnore]
        private uint completedIntervals
        {
            get
            {
                return (uint)(ElapsedSeconds / 60 / Parent.NagIntervalInMinutes);
            }
            set
            {
            }
        }

        [XmlIgnore]
        private string warningLevelColor;
        public string WarningLevelColor
        {
            get
            {
                return GetWarningColor();
            }
            set
            {
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
                MinutesUsed = string.Format("{0}m", (elapsedSeconds / 60));

                OnPropertyChanged(new PropertyChangedEventArgs("Completion"));
            }
        }

        public void AddSeconds(uint seconds)
        {
            uint oldCompletedIntervals = completedIntervals;
            ElapsedSeconds += seconds;

            Completion = (uint)((100.0 * ElapsedSeconds / 60 / Parent.NagIntervalInMinutes)) % 100;

            if (Process != "Total")
            {
                // Add to "Total"
                Parent.Processes[Parent.Processes.Count - 1].AddSeconds(seconds);
            }

            if (completedIntervals != oldCompletedIntervals)
            {
                OnPropertyChanged(new PropertyChangedEventArgs("WarningLevelColor"));
            }
            
            if (Process == "Total")
            {
                if (completedIntervals != oldCompletedIntervals)
                {
                    NagTheUser();
                }
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

        private string GetWarningColor()
        {
            switch (completedIntervals)
            {
                case 0 : return "Green";
                case 1 : return "Yellow";
                case 2 : return "Red";
                default : return "Black";
            };
        }
    }
}
