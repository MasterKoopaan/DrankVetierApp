// Xamarin/C# app voor de besturing van een Arduino (Uno met Ethernet Shield) m.b.v. een socket-interface.
// Dit programma werkt samen met het Arduino-programma DomoticaServer.ino
// De besturing heeft betrekking op het aan- en uitschakelen van een Arduino pin, waar een led aan kan hangen of, 
// t.b.v. het Domotica project, een RF-zender waarmee een klik-aan-klik-uit apparaat bestuurd kan worden.
//
// De socket-communicatie werkt in is gebaseerd op een timer, waarbij het opvragen van gegevens van de 
// Arduino (server) m.b.v. een Timer worden gerealisseerd.
//
// Werking: De communicatie met de (Arduino) server is gebaseerd op een socket-interface. Het IP- en Port-nummer
// is instelbaar. Na verbinding kunnen, middels een eenvoudig commando-protocol, opdrachten gegeven worden aan 
// de server (bijv. pin aan/uit). Indien de server om een response wordt gevraagd (bijv. led-status of een
// sensorwaarde), wordt deze in een 4-bytes ASCII-buffer ontvangen, en op het scherm geplaatst. Alle commando's naar 
// de server zijn gecodeerd met 1 char.
//
// Aanbeveling: Bestudeer het protocol in samenhang met de code van de Arduino server.
// Het default IP- en Port-nummer (zoals dat in het GUI verschijnt) kan aangepast worden in de file "Strings.xml". De
// ingestelde waarde is gebaseerd op je eigen netwerkomgeving, hier (en in de Arduino-code) is dat een router, die via DHCP
// in het segment 192.168.1.x IP-adressen uitgeeft.
// 
// Resource files:
//   Main.axml (voor het grafisch design, in de map Resources->layout)
//   Strings.xml (voor alle statische strings in het interface (ook het default IP-adres), in de map Resources->values)
// 
// De software is verder gedocumenteerd in de code. Tijdens de colleges wordt er nadere uitleg over gegeven.
// 
// Versie 1.2, 16/12/2016
// S. Oosterhaven
//
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Android.Graphics;
using System.Threading.Tasks;

