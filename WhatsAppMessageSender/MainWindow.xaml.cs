using System.Windows;
using WhatsAppMessageSender.Data;
using MahApps.Metro.Controls;
using System.Data;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Win32;
using System.Windows.Controls;
using static WhatsAppMessageSender.Data.WhatsAppViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WhatsAppMessageSender
{
    /// <summary>
    /// Interaction logic of MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary> CSV read button click event </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void read_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "";
            ofd.DefaultExt = "*.csv";
            if (ofd.ShowDialog() == false)
            {
                return;
            }
            this.dataGrid.DataContext = new WhatsAppViewModel(ofd.FileName);
        }

        /// <summary> send message button </summary>
        private void send_button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure to send ?",
                "Confirmation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                List<string> sendList = new List<string>();
                // OK case
                ObservableCollection<PhoneNumViewModel> phoneNumList = (ObservableCollection<PhoneNumViewModel>)this.dataGrid.ItemsSource;
                foreach (PhoneNumViewModel phoneNum in phoneNumList)
                {
                    if (phoneNum.IsChecked)
                    {
                        sendList.Add(phoneNum.PhoneNumber);
                    }
                }
                var message = this.WhatsAppMessage.Text;
            }

        }

        public static DataTable DataViewAsDataTable(DataView dv)
        {
            DataTable dt = dv.Table.Clone();
            foreach (DataRowView drv in dv)
                dt.ImportRow(drv.Row);
            return dt;
        }


    }
}
