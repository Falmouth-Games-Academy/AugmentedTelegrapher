using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fmod_staticSound_example : MonoBehaviour {

    //this is required to get a reference to the event from fmod
    [FMODUnity.EventRef]
    string switchE = "event:/Morse Key/Switch"; //event path can be found the fmod browser

    //we store an instance of the event so that we can call it
    FMOD.Studio.EventInstance switchEInst;

    void Start()
    {
        //we need to set the event to the instance and set the location of the instance (will only work on static objects)
        switchEInst = FMODUnity.RuntimeManager.CreateInstance(switchE);
        switchEInst.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(this.transform, GetComponent<Rigidbody>()));
    }

    void Update ()
    {
        if (Input.GetKeyUp("1"))
        {
            //play the sound
           switchEInst.start();
        }
	}
}
