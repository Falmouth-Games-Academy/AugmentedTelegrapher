using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part1AnimationScript : MonoBehaviour {

    private Animator anim;
    int placeHash = Animator.StringToHash("placePart1");

   
	// Use this for initialization
	void Start () {

        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.Q)) { 
           
            anim.SetTrigger(placeHash);
            Debug.Log("Trigger");
        }   
    }
}
