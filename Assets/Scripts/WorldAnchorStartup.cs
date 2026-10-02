using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MqttTest
{
    public class WorldAnchorStartup : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
           
            //start and instantiate the manager, so it can load the anchor store
            WorldAnchorManager w = WorldAnchorManager.Instance;
        }

    }
}
