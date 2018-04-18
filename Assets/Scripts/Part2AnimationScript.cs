using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part2AnimationScript : MonoBehaviour {

    private Animator anim;
    int placeHash = Animator.StringToHash("placePart2");

   
	// Use this for initialization
	void Start () {

        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.W)) { 
           
            anim.SetTrigger(placeHash);
            Debug.Log("Trigger");
        }   
    }
}
