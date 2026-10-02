using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using HUX.Interaction;

namespace MqttTest
{
    public class MqttManager : MonoBehaviour
    {

        //Verbindung
        public string ip = "192.168.0.5";
        public string publishPath = "/messages/commands/";
        public string subscribePath = "/messages/states/";


        private string errorMsg;
        private bool verbunden;

        private MqttClient client;
        private List<KeyValuePair<string, string>> DataFromPublishReceived;

        private AppBar toolbar;
        private MqttNamesCenter mqttnamesCenter;


        void Start()
        {

            //Setup Reciever Data Array
            DataFromPublishReceived = new List<KeyValuePair<string, string>>();

            VerbindungHerstellen();

            //TODO: App Bar exeptions if we do not hava an app bar in scene
            //look for our app Bar
            toolbar = ManipulationManager.Instance.ActiveAppBar;
            mqttnamesCenter = MqttNamesCenter.Instance;

            mqttnamesCenter.setMqttSender(this);

            StartCoroutine(keepAliveChecker());
        }

        IEnumerator keepAliveChecker()
        {
            while (true)
            {
                yield return new WaitForSeconds(20);

                //check connection
                if (!client.IsConnected)
                {
                    Debug.LogError("The Mqtt Client is not connected");
                    Debug.Log("Trying to reconnect");
                    VerbindungHerstellen();
                }


            }

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
                errorMsg = ("Mqtt konnte sich nicht verbinden");
                Debug.Log(errorMsg);
                verbunden = false;
            }
        }
        //Standard-Methode aus MQTT-Klasse 
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //so we can read recived messages in the debug log
            //if(e.Topic.Contains("HUE"))                Debug.Log(e.Topic + "  " + System.Text.Encoding.UTF8.GetString(e.Message));

            //this adds all new incomming states to the states, only adds last topic part
            mqttnamesCenter.stateChanged(e.Topic.Substring(e.Topic.LastIndexOf('/') + 1), System.Text.Encoding.UTF8.GetString(e.Message));

            //deprecated
            //DataFromPublishReceived.Add(new KeyValuePair<string, string>(e.Topic, System.Text.Encoding.UTF8.GetString(e.Message)));
        }

        //MQTT-Publish-Methode
        public void MQTTPublish(string item, string message)
        {
            Debug.Log("path: " + publishPath + ", item: " + item + ", message: " + message);
            client.Publish(publishPath + item, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }

        private void OnApplicationQuit()
        {
            client.Disconnect();   
        }

    }
}



