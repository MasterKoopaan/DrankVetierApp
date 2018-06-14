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
    [Activity(Label = "OptionsActivity")]
    public class OptionsActivity : Activity
    {
        //objects and values:
        Button buttonSave, buttonCancel, buttonPlus, buttonMin;
        EditText editTextWidth;
        TextView textViewLayersValue;
        ListView listViewLayers;

        RackConfig config;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Options);

            //fill view object;
            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            editTextWidth = FindViewById<EditText>(Resource.Id.editTextWidth);
            textViewLayersValue = FindViewById<TextView>(Resource.Id.textViewLayersValue);
            buttonPlus = FindViewById<Button>(Resource.Id.buttonPlus);
            buttonMin = FindViewById<Button>(Resource.Id.buttonMin);
            listViewLayers = FindViewById<ListView>(Resource.Id.listViewLayers);

            config = GetConfig();
            if (config == null)
            {
                config = new RackConfig(0, 1, "7:");
            }

            //set start point view
            editTextWidth.Text = Convert.ToString(config.GetWidth());
            textViewLayersValue.Text = Convert.ToString(config.GetLayersCount());

            //change the amount of Layers
            buttonPlus.Click += (slender, e) =>
            {
                int LayersCount = config.GetLayersCount();
                if (LayersCount < 99)
                {
                    LayersCount++;
                    config.SetLayersCount(LayersCount);
                    UpdateTextViewLayersCount(LayersCount);
                }
                //update list view
            };
            buttonMin.Click += (slender, e) =>
            {
                int LayersCount = config.GetLayersCount();
                if (LayersCount > 0)
                {
                    LayersCount--;
                    config.SetLayersCount(LayersCount);
                    UpdateTextViewLayersCount(LayersCount);
                }
                //update list view
            };

            //save the new config if correct and complete
            buttonSave.Click += (slender, e) =>
            {
                //check if the current config data complete is


                //overwrite the config
                SaveConfig();

                //go back to main activity
                Intent newActivityMain = new Intent(this, typeof(MainActivity));
                StartActivity(newActivityMain);
                
                //else show a toast with fail message
            };

            //cancel by going back to main and saving nothing (the original config will be unchanged)
            buttonCancel.Click += (slender, e) =>
            {
                GoBackToMain();
            };
        }

        //going back to the main activity
        public void GoBackToMain()
        {
            Intent newActivityMain = new Intent(this, typeof(MainActivity));
            StartActivity(newActivityMain);
        }

        public void SaveConfig()
        {
            var MyRackConfig = Application.Context.GetSharedPreferences("MyRackConfig", FileCreationMode.Private);
            var MyRackConfigEdit = MyRackConfig.Edit();
            MyRackConfigEdit.PutString("Width", Convert.ToString(config.GetWidth()));
            MyRackConfigEdit.PutString("LayersCount", Convert.ToString(config.GetLayersCount()));
            MyRackConfigEdit.PutString("LayersInfo", config.GetLayersInfo());
        }

        public RackConfig GetConfig()
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

        public void UpdateTextViewLayersCount(int value)
        {
            textViewLayersValue.Text = Convert.ToString(value);
        }
    }
}