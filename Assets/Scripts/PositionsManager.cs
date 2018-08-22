using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;


public class PositionsManager : MonoBehaviour {

    public PositionList positionList;

	// Use this for initialization
	void Start () {
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
                Debug.Log("Form upload complete!");
            }

            this.positionList = JsonUtility.FromJson<PositionList>(www.downloadHandler.text);

            Debug.Log("Number of Positions: " + positionList.positions.Length);

        }
    }

    public PositionObject[] returnPositions(){
        
        if (this.positionList.positions.Length){
            return this.positionList.positions;
        } else {
            return false;
        }
    }

}
