using HUX.Buttons;
using HUX.Interaction;
using HUX.Receivers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;

namespace MqttTest
{
    [RequireComponent(typeof(BoundingBoxTarget))]
    public class MqttDevice : MonoBehaviour
    {    
        //this object can have a toolbar attatched, so here we put the reference on it
        //TODO: make this invisible in inspector
        private AppBar toolbar;

        [SerializeField]
        private deviceReceiver receiver;


        /// <summary>
        /// this is for the inspector in unity, so a user can put different kinds of buttons on each device
        /// </summary>
        //TODO: make this so, that you just need to select the wanted function
        public functionButtonTemplate[] functionButtonTemplates;

        [System.Serializable]
        public struct functionButtonTemplate
        {
            public string mqttAdress;
            public ButtonFunctionEnum buttonFunction;

            public functionButtonTemplate(ButtonFunctionEnum buttonFunction,string mqttAdress)
            {
                this.buttonFunction = buttonFunction;
                this.mqttAdress = mqttAdress;
            }
        }


        // Use this for initialization
        void Start()
        {

            //set our name, for communication with the mqtt broker
            //


            //FOR THE APPBAR
            //TODO: App Bar exeptions if we do not hava an app bar in scene
            //look for our app Bar
            toolbar = ManipulationManager.Instance.ActiveAppBar;
        }


        //is called when object is tapped and app bar will be attached
        //TODO: Timing issues because 2 tappped() are called. if this one is called first, then nothing happens
        public void Tapped2()
        {
            //TODO: Device remove button from device receiver list. otherwise more and more objects get registered

            //we make shure, that the object that is tapped is now at the appBar
            if (this.gameObject == toolbar.BoundingBox.Target)
            {
                //tell the receiver, to end alle current actions with the app bar, because there was an appbar change
                receiver.notifyAppBarChanged();

                //gets all buttons currently on the appbar
                List<AppBarButton> apb = new List<AppBarButton>(toolbar.GetComponentsInChildren<AppBarButton>());

                int currentButtonPosition = 0;

                //for every button backwards
                for (int i = apb.Count - 1; i > -1; i--)
                {
                    //if the button is custom
                    if (apb[i].template.Type == AppBar.ButtonTypeEnum.Custom)
                    {
                        //remove the button
                        Destroy(apb[i].gameObject);
                        apb.RemoveAt(i);


                        //we have one less button, so tell that the appbar
                        toolbar.NumDefaultButtons--;
                    }
                    //give every undeleted button a new position, so button alignment is correct
                    else
                    {
                        //we dont count "done" and "Show" them for positioning
                        if (apb[i].template.Type != AppBar.ButtonTypeEnum.Done && apb[i].template.Type != AppBar.ButtonTypeEnum.Show)
                        {
                            apb[i].template.DefaultPosition = currentButtonPosition;
                            currentButtonPosition++;
                        }
                    }
                }

                //add as many buttons to the app bar as predefined in the inspector
                foreach(functionButtonTemplate FBtemplate in functionButtonTemplates)
                {
                    this.addButton(FBtemplate,ref currentButtonPosition);
                }
            }
        }

