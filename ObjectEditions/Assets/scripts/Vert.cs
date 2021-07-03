using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vert
{
    public float angle;
    public int angleSign;

    public Vert()
    {

    }
    public Vert(Vector3 v, float a, int aS)
    {
        this.angle = a;
        this.angleSign = aS;
    }
}
