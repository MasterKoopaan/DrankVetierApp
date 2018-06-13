using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Net;
using System;
using Android.Content;

namespace DrankVetierApp
{
    [Activity(Label = "DrankVertierApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public RackConfig rackConfig;
        Socket socket = null;

        TextView textViewMainInfo;
        Button buttonOptions;
        ListView listViewResults;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            textViewMainInfo = FindViewById<TextView>(Resource.Id.textViewMainInfo);
            buttonOptions = FindViewById<Button>(Resource.Id.buttonOptions);
            listViewResults = FindViewById<ListView>(Resource.Id.listViewResults);

            buttonOptions.Click += (slender, e) =>
            {
                Intent nextActivityOptions = new Intent(this, )
            };
        }

    }
}

