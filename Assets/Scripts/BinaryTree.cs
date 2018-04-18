using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

using SocketIO;



public class BinaryTree : MonoBehaviour {

    public string socketServerAddress = "ws://192.168.0.103:3000/socket.io/?EIO=4&transport=websocket";
    public GameObject outputText;
    public GameObject treeNode;
    private GameObject rootNode;
    private TreeNode rootNodeScript;

    private Vector2 mousePos;
    private Vector3 screenPos;
    public Camera camera;
    public float speed;

    private int degs = 0;

    private float lastLetterInputTime = 0.0f;
    private float inputLetterDelay = 3;
    private float inputSpaceDelay = 5;
    private float inputEndDelay = 7;

    // decide what to do when
    private int inputStage = 0;    // 0=letters 1=space 1=new word

    private bool inputActive = false;

    private string phrase = "";

    public static SocketIOComponent socket;
    

    // Use this for initialization
    void Start () {

        GameObject go = GameObject.Find("SocketIO");

        if (go != null)
        {
            socket = go.GetComponent<SocketIOComponent>();

            if (socket != null)
            {
                socket.Connect();
                socket.On("hardware", OnHardware);
            }
            else
            {
                Debug.Log("No socket in scene");
            }
        }
        else
        {
            Debug.Log("No SocketIO in scene");
        }
       

        camera = Camera.main;
        initRoot();
    }

    // Update is called once per frame
    void Update() {


        if (Input.GetKeyDown(KeyCode.LeftArrow)) addToPath('.');

        if (Input.GetKeyDown(KeyCode.RightArrow)) addToPath('-');

        if (Input.GetKey(KeyCode.Delete))
        {
            TreeNode.resetPath();
            phrase = "";
            outputText.GetComponent<TextMeshPro>().text = phrase;

        }

        if (Input.GetKeyDown("space"))
        {
            rootNodeScript = rootNode.GetComponent<TreeNode>();
            if (rootNodeScript != null)
            {
                rootNodeScript.Traverse();
                //TreeNode.getCurrentChar();
                Debug.Log(TreeNode.currentChar);
            }
        }


        float nowTime = Time.time;

        // ADD LETTER
        if (inputActive && nowTime - lastLetterInputTime > inputLetterDelay)
        {
            rootNodeScript = rootNode.GetComponent<TreeNode>();
        
            // checked for space - if there is no space add onn
            phrase = phrase + TreeNode.currentChar;

            outputText.GetComponent<TextMeshPro>().text = phrase;

            TreeNode.resetPath();

            rootNodeScript.resetUI();

            inputActive = false;
            

            // send message for complete
           Debug.Log(phrase);
        }

        // FINISH THE SENTENCE
        if (inputActive && nowTime - lastLetterInputTime > inputEndDelay)
        {
            
        }

    }

    public void addToPath(char dir)
    {
        lastLetterInputTime = Time.time;
        inputActive = true;
        TreeNode.Input(dir);
        if (rootNodeScript != null)
        {
            rootNodeScript.Traverse();
        }
    }

    void initRoot()
    {
        rootNode = Instantiate(treeNode);
        TreeNode.setParent(rootNode);

        rootNode.name = "Root";

        rootNode.transform.SetParent(transform);

        if (rootNode != null)
        {
            rootNodeScript = rootNode.GetComponent<TreeNode>();
            if (rootNodeScript != null)
            {
                rootNodeScript.setValue('*');
                rootNodeScript.setIndex(32);   
            }
        }

        initTree();
    }

    void initTree()
    {
        rootNodeScript.insert(16,   'E');
        rootNodeScript.insert(48,   'T');

        rootNodeScript.insert(8,    'I');
        rootNodeScript.insert(24,   'A');
        rootNodeScript.insert(40,   'N');
        rootNodeScript.insert(56,   'M');
        
        rootNodeScript.insert(4,    'S');
        rootNodeScript.insert(12,   'U');
        rootNodeScript.insert(20,   'R');
        rootNodeScript.insert(28,   'W');
        rootNodeScript.insert(36,   'D');
        rootNodeScript.insert(44,   'K');
        rootNodeScript.insert(52,   'G');
        rootNodeScript.insert(60,   'O');
        
        rootNodeScript.insert(2,    'H');
        rootNodeScript.insert(6,    'V');
        rootNodeScript.insert(10,   'F');
        rootNodeScript.insert(14,   'Ü');
        rootNodeScript.insert(18,   'L');
        rootNodeScript.insert(22,   'Ä');
        rootNodeScript.insert(26,   'P');
        rootNodeScript.insert(30,   'J');
        rootNodeScript.insert(34,   'B');
        rootNodeScript.insert(38,   'X');
        rootNodeScript.insert(42,   'C');
        rootNodeScript.insert(46,   'Y');
        rootNodeScript.insert(50,   'Z');
        rootNodeScript.insert(54,   'Q');
        rootNodeScript.insert(58,   'Ö');
        rootNodeScript.insert(62,   '-');

        rootNodeScript.insert(1,    '5');
        rootNodeScript.insert(3,    '4');
        rootNodeScript.insert(5,    'Ś');
        rootNodeScript.insert(7,    '3');
        rootNodeScript.insert(9,    'É');
        rootNodeScript.insert(11,   '-');
        rootNodeScript.insert(13,   'Ð');
        rootNodeScript.insert(15,   '2');
        rootNodeScript.insert(17,   '&');
        rootNodeScript.insert(19,   'É');
        rootNodeScript.insert(21,   '+');
        rootNodeScript.insert(23,   '-');
        rootNodeScript.insert(25,   '-');
        rootNodeScript.insert(27,   'À');
        rootNodeScript.insert(29,   'ĵ');
        rootNodeScript.insert(31,   '1');
        rootNodeScript.insert(33,    '6');
        rootNodeScript.insert(35,   '=');
        rootNodeScript.insert(37,   '/');
        rootNodeScript.insert(39,   '-');
        rootNodeScript.insert(41,   'Ç');
        rootNodeScript.insert(43,   '-');
        rootNodeScript.insert(45,   '(');
        rootNodeScript.insert(47,   '-');
        rootNodeScript.insert(49,    '7');
        rootNodeScript.insert(51,   '-');
        rootNodeScript.insert(53,   'Ğ');
        rootNodeScript.insert(55,   'Ñ');
        rootNodeScript.insert(57,    '8');
        rootNodeScript.insert(59,   '-');
        rootNodeScript.insert(61,   '9');
        rootNodeScript.insert(63,   '0');

        transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        Vector3 pos = transform.position;
        pos.z = 60;
        pos.y = 0;
        pos.x = 6;
        transform.position = pos;
    }

    // Called by SpeechManager when the user says the "Reset world" command
    void OnReset()
    {
        TreeNode.resetPath();
        phrase = "";
        outputText.GetComponent<TextMeshPro>().text = phrase;
    }


    void OnMorseInput(char morseSymbol)
    {
        addToPath(morseSymbol);
    }

    public void OnHardware(SocketIOEvent e)
    {
        char morseSymbol = '-';

        if (e.data.GetField("letter").str == ".") morseSymbol = '.'; // addToPath('.');
        if (e.data.GetField("letter").str == "-") morseSymbol = '-'; // addToPath('-');

        Debug.Log("Symbol: " + morseSymbol);

        GameObject messageTarget = null;


        this.addToPath(morseSymbol);

    }
}
