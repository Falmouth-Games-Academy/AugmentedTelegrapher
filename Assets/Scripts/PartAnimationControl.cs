using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartAnimationControl : MonoBehaviour {

    public int partNumber = 0;

    private bool isFound = false;
    private Animator anim;
    int placeHash = Animator.StringToHash("place");
    int loseHash = Animator.StringToHash("lose");


    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
    }
	
	
    void OnGalvanometer(JSONObject states)
    {
        float val = states[partNumber - 1].n;

        if (val == 0 && isFound == false)
        {
            isFound = true;
            anim.SetTrigger(placeHash);
            Debug.Log("Place");
        }

        if (val == 1 && isFound == true)
        {
            isFound = false;
            anim.SetTrigger(loseHash);
            Debug.Log("Lose");
        }
    }
}
