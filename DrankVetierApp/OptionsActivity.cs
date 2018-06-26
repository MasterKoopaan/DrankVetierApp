using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
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

        public RackConfig config;
        ListViewConfigure_Adapter adapter;
        TextWatcher textWatcher = new TextWatcher();

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

            config = DataHandler.GetConfig();
            if (config == null)
            {
                config = new RackConfig(0, 1, "0:"); //|6:Bier|3:S
            }

            //set start point view
            editTextWidth.Text = Convert.ToString(config.GetWidth());
            textViewLayersValue.Text = Convert.ToString(config.GetLayersCount());

            UpdateListViewConfig();
            //adapter = new ListViewConfigure_Adapter(this, config.Layers);
            //adapter.TxtChanged += OnTxtChanged;
            //listViewConfigure.Adapter = adapter;

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
                    config.Layers.Add(new Layer(0, ""));
                    //update list view
                    UpdateListViewConfig();
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
                    UpdateListViewConfig();
                }
               
            };

            //save the new config if correct and complete
            buttonSave.Click += (slender, e) =>
            {
                //check if the current config data complete and valid is,
                //else show a toast with fail message
                if (CheckNewConfig())
                {
                    //overwrite the config and empty the currently saved data 
                    DataHandler.SaveConfig(config);
                    DataHandler.ResetData();

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

        /// <summary>
        /// Going back to the main activity
        /// </summary>
        public void GoBackToMain()
        {
            Intent newActivityMain = new Intent(this, typeof(MainActivity));
            StartActivity(newActivityMain);
        }

        /// <summary>
        /// Check of the current the config correct and complete is
        /// </summary>
        /// <returns></returns>
        public bool CheckNewConfig()
        {
            if (config.GetWidth() <= 0 || config.GetWidth() > 999)
            {
                Toast.MakeText(this, "The set With is not valid, it needs to be between 1 and 999", ToastLength.Long).Show();
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

        /// <summary>
        /// Update the layercount textview
        /// </summary>
        /// <param name="value"></param>
        public void UpdateTextViewLayersCount(int value)
        {
            textViewLayersValue.Text = Convert.ToString(value);
        }

        /// <summary>
        /// Update the config listview
        /// </summary>
        public void UpdateListViewConfig()
        {
            adapter = new ListViewConfigure_Adapter(this, config.Layers);
            //adapter.SpanChanged += OnSpanChanged;
            //adapter.NameChanged += OnNameChanged;
            adapter.TxtChanged += OnTxtChanged;
            listViewConfigure.Adapter = adapter;
            //OnTxtChanged += OnTxtChanged;
        }

        private void OnSpanChanged(object sender, TextChangedEventArgs e)
        {
            
            config.Layers[0].SetSpan(Convert.ToInt16(e.Text));
        }

        public void OnNameChanged(object source, TextChangedEventArgs e)
        { 
            config.Layers[0].SetName(Convert.ToString(e.Text));
        }
        
        public void OnTxtChanged(object source, Custom_TextChangedArgs e)
        {
            if (e.type == "name")
            {
                config.Layers[e.position].SetName(e.text);
            } else
            {
                if (e.text == "")
                {
                    e.text = "0";
                }
                config.Layers[e.position].SetSpan(Convert.ToInt32(e.text));
            }
            
        }
    }
}