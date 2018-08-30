using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagePosition : MonoBehaviour {

    private Vector3 prevPosition = new Vector3();
    private string targetName = "1-Datum";

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        // get the gameobject
        GameObject target = GameObject.Find(targetName);
        if (target != null)
        {
            if (target.transform.position != this.prevPosition)
            {
                transform.position -= target.transform.position;
                this.prevPosition = target.transform.position;
                Debug.Log("Changed Position");
            }
        }
    }
}
