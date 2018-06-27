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
    [Activity(Label = "ExtraActivity")]
    public class ExtraActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Extra);

            // Set Result message
            Random r = new Random();
            if (r.Next(2) == 0)
            {
                FindViewById<TextView>(Resource.Id.textViewRandom).Text = "No";
            } else
            {
                FindViewById<TextView>(Resource.Id.textViewRandom).Text = "Yes";
            }

            // Go Back
            FindViewById<Button>(Resource.Id.buttonGoBack).Click += (slender, e) =>
            {
                Intent newActivityMain = new Intent(this, typeof(MainActivity));
                StartActivity(newActivityMain);
            };
        }
    }
}