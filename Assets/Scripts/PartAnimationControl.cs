using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartAnimationControl : MonoBehaviour {

    public int partNumber = 0;

    private bool isFound = true;  // starts off found 
    private Animator anim;
    int placeHash = Animator.StringToHash("place");
    int loseHash = Animator.StringToHash("lose");


    // Use this for initialization
    void Start() {
        anim = GetComponent<Animator>();
    }

    // only for debug
    void Update()
    {
        if (partNumber == 1 && Input.GetKeyDown(KeyCode.A))
        {
            togglePart();
        }

        if (partNumber == 2 && Input.GetKeyDown(KeyCode.S))
        {
            togglePart();
        }

        if (partNumber == 3 && Input.GetKeyDown(KeyCode.D))
        {
            togglePart();
        }

        if (partNumber == 4 && Input.GetKeyDown(KeyCode.F))
        {
            togglePart();
        }

        if (partNumber == 5 && Input.GetKeyDown(KeyCode.G))
        {
            togglePart();
        }
    }
	
    void togglePart()
    {
        if (isFound == false)
        {
            isFound = true;
            anim.SetTrigger(placeHash);
            Debug.Log("Place");
        }
        else
        {
            isFound = false;
            anim.SetTrigger(loseHash);
            Debug.Log("Lose");
        }
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
