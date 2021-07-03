using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubstractMesh : MonoBehaviour
{
    public float a = 1;

    private GameObject gObj;
    private Vector3 center;

    private int[] oldTriangles;
    private List<int> newTriangles;

    private Vector3[] oldVerticies;
    private List<Vector3> newVertices;

    private Vector3[] oldNormals;
    private List<Vector3> newNormals;

    private Vector3 normal = new Vector3(0, 0, 0);

    public Vector3 cubePos;
    public Vector3 inPointPos;
    public Vector3 outPointPos;
    public Vector3 intersection = new Vector3(0,0,0);

    // Start is called before the first frame update
    void Start()
    {
        cubePos = GameObject.Find("Cube").transform.position;
        inPointPos = GameObject.Find("InPoint").transform.position;
        outPointPos = GameObject.Find("OutPoint").transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                findSquareIntersection(hit.point, hit.triangleIndex, hit.transform.gameObject, ray.direction);
                //Debug.Log(hit.point.ToString());
                //Debug.Log(hit.transform.gameObject.ToString());

                /*cubePos = GameObject.Find("Cube").transform.position;
                inPointPos = GameObject.Find("InPoint").transform.position;
                outPointPos = GameObject.Find("OutPoint").transform.position;
                center = cubePos;
                a = 1;
                intersection = intersectionInPoint(inPointPos, outPointPos);*/
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        if (gObj != null) Gizmos.DrawWireCube(center+ gObj.transform.position, new Vector3(a, a, a));

        //Gizmos.DrawLine(inPointPos, outPointPos);
        //Gizmos.DrawSphere(intersection, 0.05f);
    }

    private void findSquareIntersection(Vector3 point, int triangleIndex, GameObject obj, Vector3 direction)
    {
        gObj = obj;
        center = point - obj.transform.position;
        Destroy(obj.GetComponent<MeshCollider>());
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;

        oldTriangles = mesh.triangles;
        newTriangles = new List<int>();

        oldVerticies = mesh.vertices;
        newVertices = new List<Vector3>();

        oldNormals = mesh.normals;
        newNormals = new List<Vector3>();

        int j = 0;
        while (j < oldTriangles.Length)
        {
            Vector3 x1 = oldVerticies[oldTriangles[j]];
            Vector3 x2 = oldVerticies[oldTriangles[j + 1]];
            Vector3 x3 = oldVerticies[oldTriangles[j + 2]];
            //normal = (oldNormals[oldTriangles[j]] + oldNormals[oldTriangles[j+1]] + oldNormals[oldTriangles[j+2]])/3;
            normal = new Vector3(0, 1, 0);
            if (isTriangleIntersect(x1, x2, x3))
            {
                bool x1Inside = isPointIn(x1);
                bool x2Inside = isPointIn(x2);
                bool x3Inside = isPointIn(x3);

                if ((x1Inside && !x2Inside && !x3Inside))
                {
                    if (!isLineIntersect(x2, x3)) fillMeshOneInside(x1, x2, x3);
                    else fillMeshOneInsideInter(x1, x2, x3);
                }
                else if ((x2Inside && !x1Inside && !x3Inside))
                {
                    if (!isLineIntersect(x1, x3)) fillMeshOneInside(x2, x3, x1);
                    else fillMeshOneInsideInter(x2, x3, x1);
                }
                else if ((x3Inside && !x2Inside && !x1Inside))
                {
                    if (!isLineIntersect(x1, x2)) fillMeshOneInside(x3, x1, x2);
                    else fillMeshOneInsideInter(x3, x1, x2);
                }

                else if (x1Inside && x2Inside)
                {
                    fillMeshTwoInside(x1, x2, x3);
                }
                else if (x2Inside && x3Inside)
                {
                    fillMeshTwoInside(x2, x3, x1);
                }
                else if (x1Inside && x3Inside)
                {
                    fillMeshTwoInside(x3, x1, x2);
                }

                else
                {
                    fillMeshAllOutside(x1, x2, x3);
                }
            }
            else if(isTriangleIn(x1,x2,x3))
            {
                
            }
            else if (j == triangleIndex * 3)
            {
                //cube is inside triangle - cube is sliced by the infinite plane
                /*
                 * count number of cube verticies above the plane (in the direction of plane's normale)- find the situation (=n)
                 * n=1 -> intersection = 3-angle
                 * m=2 -> 4-angle
                 * m=3 -> 5-angle
                 * m=4 -> 4-angle or 6angle
                 * m>4 -> inverse normale -> m=8-m;
                 */
                addVerticies(x1, x2, x3);
            }
            else
            {
                //far from the place of the action
                addVerticies(x1, x2, x3);
                addTriangle(x1, x2, x3);
            }
            j += 3;
        }

        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.normals = newNormals.ToArray();
        //mesh.uv = newUVs.ToArray();

        mesh.RecalculateUVDistributionMetrics();
        //mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.Optimize();
        obj.AddComponent<MeshCollider>();
    }

    private void addVerticies(params Vector3[] verticies)
    {
        foreach(Vector3 vertex in verticies)
        {
            if (!newVertices.Contains(vertex))
            {
                newVertices.Add(vertex);
                newNormals.Add(normal);
            }
        }
    }

    private void addTriangle(Vector3 x1, Vector3 x2, Vector3 x3)
    {
        newTriangles.Add(newVertices.IndexOf(x1));
        newTriangles.Add(newVertices.IndexOf(x2));
        newTriangles.Add(newVertices.IndexOf(x3));
    }

    private void fillMeshOneInside(Vector3 xIn, Vector3 x1Out, Vector3 x2Out)
    {
        Vector3 x1 = intersectionInPoint(xIn, x1Out);
        Vector3 x2 = intersectionInPoint(xIn, x2Out);
        if (isLineIntersect(x1, x2))
        {
            //find point on cube and in triangle (=xCube)
            //addVerticies(x1Out, x2Out, x2, x1, xCube);
            //addTriangle(x1, x1Out, xCube);
            //addTriangle(xCube, x1Out, x2Out);
            //addTriangle(x2, xCube, x2Out);
        }
        else
        {
            addVerticies(x1Out, x2Out, x2, x1);
            addTriangle(x1, x1Out, x2Out);
            addTriangle(x2Out, x2, x1);
        }
    }
    private void fillMeshOneInsideInter(Vector3 x1, Vector3 x2, Vector3 x3)
    {
        Vector3[] xInt = intersectionOutPoint(x2, x3).ToArray();
        Vector3 x12 = intersectionInPoint(x1, x2);
        Vector3 x13 = intersectionInPoint(x1, x3);
        addVerticies(x2,x3,x12,x13);
        addTriangle(x2, xInt[0], x12);
        addTriangle(xInt[1], x3, x13);
    }
    private void fillMeshTwoInside(Vector3 x1In, Vector3 x2In, Vector3 xOut)
    {
        Vector3 x1 = intersectionInPoint(x1In, xOut);
        Vector3 x2 = intersectionInPoint(x2In, xOut);
        if (isLineIntersect(x1, x2))
        {
            //find point on cube and in triangle (=xCube)
            //addVerticies(xOut, x1, x2, xCube);
            //addTriangle(xCube, x1, xOut);
            //addTriangle(x2, xCube, xOut);
        }
        else
        {
            addVerticies(xOut, x1, x2);
            addTriangle(x1, x2, xOut);
        }
    }
    private void fillMeshAllOutside(Vector3 x1, Vector3 x2, Vector3 x3)
    {
        bool isx1x2Intersect = isLineIntersect(x1, x2);
        bool isx1x3Intersect = isLineIntersect(x1, x3);
        bool isx3x2Intersect = isLineIntersect(x3, x2);
        Vector3[] x1x2Int;
        Vector3[] x1x3Int;
        Vector3[] x3x2Int;

        if ((isx1x2Intersect && !isx1x3Intersect && !isx3x2Intersect) || (!isx1x2Intersect && isx1x3Intersect && !isx3x2Intersect) || (!isx1x2Intersect && !isx1x3Intersect && isx3x2Intersect))
        {
            addVerticies(x1,x2,x3);
            addTriangle(x1,x2,x3);
        }
        else if (isx1x2Intersect && isx1x3Intersect && !isx3x2Intersect)
        {
            x1x2Int = intersectionOutPoint(x1, x2).ToArray();
            x1x3Int = intersectionOutPoint(x1, x3).ToArray();
            fill2intersections(x1,x2,x3, x1x2Int, x1x3Int);
        }
        else if (isx1x2Intersect && !isx1x3Intersect && isx3x2Intersect)
        {
            x1x2Int = intersectionOutPoint(x2, x1).ToArray();
            x3x2Int = intersectionOutPoint(x2, x3).ToArray();
            fill2intersections(x2, x3, x1, x3x2Int, x1x2Int);
        }
        else if (!isx1x2Intersect && isx1x3Intersect && isx3x2Intersect)
        {
            x1x3Int = intersectionOutPoint(x3, x1).ToArray();
            x3x2Int = intersectionOutPoint(x3, x2).ToArray();
            fill2intersections(x3, x1, x2, x1x3Int, x3x2Int);
        }
        else
        {
            x1x2Int = intersectionOutPoint(x1, x2).ToArray();
            x1x3Int = intersectionOutPoint(x1, x3).ToArray();
            x3x2Int = intersectionOutPoint(x3, x2).ToArray();

            addVerticies(x1, x2, x3, x1x2Int[0], x1x2Int[1], x1x3Int[0], x1x3Int[1], x3x2Int[0], x3x2Int[1]);
            addTriangle(x1, x1x2Int[0], x1x3Int[0]);
            addTriangle(x1x2Int[1], x2, x3x2Int[1]);
            addTriangle(x3, x1x3Int[1], x3x2Int[0]);
        }
    }
    private void fill2intersections(Vector3 common, Vector3 outer1, Vector3 outer2, Vector3[] intersect1, Vector3[] intersect2)
    {
        addVerticies(common, outer1, outer2, intersect1[0], intersect1[1], intersect2[0], intersect2[1]);
        addTriangle(common, intersect1[0], intersect2[0]);
        addTriangle(intersect1[1], outer1, outer2);
        addTriangle(outer2, intersect2[1], intersect1[1]);
    }

    private bool isPointIn(Vector3 point)
    {
        if ((point.x < (center.x + a / 2)) && (point.x > (center.x - a / 2))
                && (point.y < (center.y + a / 2)) && (point.y > (center.y - a / 2))
                && (point.z < (center.z + a / 2)) && (point.z > (center.z - a / 2)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool isTriangleIn(Vector3 x1, Vector3 x2, Vector3 x3)
    {
        if (isPointIn(x1) && isPointIn(x2) && isPointIn(x3)) return true;
        else return false;
    }

    private bool isLineIntersect(Vector3 x1, Vector3 x2)
    {
        if (isPointIn(x1)) return true;
        if (isPointIn(x2)) return true;

        Vector3[] points = new Vector3[10];
        points[0] = x1;
        points[9] = x2;
        
        for (int k = 1; k < 9; k++)
        {
            points[k] = points[k - 1] + (points[9] - points[0]) / 9;
            if (isPointIn(points[k]))
            {
                return true;
            }
        }
        return false;
    }

    private bool isTriangleIntersect(Vector3 x1, Vector3 x2, Vector3 x3)
    {
        int numberOfInPoints = 0;
        if (isPointIn(x1)) numberOfInPoints++;
        if (isPointIn(x2)) numberOfInPoints++;
        if (isPointIn(x3)) numberOfInPoints++;
        if (numberOfInPoints < 3 && numberOfInPoints > 0)
        {
            return true;
        }
        else if(numberOfInPoints==0 &&
            (isLineIntersect(x1, x2) || isLineIntersect(x2, x3) || isLineIntersect(x1, x3)))
        {
            return true;
        }
        else return false;
    }

    private Vector3 intersectionInPoint(Vector3 x1, Vector3 x2)
    {
        x1 -= center;
        x2 -= center;

        Vector3 a1 = new Vector3(0, a / 2, a / 2) - new Vector3(0, x1.y, x1.z);
        Vector3 a2 = new Vector3(0, a / 2, -a / 2) - new Vector3(0, x1.y, x1.z);
        Vector3 a3 = new Vector3(0, -a / 2, -a / 2) - new Vector3(0, x1.y, x1.z);
        Vector3 a4 = new Vector3(0, -a / 2, a / 2) - new Vector3(0, x1.y, x1.z);
        Vector3 xYZ = new Vector3(0, x2.y, x2.z) - new Vector3(0, x1.y, x1.z);
        Vector3 b1 = new Vector3(a/2, a / 2, 0) - new Vector3(x1.x, x1.y, 0);
        Vector3 b2 = new Vector3(-a/2, a / 2, 0) - new Vector3(x1.x, x1.y, 0);
        Vector3 b3 = new Vector3(-a/2, -a / 2, 0) - new Vector3(x1.x, x1.y, 0);
        Vector3 b4 = new Vector3(a/2, -a / 2, 0) - new Vector3(x1.x, x1.y, 0);
        Vector3 xXY = new Vector3(x2.x, x2.y, 0) - new Vector3(x1.x, x1.y, 0);
        Vector3 c1 = new Vector3(a / 2, 0 , a / 2) - new Vector3(x1.x, 0, x1.z);
        Vector3 c2 = new Vector3(-a / 2, 0 , a / 2) - new Vector3(x1.x, 0, x1.z);
        Vector3 c3 = new Vector3(-a / 2, 0 , -a / 2) - new Vector3(x1.x, 0, x1.z);
        Vector3 c4 = new Vector3(a / 2, 0, -a / 2) - new Vector3(x1.x, 0, x1.z);
        Vector3 xXZ = new Vector3(x2.x, 0, x2.z) - new Vector3(x1.x, 0, x1.z);
        int side;
        bool direct;
        Vector3 answer = new Vector3(0,0,0);
        if (xYZ.magnitude == 0)
        {
            if ((x2 - x1).x > 0)
            {
                //x+side
                side = 0;
                direct = true;
                //Debug.Log("1");
            }
            else
            {
                //x-side
                side = 0;
                direct = false;
                //Debug.Log("2");
            }
        }
        else if (xXY.magnitude == 0)
        {
            if ((x2 - x1).z > 0)
            {
                //z+side
                side = 2;
                direct = true;
                //Debug.Log("3");
            }
            else
            {
                //z-side
                side = 2;
                direct = false;
                //Debug.Log("4");
            }
        }
        else if (xXZ.magnitude == 0)
        {
            if ((x2 - x1).y > 0)
            {
                //y+side
                side = 1;
                direct = true;
                //Debug.Log("5");
            }
            else
            {
                //y-side
                side = 1;
                direct = false;
                //Debug.Log("6");
            }
        }
        else if (xYZ.z >= 0)
        {
            if (xYZ.y / xYZ.magnitude > a1.y / a1.magnitude)
            {
                //y+side
                float xysin = xXY.y / xXY.magnitude;
                if (xXY.x >= 0 && xysin < b1.y / b1.magnitude)
                {
                    //x+side
                    side = 0;
                    direct = true;
                    //Debug.Log("7");
                }
                else if (xXY.x < 0 && xysin < b2.y / b2.magnitude)
                {
                    //x-side
                    side = 0;
                    direct = false;
                    //Debug.Log("8");
                }
                else
                {
                    //y+side
                    side = 1;
                    direct = true;
                    //Debug.Log("9");
                }
            }
            else if (xYZ.y / xYZ.magnitude < a4.y / a4.magnitude)
            {
                //y-side
                float xysin = xXY.y / xXY.magnitude;
                if (xXY.x >= 0 && xysin > b4.y / b4.magnitude)
                {
                    //x+side
                    side = 0;
                    direct = true;
                    //Debug.Log("10");
                }
                else if (xXY.x < 0 && xysin > b3.y / b3.magnitude)
                {
                    //x-side
                    side = 0;
                    direct = false;
                    //Debug.Log("11");
                }
                else
                {
                    //y-side
                    side = 1;
                    direct = false;
                    //Debug.Log("12");
                }
            }
            else
            {
                //z+side
                float xzsin = xXZ.z / xXZ.magnitude;
                if (xXZ.x >= 0 && xzsin < c1.z / c1.magnitude)
                {
                    //x+side
                    side = 0;
                    direct = true;
                    //Debug.Log("13");
                }
                else if (xXZ.x < 0 && xzsin < c2.z / c2.magnitude)
                {
                    //x-side
                    side = 0;
                    direct = false;
                    //Debug.Log("14");
                }
                else
                {
                    //z+side
                    side = 2;
                    direct = true;
                    //Debug.Log("15");
                }
            }
        }
        else
        {
            if (xYZ.y / xYZ.magnitude > a2.y / a2.magnitude)
            {
                //y+side
                float xysin = xXY.y / xXY.magnitude;
                if (xXY.x >= 0 && xysin < b1.y / b1.magnitude)
                {
                    //x+side
                    side = 0;
                    direct = true;
                    //Debug.Log("16");
                }
                else if (xXY.x < 0 && xysin < b2.y / b2.magnitude)
                {
                    //x-side
                    side = 0;
                    direct = false;
                    //Debug.Log("17");
                }
                else
                {
                    //y+side
                    side = 1;
                    direct = true;
                    //Debug.Log("18");
                }
            }
            else if (xYZ.y / xYZ.magnitude < a3.y / a3.magnitude)
            {
                //y-side
                float xysin = xXY.y / xXY.magnitude;
                if (xXY.x >= 0 && xysin > b4.y / b4.magnitude)
                {
                    //x+side
                    side = 0;
                    direct = true;
                    //Debug.Log("19");
                }
                else if (xXY.x < 0 && xysin > b3.y / b3.magnitude)
                {
                    //x-side
                    side = 0;
                    direct = false;
                    //Debug.Log("20");
                }
                else
                {
                    //y-side
                    side = 1;
                    direct = false;
                    //Debug.Log("21");
                }
            }
            else
            {
                //z-side
                float xzsin = xXZ.z / xXZ.magnitude;
                if (xXZ.x >= 0 && xzsin > c4.z / c4.magnitude)
                {
                    //x+side
                    side = 0;
                    direct = true;
                    //Debug.Log("22");
                }
                else if (xXZ.x < 0 && xzsin > c3.z / c3.magnitude)
                {
                    //x-side
                    side = 0;
                    direct = false;
                    //Debug.Log("23");
                }
                else
                {
                    //z-side
                    side = 2;
                    direct = false;
                    //Debug.Log("24");
                }
            }
        }
        answer[side] = direct ? a / 2 : -a / 2;
        for (int i = 0; i < 3; i++)
        {
            if (i != side)
            {
                answer[i] = (x2 - x1)[i] * (answer[side]-x1[side]) / (x2 - x1)[side] + x1[i];
            }
        }
        return answer + center;
    }

    private List<Vector3> intersectionOutPoint(Vector3 x1, Vector3 x2)
    {
        List<Vector3> answer = new List<Vector3>();

        x1 -= center;
        x2 -= center;

        Vector3 l = x1;
        Vector3 r = x2;
        Vector3 midl;
        Vector3 midr;
        float fml;
        float fmr;
        bool q = true;
        while (q)
        {
            midl = 2 * l / 3 + r / 3;
            midr = l / 3 + 2 * r / 3;
            fml = vector3MaxModule(midl);
            fmr = vector3MaxModule(midr);
            if ((fml > a / 2) && (fmr > a / 2))
            {
                if (isLineIntersect(midl, midr))
                {
                    l = midl;
                    r = midr;
                }
                else if (fml > fmr) l = midl;
                else r = midr;
            }
            else if((fml > a / 2) || (fmr > a / 2))
            {
                answer.Add(intersectionInPoint(midl + center, midr + center));
                if (fml > a / 2)
                {
                    answer.Add(intersectionInPoint(midr + center, r + center));
                    q = false;
                }
                else
                {
                    answer.Add(intersectionInPoint(l + center, midl + center));
                    q = false;
                }
            }
            else
            {
                answer.Add(intersectionInPoint(l + center, midl + center));
                answer.Add(intersectionInPoint(midr + center, r + center));
                q = false;
            }
        }
        return answer;
    }

    private float vector3MaxModule(Vector3 v)
    {
        return Mathf.Max(Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y)), Mathf.Abs(v.z));
    }
}