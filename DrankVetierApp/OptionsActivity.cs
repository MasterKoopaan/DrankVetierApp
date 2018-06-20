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
        ListView listViewConfigure;

        RackConfig config;
        ListViewConfigure_Adapter adapter;

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
            listViewConfigure = FindViewById<ListView>(Resource.Id.listViewConfigure);

            config = GetConfig();
            if (config == null)
            {
                config = new RackConfig(0, 1, "7:Empty"); //|6:Bier|3:S
            }

            //set start point view
            editTextWidth.Text = Convert.ToString(config.GetWidth());
            textViewLayersValue.Text = Convert.ToString(config.GetLayersCount());

            adapter = new ListViewConfigure_Adapter(this, config.Layers);
            listViewConfigure.Adapter = adapter;

            //listViewConfigure.event += subscibe 

            //change the amount of Width
            editTextWidth.TextChanged += (slender, e) => {
                string widthText = editTextWidth.Text;
                if (widthText != "")
                {
                    config.SetWidth(Convert.ToInt32(widthText));
                }
            };

            //change the amount of Layers
            buttonPlus.Click += (slender, e) =>
            {
                int LayersCount = config.GetLayersCount();
                if (LayersCount < 99)
                {
                    LayersCount++;
                    config.SetLayersCount(LayersCount);
                    UpdateTextViewLayersCount(LayersCount);
                    config.Layers.Add(new Layer(0, "Empty"));
                    //update list view
                    adapter = new ListViewConfigure_Adapter(this, config.Layers);
                    listViewConfigure.Adapter = adapter;
                }
            };
            buttonMin.Click += (slender, e) =>
            {
                int LayersCount = config.GetLayersCount();
                if (LayersCount > 1)
                {
                    LayersCount--;
                    config.SetLayersCount(LayersCount);
                    UpdateTextViewLayersCount(LayersCount);
                    config.Layers.RemoveAt(LayersCount);
                    //update list view
                    adapter = new ListViewConfigure_Adapter(this, config.Layers);
                    listViewConfigure.Adapter = adapter;
                }
               
            };

            
            //save the new config if correct and complete
            buttonSave.Click += (slender, e) =>
            {
                //check if the current config data complete and valid is,
                //else show a toast with fail message
                if (CheckNewConfig())
                {
                    //overwrite the config
                    SaveConfig();

                    //go back to main activity
                    Intent newActivityMain = new Intent(this, typeof(MainActivity));
                    StartActivity(newActivityMain);
                }
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

        public bool CheckNewConfig()
        {
            if (config.GetWidth() <= 0 || config.GetWidth() > 99)
            {
                Toast.MakeText(this, "The set With is not valid, it needs to be between 1 and 99", ToastLength.Long).Show();
                return false;
            }
            for (int i = 0; i < config.Layers.Count(); i++)
            {
                if (config.Layers[i].GetSpan() < 0 || config.Layers[i].GetSpan() > 99)
                {
                    Toast.MakeText(this, "On Layer " + Convert.ToString(i+1) + " is the span not correctly set, it needs to be between 1 and 99", ToastLength.Long).Show();
                    return false;
                }
            }
            return true;
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