using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lameover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Diversions BlockedDiversions = new Diversions();

        public Thread worker;

        public MainWindow()
        {
            InitializeComponent();

            ReadProcessList();

            blockList.ItemsSource = BlockedDiversions.Processes;

            StartProcessMonitor();
        }

        public void ReadProcessList()
        {
        	try
		    {
			    using (Stream stream = File.Open("blocklist.bin", FileMode.Open))
			    {
			        BinaryFormatter bin = new BinaryFormatter();
                    BlockedDiversions = (Diversions)bin.Deserialize(stream);
			    }
		    }
		    catch (IOException)
		    {
		    }
	    }

        public void SaveProcessList()
        {
            try
            {
                using (Stream stream = File.Open("blocklist.bin", FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, BlockedDiversions);
                }
            }
            catch (IOException)
            {
            }
        }

        public void StartProcessMonitor()
        {
            ProcessTimer timer = new ProcessTimer(this);

            worker = new Thread(new ThreadStart(timer.InfiniteLoop));
            worker.Start();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (worker != null)
            {
                worker.Abort();
            }

            base.OnClosing(e);
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            BlockedDiversions.AddDiversion(processTextBox.Text, 0);
            SaveProcessList();

            blockList.ItemsSource = BlockedDiversions.Processes;
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (blockList.SelectedIndex == -1)
            {
                return;
            }
            BlockedDiversions.RemoveDiversion(blockList.SelectedIndex);

            blockList.ItemsSource = BlockedDiversions.Processes;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           BlockedDiversions.SetMaxTime(uint.Parse(MaxTimeTextBox.Text));
        }
    }
}
