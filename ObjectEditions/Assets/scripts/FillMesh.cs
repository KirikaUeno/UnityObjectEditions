using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillMesh : MonoBehaviour
{
    private List<Vector3> newVertices = new List<Vector3>();
    private List<int> newTriangles = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(obj.GetComponent<BoxCollider>());
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;

        Vector3[] verts = new Vector3[] { new Vector3(0, 0, 0), new Vector3(-1, 0, 1), new Vector3(-1, 0, 2), new Vector3(0,0,3), new Vector3(1,0,2),new Vector3(1,0,1) };
        addVerticies(verts);

        mesh.triangles = newTriangles.ToArray();
        mesh.vertices = newVertices.ToArray();
        mesh.RecalculateUVDistributionMetrics();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.Optimize();
        obj.AddComponent<MeshCollider>();
        obj.transform.position = new Vector3(0,1,0);
    }

    private void fillByTriangles(Vector3[] bounds)
    {
        if (bounds.Length < 3) return;
        else if(bounds.Length == 3)
        {
            addTriangle(bounds[0], bounds[1], bounds[2]);
            return;
        }
        Vector3[] verts = bounds;
        Vector3 initNorm = Vector3.Cross(verts[2] - verts[1], verts[1] - verts[0]);

        Line[] lines = new Line[verts.Length];

        for (int i = 1; i < verts.Length; i++) lines[i] = new Line(verts[i - 1], verts[i]);
        lines[0]=new Line(verts[verts.Length - 1], verts[0]);

        Vert[] verts1 = new Vert[verts.Length];
        for (int i = 0; i < verts.Length; i++) verts1[i] = new Vert();

        for (int i = 0; i < verts.Length - 2; i++)
        {
            verts1[i + 1].angle = Mathf.Asin(Vector3.Cross(verts[i + 2] - verts[i + 1], verts[i + 1] - verts[i]).magnitude / ((verts[i + 2] - verts[i + 1]).magnitude * (verts[i + 1] - verts[i]).magnitude));
            verts1[i + 1].angleSign = Vector3.Dot(Vector3.Cross(verts[i + 2] - verts[i + 1], verts[i + 1] - verts[i]), initNorm) >= 0 ? 1 : -1;
        }

        verts1[verts.Length - 1].angle = Mathf.Asin(Vector3.Cross(verts[0] - verts[verts.Length - 1], verts[verts.Length - 1] - verts[verts.Length - 2]).magnitude / ((verts[0] - verts[verts.Length - 1]).magnitude * (verts[verts.Length - 1] - verts[verts.Length - 2]).magnitude));
        verts1[verts.Length - 1].angleSign = Vector3.Dot(Vector3.Cross(verts[0] - verts[verts.Length - 1], verts[verts.Length - 1] - verts[verts.Length - 2]), initNorm) >= 0 ? 1 : -1;
        verts1[0].angle = Mathf.Asin(Vector3.Cross(verts[1] - verts[0], verts[0] - verts[verts.Length - 1]).magnitude / ((verts[1] - verts[0]).magnitude * (verts[0] - verts[verts.Length - 1]).magnitude));
        verts1[0].angleSign = Vector3.Dot(Vector3.Cross(verts[1] - verts[0], verts[0] - verts[verts.Length - 1]), initNorm) >= 0 ? 1 : -1;

        float angleSum = 0;
        for (int i = 0; i < verts.Length; i++) angleSum += verts1[i].angle * verts1[i].angleSign;

        int trueSign;
        if (Mathf.Abs(angleSum - Mathf.PI) < Mathf.Abs(angleSum + Mathf.PI)) trueSign = 1;
        else trueSign = -1;

        List<Vector3> newVerts = new List<Vector3>();

        int o = 1;
        while (o < verts.Length - 1)
        {
            if (isIntersect(new Line(verts[o + 1], verts[o - 1]),lines) || verts1[o].angleSign != trueSign)
            {
                newVerts.Add(verts[o - 1]);
                o++;
            }
            else
            {
                addTriangle(verts[o - 1], verts[o], verts[o + 1]);
                newVerts.Add(verts[o - 1]);
                o += 2;
            }
        }

        fillByTriangles(newVerts.ToArray());
    }

    private bool isIntersect(Line line, Line[] lines)
    {
        foreach(Line l in lines)
        {
            if (isLinesIntersect(line, l)) return true;
        }
        return false;
    }

    private bool isLinesIntersect(Line l1, Line l2)
    {
        if (isOnSameSide(l1.x1, l1.x2, l2) return false;
        else if()
    }

    private void addVerticies(Vector3[] verticies)
    {
        foreach (Vector3 vertex in verticies)
        {
            if (!newVertices.Contains(vertex))
            {
                newVertices.Add(vertex);
            }
        }
    }

    private void addTriangle(Vector3 x1, Vector3 x2, Vector3 x3)
    {
        newTriangles.Add(newVertices.IndexOf(x1));
        newTriangles.Add(newVertices.IndexOf(x2));
        newTriangles.Add(newVertices.IndexOf(x3));
    }
}
