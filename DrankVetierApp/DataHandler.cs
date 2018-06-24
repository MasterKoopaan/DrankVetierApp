using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            for (int i = 2; i < layerscount; i += 3)
            {
                amount[i] = Convert.ToInt32(data.Substring(i, 3));
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
            //MyRackDataEdit.PutString("Updated", data.updated);
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
                return new RackData(Array.ConvertAll(Amounts.Split('|'), int.Parse), DateTime.Now);
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
        }
    }
}