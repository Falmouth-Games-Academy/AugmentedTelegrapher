using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[System.Serializable]
public class PositionObject
{
    public string name;
    public string posx;
    public string posy;
    public string posz;
    public string rotx;
    public string roty;
    public string rotz;
    public string scale;
    
}


[System.Serializable]
public class PositionList
{
    public PositionObject[] positions;
}

