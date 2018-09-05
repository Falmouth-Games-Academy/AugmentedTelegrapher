using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    public PositionList sceneList;
    public int startingScene = 0;
    public bool scenesLoaded = false;
    public GameObject stage = null;

    private PositionObject datum;
    private Vector3 datumVector;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(GetPositions());
    }

    IEnumerator GetPositions()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://192.168.0.102:5000/sqlite/position/all"))
        {

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Positions Loaded!");
            }

            // TODO: Validate sceneList before going on!
            this.sceneList = JsonUtility.FromJson<PositionList>(www.downloadHandler.text);
            this.scenesLoaded = true;

            this.datum = this.sceneList.FindByString("1-Datum");
            this.datumVector = new Vector3(
                -float.Parse(this.datum.posx), 
                -float.Parse(this.datum.posy), 
                -float.Parse(this.datum.posz));
            
            Debug.Log("Number of Positions: " + sceneList.positions.Length);
            this.displayScenes();

        }
    }

    void displayScenes()
    {
        for (int i = 0; i < this.sceneList.positions.Length; i++)
        {
            StartCoroutine(this.AsyncSceneLoader(this.sceneList.positions[i].name, i));
        }
    }


    IEnumerator AsyncSceneLoader(string sceneName, int index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // get the container object in new scene
        GameObject newObject = GameObject.Find(sceneName);

        newObject.transform.parent = this.stage.transform;

        // create a new position
        Vector3 newPosition = new Vector3(
            float.Parse(this.sceneList.positions[index].posx),
            float.Parse(this.sceneList.positions[index].posy),
            float.Parse(this.sceneList.positions[index].posz)
        );

        // position the new object
        newObject.transform.position = newPosition;

        Quaternion newRotation = new Quaternion(
            float.Parse(this.sceneList.positions[index].rotx),
            float.Parse(this.sceneList.positions[index].roty),
            float.Parse(this.sceneList.positions[index].rotz),
            float.Parse(this.sceneList.positions[index].rotw)
        );

        newObject.transform.rotation = newRotation;

        newObject.transform.position += this.datumVector;

        // remove scene to tidy things up
        // by closing the scenes
        SceneManager.UnloadSceneAsync(sceneName);

    }
}