namespace Domotica
{
    [Activity(Label = "@string/application_name", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]

    public class MainActivity : Activity
    {
        // Variables (components/controls)
        // Controls on GUI
        Button buttonConnect, buttonKaku1, buttonKaku2, buttonKaku3;
        Button buttonChangePinState;
        TextView textViewServerConnect, textViewTimerStateValue, textViewSeekBar, textViewKaku1, textViewKaku2, textViewKaku3;
        public TextView textViewChangePinStateValue, textViewSensorValue, textViewSensorValue2, textViewDebugValue;
        EditText editTextIPAddress, editTextIPPort, editText_userTimeOn, editText_userTimeOff, editTextSensorValue;
        SeekBar seekBar;

        Timer timerClock, timerSockets;             // Timers   
        Socket socket = null;                       // Socket   
        List<Tuple<string, TextView>> commandList = new List<Tuple<string, TextView>>();  // List for commands and response places on UI

        int randvoorwaarde = 200;
        DateTime timeOn, timeOff;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource (strings are loaded from Recources -> values -> Strings.xml)
            SetContentView(Resource.Layout.Main);

            // find and set the controls, so it can be used in the code
            buttonConnect = FindViewById<Button>(Resource.Id.buttonConnect);
            buttonChangePinState = FindViewById<Button>(Resource.Id.buttonChangePinState);
            textViewTimerStateValue = FindViewById<TextView>(Resource.Id.textViewTimerStateValue);
            textViewServerConnect = FindViewById<TextView>(Resource.Id.textViewServerConnect);
            textViewChangePinStateValue = FindViewById<TextView>(Resource.Id.textViewChangePinStateValue);
            textViewSensorValue = FindViewById<TextView>(Resource.Id.textViewSensorValue);
            textViewSensorValue2 = FindViewById<TextView>(Resource.Id.textViewSensorValue2);
            textViewDebugValue = FindViewById<TextView>(Resource.Id.textViewDebugValue);
            editTextIPAddress = FindViewById<EditText>(Resource.Id.editTextIPAddress);
            editTextIPPort = FindViewById<EditText>(Resource.Id.editTextIPPort);
            editText_userTimeOn = FindViewById<EditText>(Resource.Id.editText_userTimeOn);   
            editText_userTimeOff = FindViewById<EditText>(Resource.Id.editText_userTimeOff);
            editTextSensorValue = FindViewById<EditText>(Resource.Id.editTextSensorValue);

            seekBar = FindViewById<SeekBar>(Resource.Id.seekBar1);
            textViewSeekBar = FindViewById<TextView>(Resource.Id.textViewSeekBar1);

            buttonKaku1 = FindViewById<Button>(Resource.Id.buttonKaku1);
            textViewKaku1 = FindViewById<TextView>(Resource.Id.textViewKaku1);
            buttonKaku2 = FindViewById<Button>(Resource.Id.buttonKaku2);
            textViewKaku2 = FindViewById<TextView>(Resource.Id.textViewKaku2);
            buttonKaku3 = FindViewById<Button>(Resource.Id.buttonKaku3);
            textViewKaku3 = FindViewById<TextView>(Resource.Id.textViewKaku3);

            seekBar.ProgressChanged += (object slender, SeekBar.ProgressChangedEventArgs e) =>
            {
                textViewSeekBar.Text = Convert.ToString(seekBar.Progress + 1) + "x per 10 sec";
                textViewDebugValue.Text = Convert.ToString(1 + seekBar.Progress) + " " + Convert.ToString(10000 / (1 + seekBar.Progress)) + " " + Convert.ToString(seekBar.Progress);
                timerSockets.Enabled = false;
                timerSockets.Interval = 10000 / (1 + seekBar.Progress);
                timerSockets.Enabled = true;
            };

            editTextSensorValue.TextChanged += (slender, e) =>
            {
                string t = textViewSensorValue.Text;
                if (t == "") t = "0";
                randvoorwaarde = Convert.ToInt32(t);
            };

            UpdateConnectionState(4, "Disconnected"); // 4 = een out of bound value, dus de default == disconnect

            // Init commandlist, scheduled by socket timer
            commandList.Add(new Tuple<string, TextView>("s", textViewChangePinStateValue));
            commandList.Add(new Tuple<string, TextView>("a", textViewSensorValue));         //sensor 1
            commandList.Add(new Tuple<string, TextView>("b", textViewSensorValue2));         //sensor 2

            this.Title = this.Title + " (timer sockets)";

            // timer object, running clock
            timerClock = new System.Timers.Timer() { Interval = 2000, Enabled = true }; // Interval >= 1000
            timerClock.Elapsed += (obj, args) =>
            {
                RunOnUiThread(() => { textViewTimerStateValue.Text = DateTime.Now.ToString("h:mm:ss"); }); 
            };

            // timer object, check Arduino state
            // Only one command can be serviced in an timer tick, schedule from list
            timerSockets = new System.Timers.Timer() { Interval = 10000, Enabled = false }; // Interval >= 750
            timerSockets.Elapsed += (obj, args) =>
            {
                RunOnUiThread(() =>
                {
                    if (socket != null) // only if socket exists
                    {
                        for (int i = 0; i < commandList.Count; i++)
                        {
                            UpdateGUI(executeCommand(commandList[i].Item1), commandList[i].Item2);
                        }
                        UpdateKakuGUI(executeCommand("k"));
                        //if the value is lower than the value that has been put in by the user the state will switch
                        if(Convert.ToInt32(textViewSensorValue.Text) < randvoorwaarde && textViewKaku2.Text == "1")
                        {
                            socket.Send(Encoding.ASCII.GetBytes("e")); //kaku 2 switch
                        }
                        if (DateTime.TryParse(editText_userTimeOn.Text, out timeOn) && DateTime.TryParse(editText_userTimeOff.Text, out timeOff))
                        {
                            if (DateTime.Now > timeOn && DateTime.Now < timeOff)
                            {
                                if (textViewKaku3.Text == "0") socket.Send(Encoding.ASCII.GetBytes("f")); //kaku 3 switch
                            } else if (textViewKaku3.Text == "1")
                            {
                                socket.Send(Encoding.ASCII.GetBytes("f")); //kaku 3 switch
                            }
                        }

                    // Send a command to the Arduino server on every tick (loop though list)
                    //UpdateGUI(executeCommand(commandList[listIndex].Item1), commandList[listIndex].Item2);  //e.g. UpdateGUI(executeCommand("s"), textViewChangePinStateValue);
                    //if (++listIndex >= commandList.Count) listIndex = 0;
                }
                else timerSockets.Enabled = false;  // If socket broken -> disable timer
                });
            };

            //Add the "Connect" button handler.
            if (buttonConnect != null)  // if button exists
            {
                buttonConnect.Click += (sender, e) =>
                {
                    //Validate the user input (IP address and port)
                    if (CheckValidIpAddress(editTextIPAddress.Text) && CheckValidPort(editTextIPPort.Text))
                    {
                        ConnectSocket(editTextIPAddress.Text, editTextIPPort.Text);
                    }
                    else UpdateConnectionState(3, "Please check IP");
                };
            }

            //Add the "Change pin state" button handler.
            if (buttonChangePinState != null)
            {
                buttonChangePinState.Click += (sender, e) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("t"));                 // Send toggle-command to the Arduino
                };
            }