        private void addButton(functionButtonTemplate FBtemplate,ref int buttonPosition)
        {
            //set the icon properties according to users choises in the Inspector

            string name = "";
            string icon = "";
            string text = "";




            switch (FBtemplate.buttonFunction)
            {
                case ButtonFunctionEnum.Switch:
                    name = "Switch";
                    icon = "E700";

                    //get current state
                    State s = MqttNamesCenter.Instance.getMqttDeviceState(getMqttName(ButtonFunctionEnum.Switch));


                    //if state not set yet, or state off then on, otherwise off
                    if (s == null || s.CurrentState.Equals("OFF"))
                    {
                        text = "On";
                    }
                    else
                    {
                        text = "Off";
                    }

                    break;
                case ButtonFunctionEnum.RGBColor:
                    name = "RGBColor";
                    icon = "E71E";
                    text = "RGBColor";
                    break;
                case ButtonFunctionEnum.DimmerPlus:
                    name = "DimmerPlus";
                    icon = "E700";
                    text = "DimmerPlus";
                    break;
                case ButtonFunctionEnum.DimmerMinus:
                    name = "DimmerMinus";
                    icon = "E700";
                    text = "DimmerMinus";
                    break;
                case ButtonFunctionEnum.JalouUp:
                    name = "JalouUp";
                    icon = "E70E";
                    text = "Up";
                    break;
                case ButtonFunctionEnum.JalouStop:
                    name = "JalouStop";
                    icon = "E711";
                    text = "Stop";
                    break;
                case ButtonFunctionEnum.JalouDown:
                    name = "JalouDown";
                    icon = "E70D";
                    text = "Down";
                    break;
                case ButtonFunctionEnum.InformationItem:
                    name = "InformationItem";
                    icon = "E700";
                    text = "Info";
                    break;
                default:
                    name = "Unknown";
                    icon = "E700";
                    text = "Unknown";
                    break;
            }

            //create buttontemplate for appbar
            AppBar.ButtonTemplate ABtemplate = new AppBar.ButtonTemplate(AppBar.ButtonTypeEnum.Custom, name,
                icon, text, buttonPosition++, 0);
            //add receiver to it
            ABtemplate.EventTarget = receiver;
            //let appbar create that button
            toolbar.CreateButton(ABtemplate, null);
        }
        
        //if we are deselected and have no world anchor, then add one
        public void OnTargetDeselectedOLD()
        {
            if (GetComponent<WorldAnchor>() == null)
                gameObject.AddComponent<WorldAnchor>();

        }

        //if we are deselected and have no world anchor, then add one
        public void OnTargetSelectedOLD()
        {
            WorldAnchor wa = GetComponent<WorldAnchor>();
            if (wa != null)
            {
                DestroyImmediate(wa);
            }

        }

        public enum ButtonFunctionEnum
        {
            Switch,
            RGBColor,
            DimmerPlus,
            DimmerMinus,
            JalouUp,
            JalouStop,
            JalouDown,
            InformationItem,
            Unknown,
        }

        /// <summary>
        /// gives you a enum out of a string
        /// </summary>
        /// <param name="buttonFunction_str"></param>
        /// <returns></returns>
        public static ButtonFunctionEnum getButtonFunctionEnum(string buttonFunction_str)
        {
            //resolves a string to enum
            ButtonFunctionEnum buttonFunction;
            switch (buttonFunction_str)
            {
                case "Switch":
                    buttonFunction = ButtonFunctionEnum.Switch;
                    break;
                case "DimmerPlus":
                    buttonFunction = ButtonFunctionEnum.DimmerPlus;
                    break;
                case "DimmerMinus":
                    buttonFunction = ButtonFunctionEnum.DimmerMinus;
                    break;
                case "RGBColor":
                    buttonFunction = ButtonFunctionEnum.RGBColor;
                    break;
                case "JalouUp":
                    buttonFunction = ButtonFunctionEnum.JalouUp;
                    break;
                case "JalouStop":
                    buttonFunction = ButtonFunctionEnum.JalouStop;
                    break;
                case "JalouDown":
                    buttonFunction = ButtonFunctionEnum.JalouDown;
                    break;
                case "InformationItem":
                    buttonFunction = ButtonFunctionEnum.InformationItem;
                    break;
                default:
                    buttonFunction = ButtonFunctionEnum.Unknown;//dont know button function
                    break;

            }
            return buttonFunction;
        }

        /// <summary>
        /// gets the mqtt name for the button that was pressed on this device
        /// </summary>
        /// <param name="buttonFunction"> witch kind of button was pressed</param>
        /// <returns></returns>
        public string getMqttName(ButtonFunctionEnum buttonFunction)
        { 

            //try to get the mqtt adress from the buttons of this device
            foreach ( functionButtonTemplate fbt in functionButtonTemplates)
            {
                if (fbt.buttonFunction == buttonFunction)
                    return fbt.mqttAdress;
            }

            //did not find the button
            //TODO: Empty sting or null?
            return "";
        }

        public string getMqttName(string buttonFunction_str)
        {
            return getMqttName(getButtonFunctionEnum(buttonFunction_str));
        }





    }
}
