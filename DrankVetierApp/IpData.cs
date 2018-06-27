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
    public class IpData
    {
        public string ip;
        public string poort;

        public IpData(string ip, string poort)
        {
            this.ip = ip;
            this.poort = poort;
        }
    }
}