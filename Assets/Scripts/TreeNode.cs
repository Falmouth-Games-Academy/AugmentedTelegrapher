using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TreeNode : MonoBehaviour {

    // root node context
    public static GameObject parent;

    // visual aspects
    public GameObject rightOrb;
    public GameObject leftOrb;
    public GameObject smallTreeNodePrefab;
    // dispay component for char
    public TextMeshPro orbText;

    // node info
    public char value = '*';
    public int index = 0;
    public int depth = 0;
    
    // LEFT child node instance vars
    GameObject leftNode;
    GameObject leftJoin;
    Vector3 leftNodePosition;
    Vector3 leftStartPosition;
    Vector3 leftEndPosition;

    // RIGHT child node instance vars
    GameObject rightNode;
    GameObject rightJoin;
    Vector3 rightNodePosition;
    Vector3 rightStartPosition;
    Vector3 rightEndPosition;

    // positioning of child nodes
    private float horizontalOffset = 20;
    private int verticalOffset = -7;

    // Traversal Vars
    public static char[] path = {'*', '0', '0', '0', '0', '0', '0'}; // one more than need be
    public static char currentChar = ' ';
    public static int pathInsertIndex = 1;
    public static int pathTraverseIndex = 0;


    /*
     * Input - takes a dot or dash and adds it to the path 
     * 
     */
    static public void Input(char value)
    {
        // check that aren't maxing the path
        if (pathInsertIndex < path.Length)
        {
            // add to the path and step an increment
            path[pathInsertIndex] = value;
            pathInsertIndex++;
            outputPath();

        }
    }

    /*
     * Traverse - only used by the root to begin the traversing 
     * 
     */
    public void Traverse()
    {
        // reset and step
        pathTraverseIndex = 0;
        Step();
    }

    /*
     * Step - step through the tree 
     * 
     */
    public void Step()
    {
        // check if we are not at the end of the path
        if (path[pathTraverseIndex] != '0')
        {
            // set currentChar to this nodes value
            currentChar = value;

            if (value != '*')
            {
                TextMeshPro[] textmeshPro = GetComponentsInChildren<TextMeshPro>();
                textmeshPro[0].faceColor = new Color32(255, 128, 0, 255);
            }

            // progress index for the next step
            pathTraverseIndex++;

            // traverse 
            TreeNode script = null;
            if (path[pathTraverseIndex] == '.')
            {
                // go left of left
                if (leftNode != null)
                {
                    script = leftNode.GetComponent<TreeNode>();
                }
            }

            if (path[pathTraverseIndex] == '-')
            {
                // go left of right
                if (rightNode != null)
                {
                    script = rightNode.GetComponent<TreeNode>();
                }
            }

            if (script != null)
            {
                script.Step();
            }
        }            
    }

    /*
     *  insert - insert another node into the tree. 
     *  i is the value mapped to the char for creating the tree
     *  v is the char
     *  d is the depth in the tree
     * 
     */
    public void insert (int i, char v, int d = 0)
    {
        // step depth
        d++;

        // check if we chould go LEFT
        if (i < index)
        {
            // make sure left node is instantiated
            if (leftNode == null)
            {
                // if the depth is greater than 3 use a smaller prefab
                if (depth >= 4)
                {
                    // init the left node (SMALL)
                    leftNode = Instantiate(smallTreeNodePrefab, parent.transform, true) as GameObject;// , transform, false);
                }
                else
                {
                    // init the left node (STANDARD)
                    leftNode = Instantiate(leftOrb, parent.transform, true) as GameObject; //b, transform, false);
                }

                // Set the values for the new node
                leftNode.name = "left-node-" + v;
                setChildValues(leftNode, i, v, d);
                positionLeftNode();
            }
            else
            {   
                // if left node already exists, pass this down to the next node
                insertToChild(leftNode, i, v, d);
            }
        }
        // check if we chould go Right
        else if (i > index) { 
            // make sure the right node is instantiated
            if (rightNode == null){
                // if depth is greater than three use a smaller prefab
                if (depth >= 4)
                {
                    // init the small right node
                    rightNode = Instantiate(smallTreeNodePrefab, parent.transform, true) as GameObject;
                }
                else
                {
                    // init the default right node
                    rightNode = Instantiate(rightOrb, parent.transform, true) as GameObject;   
                }

                // set the values for the new node
                rightNode.name = "right-node-" + v;
                setChildValues(rightNode, i, v, d);
                positionRightNode();
            }
            else
            {
               // if rightNode already exists pass it to the next one 
               insertToChild(rightNode, i, v, d);
            }
        }
    }

    /*
     * setValues - used to set the values of each new node
     * 
     */ 
    public void setValue (char v)
    {
        value = v;
        orbText.SetText("" + v);
    }

    /*
     * getValue - returns the char associated with this node
     * 
     */ 
    public char getValue()
    {
        return value;
    }

    /*
     * setIndex - sets the index of this node
     * 
     */ 
    public void setIndex (int i)
    {
        index = i;
    }

    /*
     * setDepth - set the depth of this node. 
     * + calculates the horizontal offsets as well
     * 
     */ 
    public void setDepth (int d)
    {
        depth = d;
        horizontalOffset = (Mathf.Round((80/(Mathf.Pow(2,d+2f))) * 100.0f) / 100f);
    }

    /*
     * used to pass all the vars for the next node
     * 
     */ 
    private void setChildValues(GameObject node, int i, char v, int d)
    {
        if (node != null)
        {
            var nodeScript = node.GetComponent<TreeNode>();
            if (nodeScript != null)
            {
                nodeScript.setValue(v);
                nodeScript.setIndex(i);
                nodeScript.setDepth(d);
                
            }
        }
    }

    private void insertToChild(GameObject node, int i, char v, int d)
    {
        if (node != null)
        {
            var nodeScript = node.GetComponent<TreeNode>();
            if (nodeScript != null)
            {
                nodeScript.insert(i, v, d);
            }
        }

    }

    private void positionLeftNode()
    {
        leftNode.transform.position = transform.position;
        leftNode.transform.position += new Vector3(-horizontalOffset, verticalOffset, 0);
        
        // create the left join positions
        leftStartPosition = Vector3.MoveTowards(transform.position, leftNode.transform.position, 1.3f);
        leftEndPosition = Vector3.MoveTowards(leftNode.transform.position, transform.position, 1.3f);

        leftJoin = new GameObject("leftJoin");
        leftJoin.transform.SetParent(parent.transform, true);
        LineRenderer leftLine = leftJoin.AddComponent<LineRenderer>();

        leftLine.material = new Material(Shader.Find("Sprites/Default"));
        leftLine.widthMultiplier = 0.01f;
        leftLine.positionCount = 2;
        leftLine.useWorldSpace = false;
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.2F, 0.3F, 0.4F, 0.5F), 0.0f), new GradientColorKey(new Color(0.5F, 0.8F, 0.9F, 0.5F), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );

        leftLine.colorGradient = gradient;

        leftLine.SetPosition(0, leftStartPosition);
        leftLine.SetPosition(1, leftEndPosition);
        
    }

    private void positionRightNode()
    {
       
        rightNode.transform.position = transform.position;
        rightNode.transform.position += new Vector3(horizontalOffset, verticalOffset, 0);

        
        // create the left join positions
        rightStartPosition = Vector3.MoveTowards(transform.position, rightNode.transform.position, 1.3f);
        rightEndPosition = Vector3.MoveTowards(rightNode.transform.position, transform.position, 1.3f);

        
        rightJoin = new GameObject("rightJoin");
        rightJoin.transform.SetParent(parent.transform, true);
        LineRenderer rightLine = rightJoin.AddComponent<LineRenderer>();

        rightLine.material = new Material(Shader.Find("Sprites/Default"));
        rightLine.widthMultiplier = 0.01f;
        rightLine.positionCount = 2;
        rightLine.useWorldSpace = false;

        // Create a gradient
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.2F, 0.3F, 0.4F, 0.5F), 0.0f), new GradientColorKey(new Color(0.5F, 0.8F, 0.9F, 0.5F), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );

        rightLine.colorGradient = gradient;

        rightLine.SetPosition(0, rightStartPosition);
        rightLine.SetPosition(1, rightEndPosition);
        
    }

    public static void setParent(GameObject p)
    {
        parent = p;
    }
    
    public static char getCurrentChar()
    {
        Debug.Log(currentChar);
        return currentChar;
    }

    public static void outputPath()
    {
        Debug.Log(new string(path));
    }
    
    public static void resetPath()
    {
        Debug.Log("Path Reset");
        pathInsertIndex = 1;
        for (int i = 1; i < path.Length; i++)
        {
            path[i] = '0';
        }
    }

    public void resetUI()
    {
        if (leftNode != null)
        {
            TreeNode leftScript = leftNode.GetComponent<TreeNode>();
            leftScript.resetTextColor();
            leftScript.resetUI();
        }

        if (rightNode != null)
        {
            TreeNode rightScript = rightNode.GetComponent<TreeNode>();
            rightScript.resetTextColor();
            rightScript.resetUI();
        }
    }

    public void resetTextColor()
    {
        TextMeshPro[] textmeshPro = GetComponentsInChildren<TextMeshPro>();
        textmeshPro[0].faceColor = new Color32(255, 255, 255, 255);
    }

}
