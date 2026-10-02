using HoloToolkit.Unity;
using HUX.Receivers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MqttTest
{
    /// <summary>
    /// This is a Singleton class for handling the global  Names for the Mqtt devices
    /// </summary>
    public class MqttNamesCenter
    {

        //list of names from the smarthome 
        /// <summary>
        /// Name,(function,state)
        /// States can be:
        /// Switch:"ON","OFF"
        /// Dimmer:
        /// RGBColor
        /// </summary>
        private Dictionary<string, State> deviceStates = 
            new Dictionary<string, State>();

        private MqttManager mqttManager;


        //Singelton pattern
        private static MqttNamesCenter instance;
        private MqttNamesCenter() { }
        public static MqttNamesCenter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MqttNamesCenter();
                }
                return instance;
            }
        }


        internal void setMqttSender(MqttManager mqttManager)
        {
            this.mqttManager = mqttManager;
        }

        /// <summary>
        /// Register the device with his mehode
        /// TODO: maybe a methode without the enum as parameter?
        /// </summary>
        public void changeState(string mqttName, MqttDevice.ButtonFunctionEnum bfe, string state)
        {
            lock (this)
            {
                //if the device state is known, remove the old entry
                if (deviceStates.ContainsKey(mqttName))
                {
                    deviceStates.Remove(mqttName);
                }

                //create a new one
                deviceStates.Add(mqttName, new State(bfe, state));

                //sends the current state via Mqtt
                mqttManager.MQTTPublish(mqttName, state);
            }
        }

        /// <summary>
        /// if the mqtt center tells that a new state has arrived then update current state in ourdictionary
        /// or create a new one
        /// </summary>
        public void stateChanged(string mqttName, string state)
        {
            lock (this)
            {
                MqttDevice.ButtonFunctionEnum bfe;
                //if the device state is known, remove the old entry
                if (deviceStates.ContainsKey(mqttName))
                {
                    bfe = deviceStates[mqttName].ButtonFunctionEnum;// save the old bfe
                    deviceStates.Remove(mqttName);
                }
                else
                {
                    bfe = MqttDevice.ButtonFunctionEnum.Unknown;
                }

                //create a new one
                deviceStates.Add(mqttName, new State(bfe, state));
            }
        }

            /// <summary>
            /// Get the state of a device with this methode
            /// returns null if no device is found or it does not have a state yet
            /// </summary>
        public State getMqttDeviceState(string mqttName)
        {
            lock (this)
            {

                try
                {
                    return deviceStates[mqttName];

                }
                catch
                {
                    //not found or no state
                    return null;
                }
            }
        }

    }
}
