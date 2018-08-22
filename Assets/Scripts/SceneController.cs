using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    public PositionList sceneList;
    public int startingScene = 0;
    public bool scenesLoaded = false;

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

            this.sceneList = JsonUtility.FromJson<PositionList>(www.downloadHandler.text);
            this.scenesLoaded = true;
            Debug.Log("Number of Positions: " + sceneList.positions.Length);

        }
    }
}
