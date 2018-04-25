using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SocketIO;

public class SocketHandler : MonoBehaviour {

    private static SocketIOComponent socket;

    private GameObject galvanometerTarget = null;
    private GameObject morsecodeTarget = null;

    void Start() {

        // find the root game object for each target
        galvanometerTarget = GameObject.Find("Galvanometer");
        morsecodeTarget = GameObject.Find("MorseCode");

        // get the SocketIOComponent
        GameObject go = GameObject.Find("SocketIO");

        // if the object can be found
        if (go != null)
        {

            // get the conponent and attach events
            socket = go.GetComponent<SocketIOComponent>();
            if (socket != null)
            {
                // connect
                socket.Connect();
                socket.On("hardware", OnHardware);
            }
            else
            {
                Debug.Log("No socket in scene");
            }
        }
        else
        {
            Debug.Log("No SocketIO in scene");
        }

    }


    /*
     * OnHardware - event listener for receiving messages from hardware
     * 
     */
    private void OnHardware(SocketIOEvent e)
    {
        string hardwareMessageType = e.data.GetField("type").str;

        // choose what action to take 
        switch (hardwareMessageType)
        {
            case "morsecode":
                handleMorseCodeMessage(e);
                break;
            case "galvanometer":
                handleGalvanometerMessage(e);
                break;
            default:
                Debug.Log("SocketIO Message not recognised!");
                break;
        }
    }

    /*
     * handleMorseCodeMessage - handle the morse messages   
     * 
     */
    private void handleMorseCodeMessage(SocketIOEvent e)
    {
        // set default char
        char morseSymbol = ' ';

        // read the char from socketIO message
        if (e.data.GetField("letter").str == ".") morseSymbol = '.'; // addToPath('.');
        if (e.data.GetField("letter").str == "-") morseSymbol = '-'; // addToPath('-');

        if (morseSymbol != ' ')
        {
            // TODO: Find the Tree 
            // this.addToPath(morseSymbol);
        }
    }

    /*
    * handleGalvanometerMessage - handle the galvanometer message
    * 
    */
    private void handleGalvanometerMessage(SocketIOEvent e)
    {
        if (galvanometerTarget == null)
        {
            // find the root game object for each target
            galvanometerTarget = GameObject.Find("Galvanometer");
        }

        if (galvanometerTarget != null)
        {
            Debug.Log("Galvanometer present");

            // retrieve the states
            JSONObject galvanometerStates = e.data.GetField("states");

            // TODO: Sanitise the incoming data

            // broadcast new states to target
            galvanometerTarget.BroadcastMessage("OnGalvanometer", galvanometerStates, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.Log("Galvanometer GameObject not present");
        }

    }
}
