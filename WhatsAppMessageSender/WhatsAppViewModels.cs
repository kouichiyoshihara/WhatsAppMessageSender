using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WhatsAppMessage.Data
{
    /// <summary>Phone number class</summary>
    public class WhatsAppViewModel : INotifyPropertyChanged
    {

        public class PhoneNumViewModel : INotifyPropertyChanged
        {
            /// <summary>Number</summary>
            public string IndexNumber { get; set; }

            /// <summary>Name</summary>
            public string CustomerName { get; set; }

            /// <summary>Telephone</summary>
            public string PhoneNumber { get; set; }

            /// <summary>Email</summary>
            public string Email { get; set; }

            /// <summary>Reciever</summary>
            public string Penerima { get; set; }

            /// <summary>Information</summary>
            public string Keteramgam { get; set; }

            /// <summary>Judgement</summary>
            public string Judge { get; set; }
            /// <summary>Is selected</summary>
            public bool IsChecked
            {
                get
                {
                    return isChecked;
                }
                set
                {
                    if (isChecked != value)
                    {
                        isChecked = value;
                        OnPropertyChanged("IsChecked");

                        if (OnStatusChange != null)
                            OnStatusChange(this);
                    }
                }
            }

            public delegate void StatusChangeHandler(PhoneNumViewModel vm);
            public StatusChangeHandler OnStatusChange = null;

            private bool isChecked = false;

            public PhoneNumViewModel(string indexNumber,
                string customerName, string phoneNumber, 
                string eMail, string penerima,
                string keteramgam, string judge, StatusChangeHandler handler)
            {
                IndexNumber = indexNumber;
                CustomerName = customerName;
                PhoneNumber = phoneNumber;
                Email = eMail;
                Penerima = penerima;
                Keteramgam = keteramgam;
                Judge = judge;
                isChecked = false;
                OnStatusChange = handler;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
                }
            }

        }
        public bool IsCheckAll
        {
            get
            {
                return isCheckAll;
            }
            set
            {
                if (isCheckAll != value)
                {
                    foreach (PhoneNumViewModel vm in PhoneNumbers)
                        vm.IsChecked = value;

                    isCheckAll = value;
                    OnPropertyChanged("IsCheckAll");
                }
            }
        }

        public bool EnableSelecAll
        {
            get
            {
                return enableSelecAll;
            }
            set
            {
                if (enableSelecAll != value)
                {
                    enableSelecAll = value;
                    OnPropertyChanged("EnableSelecAll");
                }

            }
        }

        public ObservableCollection<PhoneNumViewModel> PhoneNumbers { get; private set; }

        private bool enableSelecAll = false;
        private bool isCheckAll = false;
        int selecteCount = 0;

        /// <summary>Constructor</summary>
        /// <param name="filePath">CSV file path</param>
        public WhatsAppViewModel(string csvFilePath)
        {
            PhoneNumbers = new ObservableCollection<PhoneNumViewModel>();

            List<List<string>> customerList = this.readCsvFile(csvFilePath);
            foreach (List<string> record in customerList)
            {
                PhoneNumbers.Add(new PhoneNumViewModel(
                    record[0], record[1], record[2], record[3],
                    record[4], record[5], "", new PhoneNumViewModel.StatusChangeHandler(this.OnStatusChange)));
            }
            EnableSelecAll = true;
        }

        /// <summary>CSV file read</summary>
        /// <param name="filePath">CSV file path</param>
        /// <returns>phone number list</returns>
        private List<List<string>> readCsvFile(string csvFilePath)
        {
            List<List<string>> customerList = new List<List<string>>();
            try
            {
                // determine the file exists in the argument path
                if (!File.Exists(csvFilePath))
                {
                    throw new FileNotFoundException("CSV file not found");
                }
                // File open to split line data separated by commas
                using (var parser = new TextFieldParser(csvFilePath, Encoding.Default) { Delimiters = new string[] { "," } })
                {
                    // Repeat until last line
                    while (!parser.EndOfData)
                    {
                        // Don't remove prefixes and suffixes spaces in strings
                        parser.TrimWhiteSpace = false;

                        // Get an array of strings separated by commas
                        var cells = parser.ReadFields();
                        List<string> record = new List<string>();

                        for (int i = 0; i < cells.Length; i++)
                        {
                            record.Add(cells[i]);
                        }
                        // 電話番号が入っているデータのみインポート対象にする
                        if (!cells[2].Equals(""))
                        {
                            customerList.Add(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // output error detail 
                Debug.WriteLine(ex.Message);
            }

            // ヘッダー行を削除する
            try
            {
                customerList.RemoveAt(0);
            }
            catch (Exception ex)
            {
                // output error detail 
                Debug.WriteLine(ex.Message);
            }

            return customerList;
        }


        private void OnStatusChange(PhoneNumViewModel vm)
        {
            bool value = vm.IsChecked;
            if (value == true)
            {
                selecteCount++;
                if (selecteCount == PhoneNumbers.Count)
                    isCheckAll = true;
            }
            else
            {
                selecteCount--;
                isCheckAll = false;
            }

            OnPropertyChanged("IsCheckAll");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        

    }


}