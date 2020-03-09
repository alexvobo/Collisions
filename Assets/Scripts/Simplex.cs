using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simplex
{
    public int numPoints = 0;
    public List<Vector3> points = new List<Vector3>();
    public Vector3 d;


    public void Remove(Vector3 point)
    {
        points.Remove(points.Find(p => p.Equals(point)));
        numPoints--;

    }
    public void Add(Vector3 point)
    {
        points.Add(point);
        numPoints++;

    }
    public void Add(Vector3 point, int index)
    {
        points.Insert(index, point);
        numPoints++;

    }
    public Vector3 GetLast()
    {
        return points[numPoints - 1];
    }
    public Edge ClosestEdge()
    {
        Edge closestEdge = new Edge
        {
            distance = float.MaxValue
        };

        for (int i = 0; i < this.numPoints; i++)
        {
            int nextPoint = 0;

            if (numPoints != (i + 1))
            {
                nextPoint = i + 1;
            }

            // Get two points for creating the edge

            Vector3 A = points[i];
            Vector3 B = points[nextPoint];

            // Form the edge
            Vector3 edge = B - A;

            // Vector that passes through the origin
            Vector3 A0 = -A;

            // Edge through origin
            //Vector3 E0 = (A0 * Vector3.Dot(edge, edge) - edge * (Vector3.Dot(A0, edge))).normalized;
            Vector3 E0 = new Vector3(edge.z, 0.0f, -edge.x);

            // Edge through origin distance
            float d = Vector3.Dot(E0, A);

            // Update closestEdge if new distance is shorter than the edge we already found
            if (d < closestEdge.distance)
            {
                closestEdge.distance = d;
                closestEdge.direction = E0;
                closestEdge.index = nextPoint;
            }
        }

        return closestEdge;

    }
}
