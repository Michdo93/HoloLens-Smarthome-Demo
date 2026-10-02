using HUX.Buttons;
using HUX.Interaction;
using HUX.Receivers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;

namespace MqttTest
{
    public delegate void ColorPickedDelegator(string mqttAdress, Color c);

    public class deviceReceiver : InteractionReceiver
    {
        //TESTBASE
        //ENDTESTBASE

        private AppBar toolbar;
        private MqttNamesCenter nameCenter = MqttNamesCenter.Instance;

        public GameObject colorpicker_prefab;
        public GameObject TextInformation_prefab;
        public bool colorpickerIsActive = false;

        // Use this for initialization
        void Start()
        {

            //look for our app Bar
            toolbar = ManipulationManager.Instance.ActiveAppBar;
        }


        //HUXSTUFF
        protected override void OnTapped(GameObject pressedObj, InteractionManager.InteractionEventArgs eventArgs)
        {
            //nothing clicked
            if (pressedObj == null) return;

            //if obj is touching a toolbar
            //compare the singelton toolbar, w/ the object we get from theevent
            //if (pressedObj.GetComponentInParent<AppBar>().gameObject == toolbar.gameObject)
            {

                //find the attached object , where the toolbar is
                GameObject attachedObject = toolbar.BoundingBox.Target.gameObject;
                
                //get the mqttname script from the game object
                MqttDevice mqttDevice = attachedObject.GetComponent<MqttDevice>();

                Debug.Log(mqttDevice);
                if (mqttDevice == null)
                    return;//something went wrong


                //get the mqttFunction out of the script
                MqttDevice.ButtonFunctionEnum bfe = MqttDevice.getButtonFunctionEnum(pressedObj.name);

                //get the mqtt Name out of the script
                string mqttAdress = mqttDevice.getMqttName(bfe);

                if(mqttAdress == null)
                {
                    Debug.Log("failed to get mqtt adress from the device");
                    return;
                }

                //now we have our mqtt adress and a Mqtt Function
                //for to each mqtt function something can happen.
                //dimmer --> slider
                //switch --> on off
                //color picker --> color picker
                //play --> play

                switch (bfe)
                {
                    case MqttDevice.ButtonFunctionEnum.Switch:

                        //different case for ResetWorldAnchors:
                        if (mqttAdress == "ResetWorldAnchors")
                        {
                            try
                            {
                                Debug.Log("ResetWorldAnchors Tapped");
                                foreach (WorldAnchor wa in FindObjectsOfType<WorldAnchor>())
                                {
                                    DestroyImmediate(wa);
                                }
                            }
                            catch
                            {
                                //do nothing, we are probably in editor
                            }

                        }
                        else
                        {

                            //get current state
                            State s = nameCenter.getMqttDeviceState(mqttAdress);

                            //get current text
                            CompoundButtonText text = pressedObj.GetComponent<CompoundButtonText>();

                            //if state not set yet, or state off then on, otherwise off
                            if (s == null || s.CurrentState.Equals("OFF"))
                            {
                                nameCenter.changeState(mqttAdress, bfe, "ON");
                                text.Text = "Off";
                            }
                            else
                            {
                                nameCenter.changeState(mqttAdress, bfe, "OFF");
                                text.Text = "On";
                            }
                        }

                        break;
                    case MqttDevice.ButtonFunctionEnum.RGBColor:

                        //if a colorpicker is active do not load
                        if (!colorpickerIsActive)
                        {
                            Debug.Log("colorpicker wird geladen");
                            colorpickerIsActive = true;

                            GameObject colorpicker = Instantiate(colorpicker_prefab, toolbar.transform.position - new Vector3(0,0.3f,0) , toolbar.transform.rotation);

                            //colorpicker will be an child of us
                            colorpicker.transform.parent = this.transform;

                            ColorPicker cp_script = colorpicker.GetComponentInChildren<ColorPicker>();

                            //sets the mqttadress
                            cp_script.setMqttAdress(mqttAdress);

                            //sets the methode, that will be called, when the color was picked
                            ColorPickedDelegator colorPicked = new ColorPickedDelegator(ColorPicked);
                            cp_script.setCallback(colorPicked);
                        }

                        break;
                    case MqttDevice.ButtonFunctionEnum.DimmerPlus:
                    case MqttDevice.ButtonFunctionEnum.DimmerMinus:

                        //get current state
                        State dim = nameCenter.getMqttDeviceState(mqttAdress);
                        int currentDimmer;

                        if (dim == null) // no state has been set yet
                        {
                            currentDimmer = 0;//%
                        }else 
                        {
                            //parsing went wrong
                            if (! int.TryParse(dim.CurrentState, out currentDimmer))
                                currentDimmer = 0;

                        }

                        if(bfe == MqttDevice.ButtonFunctionEnum.DimmerMinus)
                            currentDimmer -= 10;
                        else
                            currentDimmer += 10;

                        if (currentDimmer < 0)
                            currentDimmer = 0;
                        else if (currentDimmer > 100)
                            currentDimmer = 100;

                        nameCenter.changeState(mqttAdress, bfe, currentDimmer.ToString());

                        break;

                    case MqttDevice.ButtonFunctionEnum.JalouDown:
                        nameCenter.changeState(mqttAdress, bfe, "DOWN");
                        break;
                    case MqttDevice.ButtonFunctionEnum.JalouStop:
                        nameCenter.changeState(mqttAdress, bfe, "STOP");
                        break;
                    case MqttDevice.ButtonFunctionEnum.JalouUp:
                        nameCenter.changeState(mqttAdress, bfe, "UP");
                        break;
                    case MqttDevice.ButtonFunctionEnum.InformationItem:

                        foreach (TextMesh txtmsh in mqttDevice.GetComponentsInChildren<TextMesh>())
                        {
                            if(txtmsh.gameObject.name.Equals(mqttDevice.name + "_text"))
                            {
                                //if we have an txt mesh already attached, then destroy and exit
                                Debug.Log("Found Mesh");
                                Destroy(txtmsh.gameObject);
                                return;
                            }
                        }
                        //otherwise:

                        GameObject textInformation = Instantiate(TextInformation_prefab, toolbar.transform.position - new Vector3(0, - 0.15f, 0), toolbar.transform.rotation);
                        textInformation.name = mqttDevice.name + "_text"; //so we recognize it later

                        //make child of the device
                        textInformation.transform.parent = mqttDevice.transform;

                        string mqttAdress_withReturn = mqttAdress.Replace("\\n", "\n");
                        

                        //the text we want to display is in the mqtt adress
                        textInformation.GetComponent<TextMesh>().text = mqttAdress_withReturn;
                        break;
                    default:
                        break;

                }

            }

            base.OnTapped(pressedObj, eventArgs);
        }



        public void ColorPicked(string mqttAdress, Color color)
        {
            float H;
            float S;
            float V;

            //OpenHAB needs HSV
            Color.RGBToHSV(color, out H, out S, out V);

            //Open hab needs Values from 1 - 100/255
            int Hi = (int) (H * 360F);
            int Si = (int) (S * 100F);
            int Vi = (int) (V * 100F);


            //sentds a new color to the mqttCenter as r,g,b
            Debug.Log("picked color: " + Hi.ToString() + "," + Si.ToString() + "," + Vi.ToString());
            
            nameCenter.changeState(mqttAdress, MqttDevice.ButtonFunctionEnum.RGBColor, Hi.ToString() + "," + Si.ToString() + "," + Vi.ToString());

            //color is picked, so colorpicker isnt active anymore
            colorpickerIsActive = false;
        }


        public void notifyAppBarChanged()
        {
            //when app bar changed then destroy the current active colorpicker
            if (colorpickerIsActive)
            {
                ColorPicker cp_script = GetComponentInChildren<ColorPicker>();
                Destroy(cp_script.transform.parent.gameObject);

                colorpickerIsActive = false;
            }

        }

        
    }
}