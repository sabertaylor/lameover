using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public ObservableCollection<string> BlockedProcesses = new ObservableCollection<string>();
        public Thread worker;

        public MainWindow()
        {
            InitializeComponent();

            StartProcessMonitor();
        }

        public void StartProcessMonitor()
        {
            ProcessTimer timer = new ProcessTimer();

            worker = new Thread(timer.InfiniteLoop);
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
            blockList.Items.Add(processTextBox.Text);
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            blockList.Items.RemoveAt(blockList.SelectedIndex);
        }
    }
}
