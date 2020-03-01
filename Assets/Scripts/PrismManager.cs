using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrismManager : MonoBehaviour
{
    public int prismCount = 10;
    public float prismRegionRadiusXZ = 5;
    public float prismRegionRadiusY = 5;
    public float maxPrismScaleXZ = 5;
    public float maxPrismScaleY = 5;
    public GameObject regularPrismPrefab;
    public GameObject irregularPrismPrefab;

    private KdTree<Prism> prisms = new KdTree<Prism>();
    private List<GameObject> prismObjects = new List<GameObject>();
    private GameObject prismParent;
    private Dictionary<Prism, bool> prismColliding = new Dictionary<Prism, bool>();

    private const float UPDATE_RATE = 0.5f;
    private Vector3 directionVector;

    #region Unity Functions

    void Start()
    {
        Random.InitState(0);    //10 for no collision

        prismParent = GameObject.Find("Prisms");
        for (int i = 0; i < prismCount; i++)
        {
            var randPointCount = Mathf.RoundToInt(3 + Random.value * 7);
            var randYRot = Random.value * 360;
            var randScale = new Vector3((Random.value - 0.5f) * 2 * maxPrismScaleXZ, (Random.value - 0.5f) * 2 * maxPrismScaleY, (Random.value - 0.5f) * 2 * maxPrismScaleXZ);
            var randPos = new Vector3((Random.value - 0.5f) * 2 * prismRegionRadiusXZ, (Random.value - 0.5f) * 2 * prismRegionRadiusY, (Random.value - 0.5f) * 2 * prismRegionRadiusXZ);

            GameObject prism = null;
            Prism prismScript = null;
            if (Random.value < 0.5f)
            {
                prism = Instantiate(regularPrismPrefab, randPos, Quaternion.Euler(0, randYRot, 0));
                prismScript = prism.GetComponent<RegularPrism>();
            }
            else
            {
                prism = Instantiate(irregularPrismPrefab, randPos, Quaternion.Euler(0, randYRot, 0));
                prismScript = prism.GetComponent<IrregularPrism>();
            }
            prism.name = "Prism " + i;
            prism.transform.localScale = randScale;
            prism.transform.parent = prismParent.transform;
            prismScript.pointCount = randPointCount;
            prismScript.prismObject = prism;

            prisms.Add(prismScript);
            prismObjects.Add(prism);
            prismColliding.Add(prismScript, false);
        }
        StartCoroutine(Run());
    }

    void Update()
    {

        #region Visualization

        DrawPrismRegion();
        DrawPrismWireFrames();

#if UNITY_EDITOR
        if (Application.isFocused)
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
#endif

        #endregion
    }

    IEnumerator Run()
    {
        yield return null;

        while (true)
        {
            foreach (var prism in prisms)
            {
                prismColliding[prism] = false;
            }

            foreach (var collision in PotentialCollisions())
            {
                if (CheckCollision(collision))
                {
                    prismColliding[collision.a] = true;
                    prismColliding[collision.b] = true;

                    ResolveCollision(collision);
                }
            }

            yield return new WaitForSeconds(UPDATE_RATE);
        }
    }

    #endregion

    #region Incomplete Functions

    private IEnumerable<PrismCollision> PotentialCollisions()
    {
        for (int i = 0; i < prisms.Count; i++)
        {
            Prism p = prisms[i];
            var nearestNeighbor = prisms.FindClosest(p.transform.position);
            Debug.Log(nearestNeighbor == p);
            if (!nearestNeighbor.Equals(p))
            {
                var checkPrisms = new PrismCollision();
                checkPrisms.a = p;
                checkPrisms.b = nearestNeighbor;


                Debug.DrawLine(p.transform.position, nearestNeighbor.transform.position, Color.red);
                yield return checkPrisms;
            }


        }
        yield break;
    }
    private Vector3 FarthestPoint(Vector3[] vertices, Vector3 d)
    {
        // Return the farthest point in vertices along direction d

        float highest = -float.MaxValue;
        Vector3 support = Vector3.zero;

        foreach (var v in vertices)
        {
            float dot = v.x * d.x + v.y * d.y;

            if (dot > highest)
            {
                highest = dot;
                support = v;
            }
        }
        //Debug.DrawLine(support, -support, Color.white);
        return support;
    }
    private Vector3 GetSupport(Prism A, Prism B, Vector3 d)
    {
        //Finds the support vector by subtracting the farthest point in B along -d from the farthest point in A along d, the opposite direction.
        return FarthestPoint(A.points, d) - FarthestPoint(B.points, -d);
    }
    public Vector3 TripleProduct(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(a, (Vector3.Cross(b, c)));
    }
    private bool CheckOrigin(Simplex s, Vector3 dir)
    {
        // Returns true if we have a collision, false if we dont
        // If we dont, updates the direction vector so that GJK can search for a new point to query.

        // Count of points in simplex.
        var simplexCount = s.pointCount;

        // A is the last point in our simplex
        var A = s.GetLast();

        // Negate A
        var A0 = -A;

        if (simplexCount == 2)
        {
            // 2 points is a line

            var B = s.GetLast();
            // Find AB
            var AB = B - A;
            // Find perpendicular of AB
            var abPerp = TripleProduct(AB, A0, AB);
            // set the new direction to perpendicular of AB so we can find another point along it and form a 3-Simplex (Triangle)
            directionVector = abPerp;
        }
        else if (simplexCount == 3)
        {
            // 3 points is a triangle.

            // Find the rest of the points in the simplex.
            var B = s.points[1];
            var C = s.points[0];
            // Find edges of triangle.
            var AB = B - A;
            var AC = C - A;
            // Find each edge's perpendicular.
            var abPerp = TripleProduct(AC, AB, AB);
            var acPerp = TripleProduct(AB, AC, AC);
            // Check for origin with perp. of AB
            if (Vector3.Dot(abPerp, A0) > 0)
            {
                // remove point c
                s.Remove(C);
                // set the new direction to perpendicular of AB so we can find a point that works, unlike C
                directionVector = abPerp;
                return false;
            }
            else
            {
                // Check for origin with perp. of AC
                if (Vector3.Dot(acPerp, A0) > 0)
                {
                    // Remove B from the simplex.
                    s.Remove(B);
                    // Set the new direction to perpendicular of AC so we can find a point that works, unlike B.
                    directionVector = acPerp;
                    return false;
                }
                else
                {
                    // Origin Found
                    return true;
                }
            }
        }

        return false;
    }
    private Simplex GJK(Prism prismA, Prism prismB, Vector3 dir)
    {
        Simplex s = new Simplex();

        //First point on the edge of the minkowski difference. 1-Simplex.\
        s.Add(GetSupport(prismA, prismB, dir));

        //Compute negative direction of d
        directionVector = -dir;

        while (true)
        {
            //Add the point to the simplex. 2-Simplex.
            s.Add(GetSupport(prismA, prismB, directionVector));

            if (Vector3.Dot(s.GetLast(), directionVector) <= 0)
            {
                //point does not pass origin so do not add it.
                return null;
            }
            else
            {
                //CheckOrigin automatically updates the directionVector on every call. If it doesnt that means we found the origin.
                if (CheckOrigin(s, directionVector))
                {
                    return s;
                }
            }
        }
    }
    private Vector3 EPA(Simplex s, Prism A, Prism B)
    {
        while (true)
        {
            Edge e = s.ClosestEdge();

            var point = GetSupport(A, B, e.normal);

            float d = Vector3.Dot(point, e.normal);
            if (d - e.distance < Mathf.Pow(10, 5))
            {

                var normal = e.normal;
                var depth = d;
                return normal * depth;
            }
            else
            {
                // we haven't reached the edge of the Minkowski Difference
                // so continue expanding by adding the new point to the simplex
                // in between the points that made the closest edge
                s.Add(point, e.index);
            }

        }
    }
    private bool CheckCollision(PrismCollision collision)

    {

        var prismA = collision.a;
        var prismB = collision.b;

        var centerA = prismA.transform.position;
        var centerB = prismB.transform.position;

        //Subtracts centers of both prisms to find the initial direction of the vector to perform GJK on.
        Vector3 d = centerB - centerA;
        Debug.Log(d);

        //collision.penetrationDepthVectorAB = d;

        // If GJK returns points we have a simplex, otherwise we have an empty list of vectors.
        Simplex GJKVector = GJK(prismA, prismB, d);
        if (GJKVector != null)
        {
            collision.penetrationDepthVectorAB = EPA(GJKVector, prismA, prismB);
            return true;
        }
        else
        {
            return false;
        }
        // return GJK(prismA, prismB, d) ? true : false;

    }

    #endregion

    #region Private Functions

    private void ResolveCollision(PrismCollision collision)
    {
        var prismObjA = collision.a.prismObject;
        var prismObjB = collision.b.prismObject;

        var pushA = -collision.penetrationDepthVectorAB / 2;
        var pushB = collision.penetrationDepthVectorAB / 2;

        prismObjA.transform.position += pushA;
        prismObjB.transform.position += pushB;

        Debug.DrawLine(prismObjA.transform.position, prismObjA.transform.position + collision.penetrationDepthVectorAB, Color.cyan, UPDATE_RATE);
    }

    #endregion

    #region Visualization Functions

    private void DrawPrismRegion()
    {
        var points = new Vector3[] { new Vector3(1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1) }.Select(p => p * prismRegionRadiusXZ).ToArray();

        var yMin = -prismRegionRadiusY;
        var yMax = prismRegionRadiusY;

        var wireFrameColor = Color.yellow;

        foreach (var point in points)
        {
            Debug.DrawLine(point + Vector3.up * yMin, point + Vector3.up * yMax, wireFrameColor);
        }

        for (int i = 0; i < points.Length; i++)
        {
            Debug.DrawLine(points[i] + Vector3.up * yMin, points[(i + 1) % points.Length] + Vector3.up * yMin, wireFrameColor);
            Debug.DrawLine(points[i] + Vector3.up * yMax, points[(i + 1) % points.Length] + Vector3.up * yMax, wireFrameColor);
        }
    }

    private void DrawPrismWireFrames()
    {
        for (int prismIndex = 0; prismIndex < prisms.Count; prismIndex++)
        {
            var prism = prisms[prismIndex];
            var prismTransform = prismObjects[prismIndex].transform;

            var yMin = prism.midY - prism.height / 2 * prismTransform.localScale.y;
            var yMax = prism.midY + prism.height / 2 * prismTransform.localScale.y;

            var wireFrameColor = prismColliding[prisms[prismIndex]] ? Color.red : Color.green;

            foreach (var point in prism.points)
            {
                Debug.DrawLine(point + Vector3.up * yMin, point + Vector3.up * yMax, wireFrameColor);
            }

            for (int i = 0; i < prism.pointCount; i++)
            {
                Debug.DrawLine(prism.points[i] + Vector3.up * yMin, prism.points[(i + 1) % prism.pointCount] + Vector3.up * yMin, wireFrameColor);
                Debug.DrawLine(prism.points[i] + Vector3.up * yMax, prism.points[(i + 1) % prism.pointCount] + Vector3.up * yMax, wireFrameColor);
            }
        }
    }

    #endregion

    #region Utility Classes

    private class PrismCollision
    {
        public Prism a;
        public Prism b;
        public Vector3 penetrationDepthVectorAB;
    }

    private class Tuple<K, V>
    {
        public K Item1;
        public V Item2;

        public Tuple(K k, V v)
        {
            Item1 = k;
            Item2 = v;
        }
    }

    #endregion
}