            if (buttonKaku1 != null && buttonKaku1 != null && buttonKaku1 != null)
            {
                buttonKaku1.Click += (sender, e) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("d"));                 // Send toggle-command to the Arduino
                };
                buttonKaku2.Click += (sender, e) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("e"));                 // Send toggle-command to the Arduino
                };
                buttonKaku3.Click += (sender, e) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("f"));                 // Send toggle-command to the Arduino
                };
            }
        }

        //Send command to server and wait for response (blocking)
        //Method should only be called when socket existst
        public string executeCommand(string cmd)
        {
            byte[] buffer = new byte[4]; // response is always 4 bytes
            int bytesRead = 0;
            string result = "---";

            if (socket != null)
            {
                //Send command to server
                socket.Send(Encoding.ASCII.GetBytes(cmd));

                try //Get response from server
                {
                    //Store received bytes (always 4 bytes, ends with \n)
                    bytesRead = socket.Receive(buffer);  // If no data is available for reading, the Receive method will block until data is available,
                    //Read available bytes.              // socket.Available gets the amount of data that has been received from the network and is available to be read
                    while (socket.Available > 0) bytesRead = socket.Receive(buffer);
                    if (bytesRead == 4)
                        result = Encoding.ASCII.GetString(buffer, 0, bytesRead - 1); // skip \n
                    else result = "err";
                }
                catch (Exception exception) {
                    result = exception.ToString();
                    if (socket != null) {
                        socket.Close();
                        socket = null;
                    }
                    UpdateConnectionState(3, result);
                }
            }
            return result;
        }

        //Update connection state label (GUI).
        public void UpdateConnectionState(int state, string text)
        {
            // connectButton
            string butConText = "Connect";  // default text
            bool butConEnabled = true;      // default state
            Color color = Color.Red;        // default color
            // pinButton
            bool butPinEnabled = false;     // default state 

            //Set "Connect" button label according to connection state.
            if (state == 1)
            {
                butConText = "Please wait";
                color = Color.Orange;
                butConEnabled = false;
            } else
            if (state == 2)
            {
                butConText = "Disconnect";
                color = Color.Green;
                butPinEnabled = true;
            }
            //Edit the control's properties on the UI thread
            RunOnUiThread(() =>
            {
                textViewServerConnect.Text = text;
                if (butConText != null)  // text existst
                {
                    buttonConnect.Text = butConText;
                    textViewServerConnect.SetTextColor(color);
                    buttonConnect.Enabled = butConEnabled;
                }
                buttonChangePinState.Enabled = butPinEnabled;
                buttonKaku1.Enabled = butPinEnabled;
                buttonKaku2.Enabled = butPinEnabled;
                buttonKaku3.Enabled = butPinEnabled;
            });
        }

        //Update GUI based on Arduino response
        public void UpdateGUI(string result, TextView textview)
        {
            RunOnUiThread(() =>
            {
                if (result == "OFF") textview.SetTextColor(Color.Red);
                else if (result == " ON") textview.SetTextColor(Color.Green);
                else textview.SetTextColor(Color.White);  
                textview.Text = result;
            });
        }

        //update Kaku state GUI
        private void UpdateKakuGUI(string v)
        {
            textViewKaku1.Text = Convert.ToString(v[0]);
            textViewKaku2.Text = Convert.ToString(v[1]);
            textViewKaku3.Text = Convert.ToString(v[2]);
        }

        // Connect to socket ip/prt (simple sockets)
        public void ConnectSocket(string ip, string prt)
        {
            RunOnUiThread(() =>
            {
                if (socket == null)                                       // create new socket
                {
                    UpdateConnectionState(1, "Connecting...");
                    try  // to connect to the server (Arduino).
                    {
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(prt)));
                        if (socket.Connected)
                        {
                            UpdateConnectionState(2, "Connected");
                            timerSockets.Enabled = true;                //Activate timer for communication with Arduino     
                        }
                    } catch (Exception exception) {
                        timerSockets.Enabled = false;
                        if (socket != null)
                        {
                            socket.Close();
                            socket = null;
                        }
                        UpdateConnectionState(4, exception.Message);
                    }
	            }
                else // disconnect socket
                {
                    socket.Close(); socket = null;
                    timerSockets.Enabled = false;
                    UpdateConnectionState(4, "Disconnected");
                }
            });
        }

        //Close the connection (stop the threads) if the application stops.
        protected override void OnStop()
        {
            base.OnStop();
        }

        //Close the connection (stop the threads) if the application is destroyed.
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        //Prepare the Screen's standard options menu to be displayed.
        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            //Prevent menu items from being duplicated.
            menu.Clear();

            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        //Executes an action when a menu button is pressed.
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.exit:
                    //Force quit the application.
                    System.Environment.Exit(0);
                    return true;
                case Resource.Id.abort:
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        //Check if the entered IP address is valid.
        private bool CheckValidIpAddress(string ip)
        {
            if (ip != "") {
                //Check user input against regex (check if IP address is not empty).
                Regex regex = new Regex("\\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\\.|$)){4}\\b");
                Match match = regex.Match(ip);
                return match.Success;
            } else return false;
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
            } else return false;
        }
    }
}
