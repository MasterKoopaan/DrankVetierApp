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
    [Activity(Label = "SetIpActivity")]
    public class SetIpActivity : Activity
    {
        //values and objects
        IpData ipData;
        EditText editTextIp, editTextPoort;
        Button buttonCancelIp, buttonSaveIp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Ip);

            // Get current ip
            ipData = DataHandler.GetIp();

            // Get view elements
            editTextIp = FindViewById<EditText>(Resource.Id.editTextIp);
            editTextPoort = FindViewById<EditText>(Resource.Id.editTextpoort);
            buttonCancelIp = FindViewById<Button>(Resource.Id.buttonCancelIp);
            buttonSaveIp = FindViewById<Button>(Resource.Id.buttonSaveIp);

            // Set default values view
            editTextIp.Text = ipData.ip;
            editTextPoort.Text = ipData.poort;

            // Set button click liseners:
            buttonCancelIp.Click += (sender, e) =>
            {
                GoBackToMain();
            };

            buttonSaveIp.Click += (sender, e) =>
            {
                if (!DataHandler.CheckValidIpAddress(editTextIp.Text))                  
                {
                    //invalide ip
                    Toast.MakeText(this, "Invalide ip format", ToastLength.Long).Show();
                }
                else if (!DataHandler.CheckValidPort(editTextPoort.Text))             
                {
                    //invalide poort
                    Toast.MakeText(this, "Invalide poort format", ToastLength.Long).Show();
                }
                else
                {
                    //valide connection data     
                    DataHandler.SaveIp(new IpData(editTextIp.Text, editTextPoort.Text));
                    GoBackToMain();
                }
            };
        }

        public void GoBackToMain()
        {
            Intent newActivityMain = new Intent(this, typeof(MainActivity));
            StartActivity(newActivityMain);
        }
    }
}