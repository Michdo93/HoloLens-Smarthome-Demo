using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using HoloToolkit.Unity.InputModule;
using System;






public class sendMqttMessage : MonoBehaviour,IInputClickHandler
{

    //Verbindung
    public string ip;
    public string publishPath;
    public string subscribePath;
    public int itempos;
    

    private string errorMsg;
    private bool verbunden;

    private MqttClient client;
    private List<KeyValuePair<string, string>> DataFromPublishReceived;

    private bool internal_lampState = false;

    // Use this for initialization
    void Start()
    {
        //Setup Reciever Data Array
        DataFromPublishReceived = new List<KeyValuePair<string, string>>();

        VerbindungHerstellen();

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
        catch (Exception e)
        {
            errorMsg = "Ein Fehler ist aufgetaucht";
            Debug.Log(errorMsg);
            verbunden = false;
        }
    }

    //Standard-Methode aus MQTT-Klasse 
    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {

        DataFromPublishReceived.Add(new KeyValuePair<string, string>(e.Topic, System.Text.Encoding.UTF8.GetString(e.Message)));
    }


    //ON CLICK
    public void OnInputClicked(InputClickedEventData eventData)
    {
        //Debug test
        Debug.Log("Sphere clicked");

        if (internal_lampState)
        {
            MQTTPublish("Multimediawand_HUE1_Toggle", "OFF");
            internal_lampState = false;
        }
        else
        {
            MQTTPublish("Multimediawand_HUE1_Toggle", "ON");
            internal_lampState = true;
        }
            


        eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.
    }

    //MQTT-Publish-Methode
    public void MQTTPublish(string item, string message)
    {
        Debug.Log("path: " + publishPath + ", item: " + item + ", message: " + message);
        client.Publish(publishPath + item, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }
}


