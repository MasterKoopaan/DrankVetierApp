using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Net;
using System;
using Android.Content;
using System.Timers;
using Android.Content.Res;

namespace DrankVetierApp
{
    [Activity(Label = "DrankVertierApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        //https://www.youtube.com/watch?v=rYxWSV-x65I https://www.youtube.com/watch?v=s6mPvaxvLXQ https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/files?tabs=vswin https://www.youtube.com/watch?v=sk9fRXu53Qs

        public RackConfig rackConfig;
        Socket socket = null;
        Timer timerSockets;

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
            //buttonOptions.SetBackgroundColor(Android.Graphics.Color.Red);
            //buttonOptions.BackgroundTintMode = ColorStateList.ValueOf(Color.HoloRedLight); 
            listViewResults = FindViewById<ListView>(Resource.Id.listViewResults);

            buttonOptions.Click += (slender, e) =>
            {
                Intent nextActivityOptions = new Intent(this, typeof(OptionsActivity));
                StartActivity(nextActivityOptions);
            };

            timerSockets = new System.Timers.Timer() { Interval = 10000, Enabled = false }; // Interval >= 750
            timerSockets.Elapsed += (obj, args) =>
            {
                //RunOnUiThread(() =>
                //{
                if (socket != null) // only if socket exists
                {
                    
                }
                else timerSockets.Enabled = false;  // If socket broken -> disable timer
                //});
            };
        }

        public void UpdateConnectionState(int state)
        {
            switch (state)
            {
                case 1:     //connecting
                    //buttonOptions.SetBackgroundColor(Android.Graphics.Color.Orange); 
                    break;
                case 2:     //connected
                    //buttonOptions.SetBackgroundColor(Android.Graphics.Color.Green);
                    break;
                case 4:     //not connected
                    //buttonOptions.SetBackgroundColor(Android.Graphics.Color.Red);
                    break;
            }
        }
        // Connect to socket ip/prt (simple sockets)
        public void ConnectSocket(string ip, string prt)
        {
            RunOnUiThread(() =>
            {
                if (socket == null)                                       // create new socket
                {
                    UpdateConnectionState(1);
                    try  // to connect to the server (Arduino).
                    {
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(prt)));
                        if (socket.Connected)
                        {
                            UpdateConnectionState(2);
                            timerSockets.Enabled = true;                //Activate timer for communication with Arduino     
                        }
                    }
                    catch (Exception exception)
                    {
                        timerSockets.Enabled = false;
                        if (socket != null)
                        {
                            socket.Close();
                            socket = null;
                        }
                        UpdateConnectionState(4);
                    }
                }
                else // disconnect socket
                {
                    socket.Close(); socket = null;
                    timerSockets.Enabled = false;
                    UpdateConnectionState(4);
                }
            });
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
    }
}

