using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Net;
using System;
using Android.Content;
using System.Timers;
using Android.Content.Res;
using System.Text;
using System.Text.RegularExpressions;

namespace DrankVetierApp
{
    [Activity(Label = "DrankVertierApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        // Initialize Object
        public RackConfig rackConfig;
        public RackData rackData;
        Socket socket = null;
        //Timer timerSockets;

        Button buttonConnect, buttonUpdate, buttonExtra, buttonOptions;
        ListView listViewResults;
        TextView textViewConnectie, textViewUpdated;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get view items
            buttonConnect = FindViewById<Button>(Resource.Id.buttonConnect);
            buttonUpdate = FindViewById<Button>(Resource.Id.buttonUpdate);
            buttonExtra = FindViewById<Button>(Resource.Id.buttonExtra);
            buttonOptions = FindViewById<Button>(Resource.Id.buttonOptions);
            listViewResults = FindViewById<ListView>(Resource.Id.listViewResults);
            textViewConnectie = FindViewById<TextView>(Resource.Id.textViewConnectie);
            textViewUpdated = FindViewById<TextView>(Resource.Id.textViewUpdated);

            // Get config en data
            rackConfig = DataHandler.GetConfig();
            rackConfig = new RackConfig(25, 1, "7:Bier");   //temp!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            rackData = DataHandler.GetData();

            // Update ListViewResults and Buttons Enabled default state
            if (rackConfig == null)
            {
                ArrayAdapter noConfigAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, new string[] { "There is no Config, Go to Options." });
                listViewResults.Adapter = noConfigAdapter;
                buttonConnect.Enabled = false;
            } else 
            {
                ListViewResults_Adapter ConfigAdapter = new ListViewResults_Adapter(this, new Rack(rackConfig.Layers, rackData == null ? null : rackData.amounts));
                listViewResults.Adapter = ConfigAdapter;
                if (rackData != null) textViewUpdated.Text = rackData.updated.ToString("h:mm:ss");
            }
            buttonUpdate.Enabled = false;
            textViewConnectie.SetTextColor(Android.Graphics.Color.Red);

        //! NOT IMPLEMENTED !
        buttonExtra.Enabled = false;

            // Go to options to set config
            buttonOptions.Click += (slender, e) =>
            {
                Intent nextActivityOptions = new Intent(this, typeof(OptionsActivity));
                StartActivity(nextActivityOptions);
            };

            //timerSockets = new System.Timers.Timer() { Interval = 10000, Enabled = false }; // Interval >= 750
            //timerSockets.Elapsed += (obj, args) =>
            //{
            //    //RunOnUiThread(() =>
            //    //{
            //    if (socket != null) // only if socket exists
            //    {
                    
            //    }
            //    else timerSockets.Enabled = false;  // If socket broken -> disable timer
            //    //});
            //};

            //If connected ask the amount on the layers
            buttonUpdate.Click += (sender, e) =>
            {
                string result = executeSend(3 + rackConfig.GetLayersCount() * 3 , "a");                 // Send toggle-command to the Arduino
                if (result != "err")
                {
                    rackData = DataHandler.SetData(result, rackConfig.GetLayersCount());
                    DataHandler.SaveData(rackData);
                    ListViewResults_Adapter ConfigAdapter = new ListViewResults_Adapter(this, new Rack(rackConfig.Layers, rackData.amounts));
                    listViewResults.Adapter = ConfigAdapter;
                    textViewUpdated.Text = rackData.updated.ToString("h:mm:ss"); 
                } else
                {
                    Toast.MakeText(this, "Updating failed", ToastLength.Long).Show();
                }
            };

            //If the connection data (Ip and Port) that are given are valid, then we will try to connect
            buttonConnect.Click += (sender, e) =>
            {
                //Validate the user input (IP address and port)
                if (CheckValidIpAddress("192.168.0.100") && CheckValidPort("3300"))
                {
                    ConnectSocket("192.168.0.100", "3300");
                }
                else UpdateConnectionState(3);
            };
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
                            string result = executeSend(4, 'c' + rackConfig.getConfig());
                            if (result == "suc")
                            {
                                UpdateConnectionState(2);
                                //timerSockets.Enabled = true;                //Activate timer for communication with Arduino    
                            }
                            else
                            {
                                if (result == "out")
                                {
                                    Toast.MakeText(this, "The device your connecting to does not suport the amount of layers you have set in the config", ToastLength.Long).Show();
                                }
                                else if (result == "err")
                                {
                                    Toast.MakeText(this, "Config invalid, make sure that you have a valid config", ToastLength.Long).Show();
                                }
                                if (socket != null)
                                {
                                    socket.Close();
                                    socket = null;
                                }
                                UpdateConnectionState(4);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        //timerSockets.Enabled = false;
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
                    //timerSockets.Enabled = false;
                    UpdateConnectionState(4);
                    textViewConnectie.Text = "Connect";
                }
            });
        }
        public void UpdateConnectionState(int state)
        {
            switch (state)
            {
                case 1:     //connecting
                    textViewConnectie.Text = "Connecting. . .";
                    textViewConnectie.SetTextColor(Android.Graphics.Color.Blue);
                    break;
                case 2:     //connected
                    textViewConnectie.Text = "Connected";
                    textViewConnectie.SetTextColor(Android.Graphics.Color.Green);
                    buttonUpdate.Enabled = true;
                    break;
                case 3:     //invalid ip or/and poort
                    textViewConnectie.Text = "Invalid Connection data";
                    textViewConnectie.SetTextColor(Android.Graphics.Color.Orange);
                    break;
                case 4:     //not connected
                    textViewConnectie.Text = "Not Connected";
                    textViewConnectie.SetTextColor(Android.Graphics.Color.Red);
                    buttonUpdate.Enabled = false;
                    break;
            }
        }

        public string executeSend(int messagesize, string send)
        {
            byte[] buffer = new byte[messagesize]; // response is max 496 characters long by "a", and 4 by "c"
            int bytesRead = 0;
            string result = "";

            if (socket != null)
            {
                //Send command to server
                socket.Send(Encoding.ASCII.GetBytes(send + '\n'));

                try //Get response from server
                {
                    //Store received bytes (always 4 bytes, ends with \n)
                    bytesRead = socket.Receive(buffer);  // If no data is available for reading, the Receive method will block until data is available,
                    //Read available bytes.              // socket.Available gets the amount of data that has been received from the network and is available to be read
                    //while (socket.Available > 0)
                    //{
                    //    bytesRead = socket.Receive(buffer);
                    //}
                    if (bytesRead > 3)
                        result = Encoding.ASCII.GetString(buffer, 0, bytesRead - 1); // skip \n
                    else result = "err";
                    while (socket.Available > 0) bytesRead = socket.Receive(buffer);
                }
                catch (Exception exception)
                {
                    result = exception.ToString();
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }
                    UpdateConnectionState(3);
                }
            }
            return result;
        }

        //Check if the entered IP address is valid.
        private bool CheckValidIpAddress(string ip)
        {
            if (ip != "")
            {
                //Check user input against regex (check if IP address is not empty).
                Regex regex = new Regex("\\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\\.|$)){4}\\b");
                Match match = regex.Match(ip);
                return match.Success;
            }
            else return false;
        }

        //Check if the entered port is valid.
        private bool CheckValidPort(string port)
        {
            //Check if a value is entered.
            if (port != "")
            {
                Regex regex = new Regex("[0-9]+");
                Match match = regex.Match(port);

                if (match.Success)
                {
                    int portAsInteger = Int32.Parse(port);
                    //Check if port is in range.
                    return ((portAsInteger >= 0) && (portAsInteger <= 65535));
                }
                else return false;
            }
            else return false;
        }
    }
}