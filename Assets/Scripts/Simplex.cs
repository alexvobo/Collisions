using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simplex
{
    public int numPoints = 0;
    public List<Vector3> points = new List<Vector3>();
    public Vector3 direction;


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
        var closestEdge = new Edge
        {
            distance = float.MaxValue
        };

        for (int i = 0; i < this.numPoints; i++)
        {
            var nextPoint = 0;

            if (numPoints != (i + 1))
            {
                nextPoint = i + 1;
            }

            // Get two points for creating the edge

            var A = points[i];
            var B = points[nextPoint];

            // Form the edge
            var edge = B - A;

            // Edge through origin
            //var E0 = (A * Vector3.Dot(edge, edge) - edge * (Vector3.Dot(A, edge)));
            var E0 = new Vector3(edge.z, 0.0f, -edge.x);

            // Edge through origin distance
            var d = Vector3.Dot(E0, A);

            // Update closestEdge if new distance is shorter than the edge we already found
            if (d < closestEdge.distance)
            {
                closestEdge.distance = d;
                closestEdge.index = nextPoint;
                closestEdge.direction = E0;
            }
        }

        return closestEdge;

    }
}
