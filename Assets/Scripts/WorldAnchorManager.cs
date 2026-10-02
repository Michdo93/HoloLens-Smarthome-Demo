using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VR.WSA.Persistence;


namespace MqttTest
{
    public class WorldAnchorManager
    {

        WorldAnchorStore store;

        //Singelton pattern
        private static WorldAnchorManager instance;
        private WorldAnchorManager() { startup(); }
        public static WorldAnchorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WorldAnchorManager();
                }
                return instance;
            }
        }

        public WorldAnchorStore Store
        {
            get
            {
                if (store != null)
                    return store;
                else
                    throw new Exception("Store wasnt loaded yet");
            }
        }

        //############################################
        //Event Publisher
        public UnityEvent storeAvailable = new UnityEvent();    // the Event

        //############################################


        //this asyncronously loads the store
        private void startup()
        {
            WorldAnchorStore.GetAsync(StoreLoaded);
        }



        //this will be called, when store is loaded
        private void StoreLoaded(WorldAnchorStore store)
        {
            this.store = store;

            Debug.Log("WorldAnchorManager" + ": Store now Loaded");

            //tell all subscribers
            storeAvailable.Invoke();
        }

    }
}
