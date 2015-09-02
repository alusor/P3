using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using MahApps.Metro;
using MahApps.Metro.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;

namespace MonitorProcesos
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 
    public class proceso {
        public int ID { get; set; }
        public string nombre { get; set; }
        public string estado { get; set; }
        public double memoria { get; set; }
    }
    public partial class MainWindow : MetroWindow
    {
        ObservableCollection<proceso> process = new ObservableCollection<proceso>();
        bool stop = false;
        List<proceso> h = new List<proceso>();
        Thread _main;
        List<Thread> thl = new List<Thread>();
        int id;
        int index=0;
        proceso a = new proceso();
        public MainWindow()
        {
            
            InitializeComponent();
            _main = new Thread(() => Manager());
            _main.Start();
        }
        public delegate void UpdateGridCallback();
        public delegate void addDataCallback(proceso p);
        public void addData(proceso p) {
            process.Add(p);
        }
        public void UpdateGrid() {
            procesView.ItemsSource = h;
            procesView.ItemsSource = process;
        }

        private void IndependientProcess()
        {
            proceso p = new proceso {
                ID = Thread.CurrentThread.ManagedThreadId,
                nombre = Thread.CurrentThread.GetApartmentState().ToString(),
                estado = "Iniciando", memoria = 0 };
            try
            {
                Dispatcher.Invoke(new addDataCallback(this.addData),p);
                int lenght = process.Count - 1;
                process[lenght].memoria = 0;
                Random r = new Random();
                Thread.Sleep(r.Next(1000, 3000));
                process[lenght].estado = "Ejecutando";
                while (true)
                {
                    

                    Thread.Sleep(r.Next(1000, 3000));
                    ++process[lenght].memoria;
                }
            }
            catch (InvalidOperationException e)
            {
                Console.Write(e);
            }
            
        }
        private void viewUpdater() {
            while (true) {
                Thread.Sleep(500);
                procesView.Dispatcher.Invoke(new UpdateGridCallback(this.UpdateGrid), DispatcherPriority.Normal);

            }

        }


        private void Manager() {
            Random r = new Random();
            Thread upda = new Thread(viewUpdater);
            upda.Start();
            while (true) {
                Thread.Sleep(r.Next(1000,30000));
                Thread pro = new Thread(IndependientProcess);
                thl.Add(pro);
                pro.Start(); 
            }
        }
        private void btnINI_Click(object sender, RoutedEventArgs e)
        {
            Thread pro = new Thread(IndependientProcess);
            thl.Add(pro);
            pro.Start();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
            
        }

        async private void about_Click(object sender, RoutedEventArgs e)
        {
            await this.ShowMessageAsync("Acerca de", "Eduardo Velez Santigo\nWPF  .NET 4.5\neduardo@alusorstroke.com\nMonitor de  procesos.");
        }

        private void btnTerminar_Click(object sender, RoutedEventArgs e)
        {
            
            
            for (int i = 0; i < thl.Count; i++)
            {
                if (thl[i].ManagedThreadId == process[index].ID)
                {
                    if (!(thl[i].ThreadState == ThreadState.Suspended)) {
                        process[index].estado = "Terminado...";
                        thl[i].Abort();
                        thl.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void btnPausa_Click(object sender, RoutedEventArgs e)
        {
            if (!process[index].estado.Equals("Terminado...")) {
                process[index].estado = "Pausado";
                for (int i = 0; i < thl.Count; i++)
                {
                    if (thl[i].ManagedThreadId == process[index].ID)
                    {
                        thl[i].Suspend();
                    }
                }
            }
        }

        private void procesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            a = (proceso)procesView.CurrentItem;
            id = a.ID;
            index = process.IndexOf(a);
            Console.WriteLine(index);
        }

        private void bntContinue_Click(object sender, RoutedEventArgs e)
        {
            if (process[index].estado.Equals("Pausado")) {
                process[index].estado = "Ejecutando";
                for (int i = 0; i < thl.Count; i++)
                {
                    if (thl[i].ManagedThreadId == process[index].ID)
                    {
                        thl[i].Resume();
                    }
                }
            }
        }
    }
}
