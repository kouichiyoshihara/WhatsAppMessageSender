using System.Windows;
using WhatsAppMessage.Data;
using MahApps.Metro.Controls;
using System.Data;
using System.Drawing;
using MessageBox = System.Windows.MessageBox;
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
using Microsoft.VisualBasic;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Management;
using System.Drawing.Imaging;
using OpenCvSharp;
using System.IO;

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

            // 設定ファイルを読み込みタイムアウト時間、ウェイト時間、クローズ時間を取得する
            string confFilePath = System.IO.Directory.GetCurrentDirectory();
            confFilePath += "\\config\\conf.csv";
            var timeConf = new Dictionary<string, int>();
            StreamReader sr = new StreamReader(@confFilePath);
            {
                // 末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // CSVファイルの一行を読み込む
                    string line = sr.ReadLine();
                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    string[] values = line.Split(',');

                    // 設定ファイルを格納する
                    if (values.Length >1)
                    {
                        timeConf.Add(values[0], int.Parse(values[1]));
                    }
                }
            }
            int BROWSER_TIMEOUT_MSEC = timeConf["BROWSER_TIMEOUT_MSEC"];                    // タイムアウト時間
            int WHATSAPP_START_WAITTIME_MSEC = timeConf["WHATSAPP_START_WAITTIME_MSEC"];    // ウェイト時間
            int WHATSAPP_CLOSE_WAITTIME_MSEC = timeConf["WHATSAPP_CLOSE_WAITTIME_MSEC"];    // クローズ時間

            // 送信するとなった場合
            if (result == MessageBoxResult.OK)
            {
                // メッセージボックスの中身を取得
                string message = this.WhatsAppMessage.Text;
                message = message.Replace("%", "%25");          // %変換

                message = message.Replace("%0d", "%250d");      // \rのインジェクションを防ぐ
                message = message.Replace("%0a", "%250a");      // \nのインジェクションを防ぐ
                message = message.Replace("%20", "%2520");      // 半角スペースのインジェクションを防ぐ
                message = message.Replace("%22", "%2522");      // "のインジェクションを防ぐ
                message = message.Replace("%23", "%2523");      // #のインジェクションを防ぐ
                message = message.Replace("%24", "%2524");      // $のインジェクションを防ぐ
                message = message.Replace("%26", "%2526");      // &のインジェクションを防ぐ
                message = message.Replace("%2C", "%252C");      // ,のインジェクションを防ぐ
                //message = message.Replace("|", "");           // |のインジェクションを防ぐ）
                message = message.Replace("%40", "%2540");      // @のインジェクションを防ぐ
                message = message.Replace("%2B", "%252B");      // +のインジェクションを防ぐ
                message = message.Replace("%3A", "%253A");      // :のインジェクションを防ぐ
                message = message.Replace("%3B", "%253B");      // ;のインジェクションを防ぐ
                message = message.Replace("%3D", "%253D");      // =のインジェクションを防ぐ
                message = message.Replace("%3F", "%253F");      // ?のインジェクションを防ぐ
                message = message.Replace("%2F", "%252F");      // /のインジェクションを防ぐ
                // 特殊文字変換
                message = message.Replace("\r\n", "%0d%0a");    // 改行変換
                message = message.Replace(" ", "%20");          // 半角スペース変換
                message = message.Replace("\"", "%22");         // "変換
                message = message.Replace("#", "%23");          // #変換
                message = message.Replace("$", "%24");          // $変換
                message = message.Replace("&", "%26");          // &変換
                message = message.Replace(",", "%2C");          // ,変換
                //message = message.Replace("|", "");             // |変換（使用不可）
                message = message.Replace("@", "%40");          // @変換
                message = message.Replace("+", "%2B");          // +変換
                message = message.Replace(":", "%3A");          // :変換
                message = message.Replace(";", "%3B");          // ;変換
                message = message.Replace("=", "%3D");          // =変換
                message = message.Replace("?", "%3F");          // ?変換
                message = message.Replace("/", "%2F");          // /変換

                // チェックがついている宛先を宛先リストとして保存
                List<string> sendList = new List<string>();
                ObservableCollection<PhoneNumViewModel> customerList = (ObservableCollection<PhoneNumViewModel>)this.dataGrid.ItemsSource;

                // インデックス
                int index = 0;
                
                // WhatsAppアプリに移行できたかどうか
                bool errFlg = false;

                // 一斉送信開始
                foreach (PhoneNumViewModel record in customerList)
                {
                    errFlg = false;

                    // WhatsAppプロセスを起動
                    var processWA = new Process();

                    // チェックがついている宛先のみ処理
                    if (record.IsChecked)
                    {
                        string url = "https://api.whatsapp.com/send?phone=" + record.PhoneNumber + "&text=" + message;

                        // Webブラウザ起動
                        var processWB = Process.Start(@"chrome.exe", url);
                        //Thread.Sleep(4000);
                        //processWB.WaitForExit(10000);       // タイムアウト時間
                        processWB.WaitForExit(BROWSER_TIMEOUT_MSEC);    // タイムアウト時間
                        processWB.CloseMainWindow();                    // Webブラウザを閉じる
                        
                        try
                        {
                            //Thread.Sleep(4000);             // ウェイト時間
                            Thread.Sleep(WHATSAPP_START_WAITTIME_MSEC); // ウェイト時間

                            // WhatsAppデスクトップアプリ（プロセス名：WhatsApp）をアクティブに
                            Process[] process = Process.GetProcessesByName("WhatsApp");
                            
                            if (process.Length > 0)
                            {
                                foreach (var p in process)
                                {
                                    if (p.MainWindowTitle.Equals("WhatsApp"))
                                    {
                                        Interaction.AppActivate(p.Id);
                                        processWA = p;
                                        break;
                                    }
                                }
                            }

                            // アクティブウィンドウのWhatsApp画面を保存する
                            string stCurrentDir = System.IO.Directory.GetCurrentDirectory();
                            string waImg = stCurrentDir + "\\wkConfig\\shot_raw.png";

                            RECT r;
                            IntPtr active = GetForegroundWindow();
                            GetWindowRect(active, out r);
                            Rectangle rect = new Rectangle(r.left, r.top, r.right - r.left, r.bottom - r.top);

                            Bitmap bmp = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                            }

                            bmp.Save(@waImg, ImageFormat.Png);

                            // 送信可能か判定
                            string templateImg = stCurrentDir + "\\wkConfig\\template\\template.png";
                            string templateImg2 = stCurrentDir + "\\wkConfig\\template\\template_2.png";
                            errFlg = (!judgeMatch(waImg, templateImg) && !judgeMatch(waImg, templateImg2));

                        }
                        finally
                        {
                            // errorが見つからなかったときはNGにする
                            if (errFlg)
                            {
                                customerList[index].Judge = "NG";
                                this.dataGrid.ItemsSource = customerList;
                            }
                            this.dataGrid.Items.Refresh();
                            Interaction.AppActivate(processWA.Id);
                            SendKeys.SendWait("{ENTER}");
                            // Thread.Sleep(1000);             // クローズ時間
                            Thread.Sleep(WHATSAPP_CLOSE_WAITTIME_MSEC); // クローズ時間
                            processWA.CloseMainWindow();

                        }
                    }
                    index++;
                }

                // 処理終了メッセージ表示
                //if (Environment.Is64BitProcess)
                //{
                //    MessageBox.Show("64bit");
                //}
                //else
                //{
                //    MessageBox.Show("32bit");
                //}
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

        // 画像マッチング
        private bool judgeMatch(string rawImgPath, string tempImgPath)
        {
            // 検索対象の画像
            using (Mat mat = new Mat(@rawImgPath))
            // テンプレート画像
            using (Mat temp = new Mat(@tempImgPath))
            
            using (Mat result = new Mat())
            {

                // テンプレートマッチ
                Cv2.MatchTemplate(mat, temp, result, TemplateMatchModes.CCoeffNormed);

                // 類似度が最大/最小となる画素の位置を調べる
                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;
                Cv2.MinMaxLoc(result, out minval, out maxval, out minloc, out maxloc);

                // しきい値で判断
                var threshold = 0.90;
                if (maxval >= threshold)
                {
                    return true;
                }
                else
                {
                    // 見つからない
                    return false;
                }
            }
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

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("User32.Dll")]
        static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        extern static IntPtr GetForegroundWindow();


        public static IEnumerable<Process> GetChildProcesses(Process process)
        {
            List<Process> children = new List<Process>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher(String.Format("Select * From Win32_Process Where ParentProcessID={0}", process.Id));

            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }

            return children;
        }

    }
}
