using HoloToolkit.Unity;
using HUX.Receivers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MqttTest
{
    public class State
    {
        private MqttDevice.ButtonFunctionEnum buttonFunctionEnum;
        private string currentState;


        //TESTBASE
        //For example, the parent state of a Switch would be a Dimmer if there is one
        private State parentState;

        public State ParentState
        {
            get
            {
                return parentState;
            }

            set
            {
                parentState = value;
            }
        }

        //END TESTBASE


        public State(MqttDevice.ButtonFunctionEnum buttonFunctionEnum, string currentState)
        {
            this.buttonFunctionEnum = buttonFunctionEnum;
            this.currentState = currentState;
        }

        public MqttDevice.ButtonFunctionEnum ButtonFunctionEnum
        {
            get
            {
                return buttonFunctionEnum;
            }

            set
            {
                buttonFunctionEnum = value;
            }
        }

        public string CurrentState
        {
            get
            {
                return currentState;
            }

            set
            {
                currentState = value;
            }
        }

        public bool Equals(State other)
        {
            return other != null &&
                   currentState.Equals(other.currentState);
        }
    }
}
