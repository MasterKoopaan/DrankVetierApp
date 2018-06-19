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
        private int LayersCount;     //max = 99
        public List<Layer> Layers; 

        public RackConfig(int Width, int LayersCount, string LayerInfo)
        {
            this.Width = Width;
            this.LayersCount = LayersCount;
            Layers = new List<Layer>();
            foreach (string layerI in LayerInfo.Split('|'))
            {
                string[] layerValues = layerI.Split(':');
                Layers.Add(new Layer(Convert.ToInt32(layerValues[0]), layerValues[1]));
            }
            
        }

        public int GetWidth()
        {
            return Width;
        }
        public int GetLayersCount()
        {
            return LayersCount;
        }
        public void SetWidth(int Width)
        {
            this.Width = Width;
        }
        public void SetLayersCount(int LayersCount)
        {
            this.LayersCount = LayersCount;
        }

        public string getConfig()
        {
            string config = Convert.ToString(LayersCount);
            if (config.Length < 2) config = "0" + config;
            string value = Convert.ToString(Width);
            if (value.Length < 2) config = "0" + value;
            return config;
        }

        public string GetLayersInfo()
        {
            string LayerInfo = "";
            foreach (Layer layer in Layers)
            {
                LayerInfo += layer.GetLayerInfo() + "|";
            }
            LayerInfo.Remove(LayerInfo.Length - 1, 1);
            return LayerInfo;
        }
    }

    public class Layer
    {
        private string name;
        private int span;

        public Layer(int span, string name)
        {
            this.span = span;
            this.name = name;
        }

        public string GetLayerInfo()
        {
            return name + ":" + Convert.ToString(span);
        }

        public string GetName()
        {
            return name;
        }

        public int GetSpan()
        {
            return span;
        }
    }
}