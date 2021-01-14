using System.Windows;
using WhatsAppMessage.Data;
using MahApps.Metro.Controls;
using System.Data;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Win32;
using System.Windows.Controls;
using static WhatsAppMessage.Data.WhatsAppViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WhatsAppMessage.Sender;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace WhatsAppMessage
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

            //テスト用の電卓計算
            //startCalc();

            /*
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
                WhatsAppMessageSender wams = new WhatsAppMessageSender();
                var process = Process.Start("C:\\Users\\ko198\\AppData\\Local\\WhatsApp\\WhatsApp.exe");

                foreach (string sendTo in sendList)
                {
                    wams.sendMessage(sendTo, message);
                }
                MessageBox.Show("Complete");
            }
            */

            // チェックボックスが押下されていることを確認
            if (!((bool)this.chk1.IsChecked && (bool)this.chk2.IsChecked))
            {
                MessageBox.Show("Please check confirmation before sending");
                return;
            }

            // 送信するかどうかの最終確認
            MessageBoxResult result = MessageBox.Show("Are you sure to send ?",
                "Confirmation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            // 送信するとなった場合
            if (result == MessageBoxResult.OK)
            {
                // メッセージボックスの中身を取得
                string message = this.WhatsAppMessage.Text;
                message = message.Replace("\r\n", "%0d%0a");    // 改行変換
                message = message.Replace(" ", "%20");          // 半角スペース変換
                message = message.Replace("　", "%E3%80%80");   // 全角スペース変換

                // チェックがついている宛先を宛先リストとして保存
                List<string> sendList = new List<string>();
                ObservableCollection<PhoneNumViewModel> customerList = (ObservableCollection<PhoneNumViewModel>)this.dataGrid.ItemsSource;

                // WhatsAppプロセスを起動
                var processWA = new Process();

                int index = 0;
                // 一斉送信開始
                foreach (PhoneNumViewModel record in customerList)
                {
                    // チェックがついている宛先のみ処理
                    if (record.IsChecked)
                    {
                        string url = "https://api.whatsapp.com/send?phone=" + record.PhoneNumber + "&text=" + message;

                        // Webブラウザ起動
                        var processWB = Process.Start(@"chrome.exe", url);
                        Thread.Sleep(4000);
                        //processWB.WaitForExit(8000);
                        processWB.CloseMainWindow(); // Webブラウザを閉じる

                        // WhatsAppアプリに移行できたかどうか
                        bool findFlg = false;
                        
                        try
                        {
                            Thread.Sleep(4000);

                            // UWPだといろいろあるようなので、ProcessをタイトルからWhatsAppを探す
                            foreach (var p in Process.GetProcesses())
                            {
                                if (p.MainWindowTitle.Equals("WhatsApp"))
                                {

                                    processWA = p;
                                    findFlg = true;
                                    break;
                                }
                            }

                            // WhatsAppが見つからなかったときはNGにする
                            if (!findFlg)
                            {
                                customerList[index].Judge = "NG";
                                this.dataGrid.ItemsSource = customerList;
                                this.dataGrid.Items.Refresh();
                            }

                            // エラーが起きた時の処理を実装 
                            try
                            {
                                var mainForm = AutomationElement.FromHandle(processWA.MainWindowHandle);

                                Thread.Sleep(3000); // OKのエレメントを取得するまで待ち時間を設定
                                //var btnOK = mainForm.FindFirst(TreeScope.Descendants,
                                //   new PropertyCondition(AutomationElement.NameProperty, "OK"));

                                // タスク設定し、４秒間応答がなければ正常終了していることとする
                                try
                                {
                                    /*
                                    var btnOK = Task.Run(() => FindButtonsByName(mainForm, "OK").First()
                                    .GetCurrentPattern(InvokePattern.Pattern) as InvokePattern);
                                    if (btnOK.Wait(TimeSpan.FromSeconds(4)))
                                    {
                                        customerList[index].Judge = "NG";
                                        this.dataGrid.ItemsSource = customerList;
                                        this.dataGrid.Items.Refresh();
                                    }
                                    */
                                    var btnOK = Task.Run(() => mainForm.FindFirst(TreeScope.Descendants,
                                       new PropertyCondition(AutomationElement.NameProperty, "OK")));
                                    if (btnOK.Wait(TimeSpan.FromSeconds(4)))
                                    {
                                        customerList[index].Judge = "NG";
                                        this.dataGrid.ItemsSource = customerList;
                                        this.dataGrid.Items.Refresh();
                                        processWA.CloseMainWindow();
                                    }
                                }
                                catch
                                {
                                    // 処理継続
                                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                                }
                                finally
                                {
                                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                                }
                            }
                            catch(Exception ex)
                            {
                                // 処理継続
                            }
                            finally
                            {
                                // 送信するためのエンターキーを押下
                                Thread.Sleep(1000);
                                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                            }
                        }
                        finally
                        {
                            Thread.Sleep(1000);
                            System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                            
                        }
                    }
                    index++;
                }
                try
                {
                    processWA.CloseMainWindow();
                }
                catch
                {
                    // 処理継続
                }
                            
                // 処理終了メッセージ表示
                MessageBox.Show("Complete");
            }

           
            //var processWB = Process.Start(@"chrome.exe", "https://api.whatsapp.com/send?phone=+818053838690&text=test%0d%0atest");
            //var process = Process.Start("https://api.whatsapp.com/send?phone=+8180538300000008690&text=test%0d%0atest");
            
            
            //Thread.Sleep(4000);
            //processWB.CloseMainWindow();
            //var processWA = new Process();
            //try
            //{
            //    Thread.Sleep(4000);

                // UWPだといろいろあるようなので、Processをタイトルから取り直す
            //    foreach (var p in Process.GetProcesses())
            //    {
            //        if (p.MainWindowTitle.Equals("WhatsApp"))
            //        {

            //            processWA = p;
            //            break;
            //        }
            //    }
            //    var mainForm = AutomationElement.FromHandle(processWA.MainWindowHandle);
                /*
                var btnOK = mainForm.FindFirst(TreeScope.Descendants,
                   new PropertyCondition(AutomationElement.NameProperty, "OK"));
                */
            //    System.Windows.Forms.SendKeys.SendWait("{ENTER}");


            //}
            //finally
            //{
            //    Thread.Sleep(2000);
            //    processWA.CloseMainWindow();


            //}


        }

        public static DataTable DataViewAsDataTable(DataView dv)
        {
            DataTable dt = dv.Table.Clone();
            foreach (DataRowView drv in dv)
                dt.ImportRow(drv.Row);
            return dt;
        }



        private static void startCalc()
        {
            var process = Process.Start("calc");
            try
            {
                Thread.Sleep(1000);

                // UWPだといろいろあるようなので、Processをタイトルから取り直す
                foreach (var p in Process.GetProcesses())
                {
                    if (p.MainWindowTitle.Contains("電卓"))
                    {
                        process = p;
                        break;
                    }
                }

                var mainForm = AutomationElement.FromHandle(process.MainWindowHandle);
                var btnClear = FindElementsByName(mainForm, "クリア").First()
                    .GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                btnClear.Invoke();

                var btn7 = FindButtonsByName(mainForm, "7").First()
                    .GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;

                foreach (var _ in Enumerable.Repeat(0, 7))
                {
                    btn7.Invoke();
                }

                var resultArea = FindElementById(mainForm, "CalculatorResults");
                var EXPECTED_VALUE = "表示は 7,777,777 です";
                var actualValue = resultArea.Current.Name;
                Console.WriteLine("期待値 {0} に対して、結果値は {1} です", EXPECTED_VALUE, actualValue);
                Console.WriteLine("テスト結果は {0} です", EXPECTED_VALUE == actualValue ? "OK" : "NG");

                Console.ReadKey();
            }
            finally
            {
                process.CloseMainWindow();
            }
        }

        private static AutomationElement FindElementById(AutomationElement rootElement, string automationId)
          => rootElement.FindFirst(TreeScope.Element | TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));

        private static IEnumerable<AutomationElement> FindElementsByName(AutomationElement rootElement, string name)
            => rootElement.FindAll(TreeScope.Element | TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, name)).Cast<AutomationElement>();

        private static IEnumerable<AutomationElement> FindButtonsByName(AutomationElement rootElement, string name)
            => FindElementsByName(rootElement, name).Where(x => x.Current.ClassName == "Button");

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        
    }
}
