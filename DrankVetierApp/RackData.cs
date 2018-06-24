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
    public class RackData
    {
        public int[] amounts;
        public DateTime updated;

        public RackData(int[] amounts, DateTime now)
        {
            this.amounts = amounts;
            updated = now;
        }

        public string GetAmountsString()
        {
            return string.Join("|", amounts);
        }
    }

    public class Rack
    {
        public List<Layer> layers;
        public int[] amounts;

        public Rack(List<Layer> layers, int[] amounts)
        {
            this.layers = layers;
            this.amounts = amounts;
        }

    }
}