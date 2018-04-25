using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    private string[] availableScenes = { "Galvanometer" };

	// Use this for initialization
	void Start () {
        SceneManager.LoadScene(availableScenes[0], LoadSceneMode.Additive);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
