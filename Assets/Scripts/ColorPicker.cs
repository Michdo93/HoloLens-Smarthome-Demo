// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using HoloToolkit.Unity.InputModule;
using HUX.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace MqttTest
{
    public class ColorPicker : InteractibleObject //, IFocusable, IInputClickHandler
    {
        public Renderer rendererComponent;

        public Color CurrentPickedColor;

        private ColorPickedDelegator colorPicked;
        private string mqttAdress;

        //will be called from the InteractionManager
        public void Tapped(InteractionManager.InteractionEventArgs eventArgs)
        {
            RaycastHit hit;
            Physics.Raycast(eventArgs.GazeRay, out hit);


            if (hit.transform.gameObject != rendererComponent.gameObject) { return; }

            var texture = (Texture2D)rendererComponent.material.mainTexture;

            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= texture.width;
            pixelUV.y *= texture.height;

            CurrentPickedColor = texture.GetPixel((int)pixelUV.x, (int)pixelUV.y);

            colorPicked(mqttAdress , CurrentPickedColor);

            //after we picked a color parent can be destroyed
            Destroy(this.transform.parent.gameObject);

        }

        //needs to be set bevore a color is picked
        public void setMqttAdress(string mqttAdress)
        {
            this.mqttAdress = mqttAdress;
        }

        //this defines the methode that is called when a color is picked
        internal void setCallback(ColorPickedDelegator colorPicked)
        {
            this.colorPicked = colorPicked;
        }
    }
}