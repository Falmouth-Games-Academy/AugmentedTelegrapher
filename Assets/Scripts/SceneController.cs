using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    private string[] availableScenes = { "BinaryTree-morse-key", "Galvanometer-models" };
    public int startingScene = 0;

	// Use this for initialization
	void Start () {
        SceneManager.LoadScene(availableScenes[startingScene], LoadSceneMode.Additive);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
