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
    public static class DataHandler
    {
        // RackConfig handler

        //Save RackConfig in Application SharedPreferences
        public static void SaveConfig(RackConfig config)
        {
            var MyRackConfig = Application.Context.GetSharedPreferences("MyRackConfig", FileCreationMode.Private);
            var MyRackConfigEdit = MyRackConfig.Edit();
            MyRackConfigEdit.PutString("Width", Convert.ToString(config.GetWidth()));
            MyRackConfigEdit.PutString("LayersCount", Convert.ToString(config.GetLayersCount()));
            MyRackConfigEdit.PutString("LayersInfo", config.GetLayersInfo());
        }

        //Get local savad RackConfig in Application SharedPreferences
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

        //Set a RackData with incoming data
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

        //Save RackData in Application SharedPreferences
        public static void SaveData(RackData data)
        {
            var MyRackData = Application.Context.GetSharedPreferences("MyRackData", FileCreationMode.Private);
            var MyRackDataEdit = MyRackData.Edit();
            MyRackDataEdit.PutString("Amounts", data.GetAmountsString());
            //MyRackDataEdit.PutString("Updated", data.updated);
        }

        //Get local savad RackData in Application SharedPreferences
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

        //Delete currently saved RackData in Application SharedPreferences Because Config chance
        public static void ResetData()
        {
            var MyRackData = Application.Context.GetSharedPreferences("MyRackData", FileCreationMode.Private);
            var MyRackDataEdit = MyRackData.Edit();
            MyRackDataEdit.PutString("Amounts", "-1");
            MyRackDataEdit.PutString("Updated", "0");
        }
    }
}