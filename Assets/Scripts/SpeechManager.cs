using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.SceneManagement;


using SocketIO;

public class SpeechManager : MonoBehaviour
{
    private string sceneOne = "BinaryTree-hololens";
    private string sceneTwo = "BinaryTree-morse-key";

    
    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords;

    // Use this for initialization
    void Start()
    {
        

        keywords = new Dictionary<string, System.Action>();
        keywords.Add("Reset Morse Code", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("OnReset", 0, SendMessageOptions.DontRequireReceiver);
        });

        keywords.Add("Load Scene One", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("OnLoadSceneOne");
        });

        keywords.Add("Load Scene Two", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("OnLoadSceneTwo");
        });

        keywords.Add("Connect to server", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("OnConnectToServer");
        });


        // Tell the KeywordRecognizer about our keywords.
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // Register a callback for the KeywordRecognizer and start recognizing!
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
   


    async void clearScenes()
    {

        if (SceneManager.GetSceneByName(sceneOne).IsValid())
        {
            SceneManager.UnloadSceneAsync(sceneOne);
        }

        if (SceneManager.GetSceneByName(sceneTwo).IsValid())
        {
            SceneManager.UnloadSceneAsync(sceneTwo);
        }
    }

    // Called by SpeechManager when the user says the "Reset world" command
    void OnLoadSceneOne()
    {

        // RemoveMixedRealityJunk();
        Debug.Log("Loading New Scene - Scene 1 - Gesture");

        clearScenes();

        SceneManager.LoadScene("BinaryTree-hololens", LoadSceneMode.Additive);
    }

    // Called by SpeechManager when the user says the "Reset world" command
    void OnLoadSceneTwo()
    {
       // RemoveMixedRealityJunk();
        Debug.Log("Loading New Scene - Scene 2 - Morse Key");

        clearScenes();

        SceneManager.LoadScene("BinaryTree-morse-key", LoadSceneMode.Additive);
    }

    // Called by SpeechManager when the user says the "Reset world" command
    void OnConnectToServer()
    {
         
    }

    
}