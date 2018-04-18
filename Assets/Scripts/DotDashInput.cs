using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotDashInput : MonoBehaviour {

    public BinaryTree tree;
    public char value;

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        // If the sphere has no Rigidbody component, add one to enable physics.
        if (!this.GetComponent<Rigidbody>())
        {
            Debug.Log(value);

            BinaryTree script = tree.GetComponent<BinaryTree>();
            script.addToPath(value);



        }
    }
}

