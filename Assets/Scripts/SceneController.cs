using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    public PositionList rawPositionData;
    public PositionObject[] sceneList;
    public int startingScene = 0;
    public bool scenesLoaded = false;
    public GameObject stage = null;

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
            this.rawPositionData = JsonUtility.FromJson<PositionList>(www.downloadHandler.text);
            this.scenesLoaded = true;

            // orient the stage to datum 
            PositionObject datum = rawPositionData.FindByString("1-Datum");
           

            sceneList = rawPositionData.CalculateOffsets("1-Datum");

            this.displayScenes();

        }
    }

    void displayScenes()
    {
        for (int i = 0; i < this.sceneList.Length; i++)
        {
            StartCoroutine(this.AsyncSceneLoader(this.sceneList[i].name, i));
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

        newObject.transform.parent = stage.transform;

        Quaternion rot = this.sceneList[index].Rotation();
        newObject.transform.localRotation = Quaternion.Inverse(rot);

        // create a new position
        Vector3 newPosition = new Vector3(
            this.sceneList[index].posx * -1,
            this.sceneList[index].posy,
            this.sceneList[index].posz * -1
        );

        // position the new object
        newObject.transform.position = newPosition;

        // remove scene to tidy things up
        // by closing the scenes
        SceneManager.UnloadSceneAsync(sceneName);

    }
}
