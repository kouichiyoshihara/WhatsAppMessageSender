using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WhatsAppMessageSender.Data
{
    /// <summary>Phone number class</summary>
    public class WhatsAppViewModel : INotifyPropertyChanged
    {

        public class PhoneNumViewModel : INotifyPropertyChanged
        {
            /// <summary>Phone number</summary>
            public string PhoneNumber { get; set; }

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

            public PhoneNumViewModel(string phoneNumber, StatusChangeHandler handler)
            {
                PhoneNumber = phoneNumber;
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

            List<string> phoneNumList = this.readCsvFile(csvFilePath);
            foreach (string phoneNum in phoneNumList)
            {
                PhoneNumbers.Add(new PhoneNumViewModel(phoneNum, new PhoneNumViewModel.StatusChangeHandler(this.OnStatusChange)));
            }
            EnableSelecAll = true;
        }

        /// <summary>CSV file read</summary>
        /// <param name="filePath">CSV file path</param>
        /// <returns>phone number list</returns>
        private List<string> readCsvFile(string csvFilePath)
        {
            List<string> phoneNumList = new List<string>();
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

                        for (int i = 0; i < cells.Length; i++)
                        {
                            phoneNumList.Add(cells[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // output error detail 
                Debug.WriteLine(ex.Message);
            }
            return phoneNumList;
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