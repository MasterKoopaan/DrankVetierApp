using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DrankVetierApp
{
    /// <summary>
    /// Data Handler for handeling RackConfig en RackData data
    /// </summary>
    public static class DataHandler
    {
        // RackConfig handler

        /// <summary>
        /// Save RackConfig in Application SharedPreferences
        /// </summary>
        /// <param name="config"></param>
        public static void SaveConfig(RackConfig config)
        {
            var MyRackConfig = Application.Context.GetSharedPreferences("MyRackConfig", FileCreationMode.Private);
            var MyRackConfigEdit = MyRackConfig.Edit();
            MyRackConfigEdit.PutString("Width", Convert.ToString(config.GetWidth()));
            MyRackConfigEdit.PutString("LayersCount", Convert.ToString(config.GetLayersCount()));
            MyRackConfigEdit.PutString("LayersInfo", config.GetLayersInfo());
            MyRackConfigEdit.Apply();
        }

        /// <summary>
        /// returns the local saved RackConfig in Application SharedPreferences
        /// </summary>
        /// <returns></returns>
        public static RackConfig GetConfig()
        {
            var MyRackConfig = Application.Context.GetSharedPreferences("MyRackConfig", FileCreationMode.Private);
            int width = Convert.ToInt32(MyRackConfig.GetString("Width", "0"));
            int layersCount = Convert.ToInt32(MyRackConfig.GetString("LayersCount", "0"));
            string layersInfo = MyRackConfig.GetString("LayersInfo", "0");
            if (width != 0 && layersCount != 0 && layersInfo != "0")
            {
                return new RackConfig(width, layersCount, layersInfo);
            }
            else
            {
                return null;
            }
        }

        // RackData handlers

        const string FMT = "O";     //https://stackoverflow.com/questions/10798980/convert-c-sharp-date-time-to-string-and-back

        /// <summary>
        /// Returns the new RackData with incoming data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="currentlayercount"></param>
        /// <returns></returns>
        public static RackData SetData(string data, int currentlayercount) //Incoming data sturcture: 03 002004010 - layers, amount layersX
        {
            int layerscount = Convert.ToInt32(data.Substring(0, 2));
            if (currentlayercount != layerscount)
            {
                return null;
            }
            int[] amount = new int[layerscount];
            for (int i = 0; i < layerscount; i ++)
            {
                amount[i] = Convert.ToInt32(data.Substring(2 + i * 3, 3));
            }
            return new RackData(amount, DateTime.Now);
        }

        /// <summary>
        /// Save RackData in Application SharedPreferences
        /// </summary>
        /// <param name="data"></param>
        public static void SaveData(RackData data)
        {
            var MyRackData = Application.Context.GetSharedPreferences("MyRackData", FileCreationMode.Private);
            var MyRackDataEdit = MyRackData.Edit();
            MyRackDataEdit.PutString("Amounts", data.GetAmountsString());
            MyRackDataEdit.PutString("Updated", data.updated.ToString(FMT));
            MyRackDataEdit.Apply();
        }

        /// <summary>
        /// Returns the local savad RackData in Application SharedPreferences
        /// </summary>
        /// <returns></returns>
        public static RackData GetData()
        {
            var MyRackData = Application.Context.GetSharedPreferences("MyRackData", FileCreationMode.Private);
            string Amounts = MyRackData.GetString("Amounts", "-1");
            string Updated = MyRackData.GetString("Updated", "0");
            if (Amounts != "-1" && Updated != "0")
            {
                return new RackData(Array.ConvertAll(Amounts.Split('|'), int.Parse), DateTime.ParseExact(Updated, FMT, CultureInfo.InvariantCulture));
            }
            else
            {
                return null;
            }     
        }

        /// <summary>
        /// Delete currently saved RackData in Application SharedPreferences because a chance of the current Config 
        /// </summary>
        public static void ResetData()
        {
            var MyRackData = Application.Context.GetSharedPreferences("MyRackData", FileCreationMode.Private);
            var MyRackDataEdit = MyRackData.Edit();
            MyRackDataEdit.PutString("Amounts", "-1");
            MyRackDataEdit.PutString("Updated", "0");
            MyRackDataEdit.Apply();
        }

        // Ip handler

        /// <summary>
        /// Save RackData in Application SharedPreferences
        /// </summary>
        /// <param name="data"></param>
        public static void SaveIp(IpData ipData)
        {
            var MyIpData = Application.Context.GetSharedPreferences("MyRackData", FileCreationMode.Private);
            var MyIpDataEdit = MyIpData.Edit();
            MyIpDataEdit.PutString("Ip", ipData.ip);
            MyIpDataEdit.PutString("Poort", ipData.poort);
            MyIpDataEdit.Apply();
        }

        /// <summary>
        /// Returns the local savad RackData in Application SharedPreferences
        /// </summary>
        /// <returns></returns>
        public static IpData GetIp()
        {
            var MyIpData = Application.Context.GetSharedPreferences("MyRackData", FileCreationMode.Private);
            string Ip = MyIpData.GetString("Ip", "0");
            string Poort = MyIpData.GetString("Poort", "0");
            if (Ip == "0" && Poort == "0")
            {
                return new IpData("192.168.0.100", "3300");
            }
            else
            {
                return new IpData(Ip, Poort);
            }
        }

        //Check if the entered IP address is valid.
        public static bool CheckValidIpAddress(string ip)
        {
            if (ip != "")
            {
                //Check user input against regex (check if IP address is not empty).
                Regex regex = new Regex("\\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\\.|$)){4}\\b");
                Match match = regex.Match(ip);
                return match.Success;
            }
            else return false;
        }

        //Check if the entered port is valid.
        public static bool CheckValidPort(string port)
        {
            //Check if a value is entered.
            if (port != "")
            {
                Regex regex = new Regex("[0-9]+");
                Match match = regex.Match(port);

                if (match.Success)
                {
                    int portAsInteger = Int32.Parse(port);
                    //Check if port is in range.
                    return ((portAsInteger >= 0) && (portAsInteger <= 65535));
                }
                else return false;
            }
            else return false;
        }
    }
}