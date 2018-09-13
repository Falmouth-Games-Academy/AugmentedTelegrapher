using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class PositionObject
{
    public string name;
    public float posx;
    public float posy;
    public float posz;
    public float rotx;
    public float roty;
    public float rotz;
    public float rotw;
    public float scale;
    
    public Vector3 Position()
    {
        return new Vector3(posx, posy, posz);
    }

    public Quaternion Rotation()
    {
        return new Quaternion(rotx, roty, rotz, rotw);
    }

    public void SetPosition(Vector3 pos)
    {
        posx = pos.x;
        posy = pos.y;
        posz = pos.z;

        Debug.Log(posx + "  " + posy + "  " + posz);
    }

    public void SetRotation(Quaternion rot)
    {
        rotx = rot.x;
        roty = rot.y;
        rotz = rot.z;
        rotw = rot.w;

        Debug.Log(rotx + "  " + roty + "  " + rotz + "  " + rotw);
    }
}



[System.Serializable]
public class PositionList
{
    public PositionObject[] positions;

    public PositionObject FindByString(string name)
    {
        for (int i = 0; i < this.positions.Length; i++)
        {
            if (positions[i].name == name)
            {
                return positions[i];
            }
        }
        return null;
    }

    public PositionObject[] CalculateOffsets(string main)
    {
        PositionObject[] offsets = new PositionObject[positions.Length];

        PositionObject mainObject = FindByString(main);
        Vector3 mainVector = mainObject.Position();
        Quaternion mainRotation = mainObject.Rotation();

        for (int i = 0; i < positions.Length; i++)
        {
            offsets[i] = new PositionObject();

            offsets[i].name = positions[i].name;
            Vector3 targetVector = positions[i].Position();
            offsets[i].SetPosition(targetVector - mainVector);

            offsets[i].SetRotation((mainRotation * Quaternion.Inverse(positions[i].Rotation()))); // get difference in rotation
        }

        return offsets;
    }
}

