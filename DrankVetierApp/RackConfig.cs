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
    //03.120.07.04.12 
    public class RackConfig
    {
        private int Width;      //max = 999
        private string sWidth;
        private int LayersCount;     //max = 99
        private string sLayerCount;
        private Layer[] Layers; 

        public RackConfig(int Width, int LayersCount)
        {
            this.Width = Width;
            string sValue = Convert.ToString(Width);
            if (sValue.Length < 3) {
                if (sValue.Length < 2) {
                    sValue = "00" + sValue;
                } else {
                    sValue = "0" + sValue;
                }
            }
            sWidth = sValue;
            this.LayersCount = LayersCount;
            sLayerCount = LayersCount.ToString().Length < 2 ? "0" + LayersCount.ToString() : LayersCount.ToString();
            Layers = new Layer[LayersCount];
        }
        
        public string getConfig()
        {
            string config = Convert.ToString(LayersCount);
            if (config.Length < 2) config = "0" + config;
            string value = Convert.ToString(Width);
            if (value.Length < 2) config = "0" + value;
            return config;
        }
    }

    public class Layer
    {
        private string name;
        private int span;

    }
}