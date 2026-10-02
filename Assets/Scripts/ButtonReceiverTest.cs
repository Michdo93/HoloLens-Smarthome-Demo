using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using HUX.Focus;
using HUX.Interaction;
using HUX.Receivers;
using System;
using UnityEngine.VR.WSA;

namespace MqttTest
{ 
    public class ButtonReceiverTest : InteractionReceiver
    {

        //Verbindung
        public string ip = "192.168.0.5";
        public string publishPath = "/messages/commands/";
        public string subscribePath = "/messages/states/";


        private string errorMsg;
        private bool verbunden;

        private MqttClient client;
        private List<KeyValuePair<string, string>> DataFromPublishReceived;

        private bool internal_lampState1 = false;
        private bool internal_lampState2 = false;

        private AppBar toolbar;
        private MqttNamesCenter mqttnamesCenter;

        //HUX Example stuff
        public GameObject TextObjectState;
        public TextMesh txt;

        // Use this for initialization
        void Start()
        {
            //Setup Reciever Data Array
            DataFromPublishReceived = new List<KeyValuePair<string, string>>();

            VerbindungHerstellen();

            // if user diddnt assign a text prefab
            if (txt == null)
                txt = TextObjectState.GetComponentInChildren<TextMesh>();

            //TODO: App Bar exeptions if we do not hava an app bar in scene
            //look for our app Bar
            toolbar = ManipulationManager.Instance.ActiveAppBar;
            mqttnamesCenter = MqttNamesCenter.Instance;

        }

        //VERBINDUNG
        public void VerbindungHerstellen()
        {
            // create client instance 
            client = new MqttClient(ip);
            //client = new MqttClient(ip);

            // register to message received asd
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            // subscribe to the topic "
            client.Subscribe(new string[] { subscribePath + "#" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            try
            {
                client.Connect("UnityClient");
                verbunden = true;
            }
            catch
            {
                errorMsg = "Ein Fehler ist aufgetaucht";
                Debug.Log(errorMsg);
                verbunden = false;
            }
        }

        //Standard-Methode aus MQTT-Klasse 
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //so we can read recived messages in the debug log
            //Debug.Log(e.Topic + "  " + System.Text.Encoding.UTF8.GetString(e.Message));

            DataFromPublishReceived.Add(new KeyValuePair<string, string>(e.Topic, System.Text.Encoding.UTF8.GetString(e.Message)));
        }


        //ON CLICK
        public void switchLamp(string name)
        {

            if (name.Contains("1"))
            {
                if (internal_lampState1)
                {
                    MQTTPublish(name, "OFF");
                    internal_lampState1 = false;
                }
                else
                {
                    MQTTPublish(name, "ON");
                    internal_lampState1 = true;
                }
            }
            else { 

                if (internal_lampState2)
                {
                    MQTTPublish(name, "OFF");
                    internal_lampState2 = false;
                }
                else
                {
                    MQTTPublish(name, "ON");
                    internal_lampState2 = true;
                }
            }
        }

        //MQTT-Publish-Methode
        public void MQTTPublish(string item, string message)
        {
            Debug.Log("path: " + publishPath + ", item: " + item + ", message: " + message);
            client.Publish(publishPath + item, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }


        //HUXSTUFF
        protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
        {
            txt.text = obj.name + " : OnTapped";



            //if we are touching a toolbar
            //compare the singelton toolbar, w/ the object we get from theevent
            Debug.Log("compare toolbar");
            if (obj.GetComponentInParent<AppBar>().gameObject == toolbar.gameObject)
            {
                Debug.Log("save gameobject of toolbar");
                GameObject attachedObject = toolbar.BoundingBox.Target.gameObject;

               

                Debug.Log(obj.name + " : loste aus OnTapped");

                //1.check if a button from the toolbar was pressed
                if (obj.name.Equals("AppBarButton1"))
                {

                    //2. game obj from toolbar is where toolbar is currently attached
                    //toolbar.gameobject


                    Debug.Log("das game objekt der appbar heißt: " + attachedObject);

                    //3.get somehow the name of the Hue Lamp from the game object where we touched
                    //string MqttName = mqttnamesCenter.getMqttDeviceName(attachedObject);

                    //4. Send the on or of to thhe device
                    //switchLamp(MqttName);
                }

                //if Unlock button was pressed
                if (obj.name.Equals("Unlock"))
                {
                    Debug.Log("Worldanchor delete");
                    //remove the world ancor. otherwise the object cant be moved
                    DestroyImmediate(attachedObject.GetComponent<WorldAnchor>());
                }

                //if Lock button was pressed
                if (obj.name.Equals("Lock"))
                {
                    Debug.Log("Worldanchor save");
                    //put a world anchor to the game object, to lock it in place
                    attachedObject.AddComponent<WorldAnchor>();


                }
            }


            base.OnTapped(obj, eventArgs);
        }

        protected override void OnHoldStarted(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
        {
            txt.text = obj.name + " : OnHoldStarted";
            base.OnHoldStarted(obj, eventArgs);
        }

        protected override void OnFocusEnter(GameObject obj, FocusArgs args)
        {
            txt.text = obj.name + " : OnFocusEnter";
            base.OnFocusEnter(obj, args);
        }

        protected override void OnFocusExit(GameObject obj, FocusArgs args)
        {
            txt.text = obj.name + " : OnFocusExit";
            base.OnFocusExit(obj, args);
        }


    }
}


