﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// using UnityEngine.Networking;
using Vuforia;

public class ImageDetected : MonoBehaviour, ITrackableEventHandler {

    // Component attached to the ImageTracker object
    private TrackableBehaviour mTrackableBehaviour;
    public GameObject stage = null;

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        if(newStatus == TrackableBehaviour.Status.DETECTED ||
           newStatus == TrackableBehaviour.Status.TRACKED ||
           newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            this.positionStage();   
        }
    }

    // Use this for initialization
    void Start () {

        // register this as trackable event hadnler
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();

        if (mTrackableBehaviour)
        {
            Debug.Log("Trackable Behaviour component found");
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }
        else
        {
            Debug.Log("Unable to get Trackable Behaviour component");
        }
	}

    void positionStage()
    {
        stage.transform.position = transform.position;
        stage.transform.rotation = transform.rotation;
    }
}
