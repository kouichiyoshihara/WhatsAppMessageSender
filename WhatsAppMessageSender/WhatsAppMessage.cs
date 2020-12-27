using System;
using System.Net;
using System.Web.Script.Serialization; // requires the reference 'System.Web.Extensions'
using System.IO;
using System.Text;
using System.Collections;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace WhatsAppMessage.Sender
{
    class WhatsAppMessageSender
    {
        private static string CONFIG_FILE_PATH;

        private static string INSTANCE_ID;
        private static string CLIENT_ID;
        private static string CLIENT_SECRET;
        private static string API_URL;

        private static string APL_PATH;

        /// <summary>Read INSTANCE_ID, CLIENT_ID, CLIENT_SECRET and API_URL from config file</summary>
        public WhatsAppMessageSender()
        {
            CONFIG_FILE_PATH = System.AppDomain.CurrentDomain.BaseDirectory + "conf\\WhatsAppGatewayAPI.config";
            Hashtable ht = this.readConfFile(CONFIG_FILE_PATH);
            INSTANCE_ID = (string)ht["INSTANCE_ID"];
            CLIENT_ID = (string)ht["CLIENT_ID"];
            CLIENT_SECRET = (string)ht["CLIENT_SECRET"];
            API_URL = (string)ht["API_URL"] + INSTANCE_ID;
            APL_PATH = (string)ht["APL_PATH"];
        }

        /// <summary>Config file read</summary>
        /// <param name="filePath">Conf file path</param>
        /// <returns>hashtable of INSTANCE_ID, CLIENT_ID, CLIENT_SECRET and API_URL</returns>
        private Hashtable readConfFile(string confFilePath)
        {
            Hashtable ht = new Hashtable();
            try
            {
                // determine the file exists in the argument path
                if (!File.Exists(confFilePath))
                {
                    throw new FileNotFoundException("WhatsAppGatewayAPI.config file not found");
                }
                // File open to split line data separated by double colon
                using (var parser = new TextFieldParser(confFilePath, Encoding.Default) { Delimiters = new string[] { "::" } })
                {
                    // Repeat until last line
                    while (!parser.EndOfData)
                    {
                        // Remove prefixes and suffixes spaces in strings
                        parser.TrimWhiteSpace = true;

                        // Get an hashtable of strings separated by double colon
                        var cells = parser.ReadFields();
                        ht.Add(cells[0], cells[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                // output error detail 
                Debug.WriteLine(ex.Message);
            }
            return ht;
        }

        public bool sendMessage(string number, string message)
        {
            bool success = true;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers["X-WM-CLIENT-ID"] = CLIENT_ID;
                    client.Headers["X-WM-CLIENT-SECRET"] = CLIENT_SECRET;

                    Payload payloadObj = new Payload() { number = number, message = message };
                    string postData = (new JavaScriptSerializer()).Serialize(payloadObj);

                    client.Encoding = Encoding.UTF8;
                    string response = client.UploadString(API_URL, postData);
                    Console.WriteLine(response);
                }
            }
            catch (WebException webEx)
            {
                Console.WriteLine(((HttpWebResponse)webEx.Response).StatusCode);
                Stream stream = ((HttpWebResponse)webEx.Response).GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                String body = reader.ReadToEnd();
                Console.WriteLine(body);
                success = false;
            }

            return success;
        }

        private class Payload
        {
            public string number { get; set; }
            public string message { get; set; }
        }
    }

}
