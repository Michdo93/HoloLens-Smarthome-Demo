using MqttTest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;

namespace MqttTest
{
    public class WorldAnchorUser : MonoBehaviour
    {
        bool anchorLocated = false;

        // Use this for initialization
        void Start()
        {

            WorldAnchorStore store;
            //tries to get a world ancor and restores it
            try
            {
                //restores the anchor to place the object
                store = WorldAnchorManager.Instance.Store;

            }
            catch
            {
                //wait for the store to be loaded
                //subscribe
                Debug.Log(this.gameObject.name + ": waiting for store to be loaded");
                WorldAnchorManager.Instance.storeAvailable.AddListener(Start);
                return;
            }

            store.Load(this.gameObject.name, this.gameObject);
            Debug.Log(this.gameObject.name + ": Store now Loaded");



            WorldAnchor wa = this.GetComponent<WorldAnchor>();
            if (wa != null)
            {
#if DEBUG
                Color c = wa.isLocated ? Color.green : Color.yellow; //if already located, then true

                    //color yellow for debug
                    foreach (Renderer r in GetComponentsInChildren<Renderer>())
                    {
                        r.material.color = c;
                    }

                wa.OnTrackingChanged += onTrackingChanged;
#endif
                //see if the anchor can be located

                //TODO: Destroy only just before movement and after movement rstore
                //DestroyImmediate(wa);//destroys the anchotr so the object can be moved
            }

        }

        /// <summary>
        /// Will all be dirty handeld from the AppBar
        /// </summary>
        /// <param name="pauseStatus"></param>
        void OnApplicationPauseOLD(bool pauseStatus)
        {
            // Save data about holograms positioned by this world anchor

            //save anchors if located or if there an anchor
            if (anchorLocated == true || this.gameObject.GetComponent<WorldAnchor>() != null)
            {

                //delete Previous anchor from store, bevore storing new
                WorldAnchorManager.Instance.Store.Delete(this.gameObject.name);


                //when exiting an application, add an anchor to the device. Usially an anchor is already on an device. so only add one, 
                //if there is none attached
                WorldAnchor wa = this.gameObject.GetComponent<WorldAnchor>();
                if (wa == null)
                    wa = this.gameObject.AddComponent<WorldAnchor>();


                //store Anchor if there is one attached
                WorldAnchorManager.Instance.Store.Save(this.gameObject.name, wa);
                //Assert(this.savedRoot);
                //}
            }
        }

        private void onTrackingChanged(WorldAnchor w, bool located)
        {
            if (located)
            {
                anchorLocated = true;
#if DEBUG

                    //color green for debug
                    foreach (Renderer r in GetComponentsInChildren<Renderer>())
                    {
                        r.material.color = Color.green;
                    }

#endif

                //unsubscribe, we have been found
                Unsubscribe();
            }


        }

        public void Unsubscribe()
        {
            try
            {
                GetComponent<WorldAnchor>().OnTrackingChanged -= onTrackingChanged;
            }
            catch
            {

            }

        }


    }
}
