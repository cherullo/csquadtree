using UnityEngine;
using System.Collections;
using System.Diagnostics;


public class CQuadTreeTest : MonoBehaviour {

    private const int K_MAX_ITENS = 100;
    private const int K_SEARCHES_PER_TREE = 200;

    PointTest<Transform>[] m_tests = new PointTest<Transform>[] {
        new OriginalQuadTreeTest<Transform>(),

        new QuadTree2Test<Transform>(),
        new QuadTree21Test<Transform>(),
        new QuadTree2_100x100Test<Transform>(),
        new ComponentQuadTreeTest<Transform>(),
        new ComponentQuadTreeTest2<Transform>()
    };

    Stopwatch m_bruteForceWatch = new Stopwatch();

    int m_iterationCounter = 0;
    Transform[] m_values;

    void Awake()
    {
        m_values = new Transform[K_MAX_ITENS];

        for (int i = 0; i < m_values.Length; i++)
        {
            GameObject ball = GameObject.CreatePrimitive (PrimitiveType.Sphere);
            ball.name = "Sphere " + i;
            ball.transform.localScale = 0.1f * Vector3.one;

            Vector2 temp;
            temp.x = Random.Range (0f, 10f);
            temp.y = Random.Range (0f, 10f);

            ball.transform.position = new Vector3(temp.x, temp.y, 0.0f);

            m_values [i] = ball.transform;
        }
    }

    void Update()
    {
        Vector2[] positions = RandomizeTransformPositions (m_values); // BuildPositions (K_MAX_ITENS);
        Vector2[] searchPoints = BuildPositions(K_SEARCHES_PER_TREE);

        System.GC.Collect (System.GC.MaxGeneration, System.GCCollectionMode.Forced);

        Transform[] expected = new Transform[K_SEARCHES_PER_TREE];

        m_bruteForceWatch.Start ();
        for (int i = 0; i < K_SEARCHES_PER_TREE; i++)
        {
            expected[i] = BruteForce (searchPoints[i], positions);
        }
        m_bruteForceWatch.Stop ();

        for (int i = 0; i < m_tests.Length; i++)
        {
            PointTest<Transform> test = m_tests [i];

            test.Watch.Start ();

            test.Clear ();

            test.FeedPoints (positions, m_values);

            Transform[] results = test.SearchPoints (searchPoints);

            test.Watch.Stop ();

            int index = getDifferentIndex (expected, results);
            if (index != -1)
            {
                UnityEngine.Debug.LogError (test.GetName() + "  failed. Expected " + expected[index] + ", got " + results[index]);

                // PrintResults (positions, searchPoints[index], expected [index], results [index]);
                this.enabled = false;
                return;
            }
        }

        m_iterationCounter++;

        System.Text.StringBuilder sb = new System.Text.StringBuilder ();
        sb.Append ("Iteration: " + m_iterationCounter);
        sb.Append (" BruteForce: ").Append(string.Format("{0:0.0000}", m_bruteForceWatch.Elapsed.TotalMilliseconds / m_iterationCounter));
        for (int i = 0; i < m_tests.Length; i++)
        {
            PointTest<Transform> test = m_tests [i];

            double millis = test.Watch.Elapsed.TotalMilliseconds / m_iterationCounter;

            sb.Append(" ")
                .Append (test.GetName ())
                .Append(": ")
                .Append(string.Format("{0:0.0000}", millis));
        }

        UnityEngine.Debug.Log (sb.ToString ());
    }

    private int getDifferentIndex(Transform[] p_expected, Transform[] p_results)
    {
        for (int i = 0; i < p_expected.Length; i++)
        {
            if (p_expected [i] != p_results [i])
            {
                return i;
            }
        }

        return -1;
    }

    private Vector2[] RandomizeTransformPositions(Transform[] p_transforms)
    {
        Vector2[] ret = new Vector2[p_transforms.Length];

        for (int i = 0; i < p_transforms.Length; i++)
        {
            Vector3 pos = p_transforms [i].position;

            pos.x = Mathf.Clamp (pos.x + Random.Range (-0.1f, 0.1f), 0f, 10f);
            pos.y = Mathf.Clamp (pos.y + Random.Range (-0.1f, 0.1f), 0f, 10f);

            p_transforms [i].position = pos;

            Vector2 temp;
            temp.x = pos.x;
            temp.y = pos.y;
            ret [i] = temp;
        }

        return ret;
    }

    private void PrintResults (Vector2[] p_positions, Vector2 p_searchPoint, int p_expected, int p_result)
    {
        for (int i = 0; i < p_positions.Length; i++)
        {
            GameObject ball = GameObject.CreatePrimitive (PrimitiveType.Sphere);
            ball.transform.localScale = 0.1f * Vector3.one;
            ball.transform.position = p_positions [i];

            Color color = Color.white;
            if (i == p_expected)
            {
                color = Color.blue;
            }
            else if (i == p_result)
            {
                color = Color.red;
            }

            ball.GetComponent<Renderer> ().material.color = color;
        }

        GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
        cube.transform.position = p_searchPoint;
        cube.transform.localScale = 0.1f * Vector3.one;
    }

    private Transform BruteForce(Vector2 p_searchPoint, Vector2[] p_positions)
    {
        int retIndex = 0;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < p_positions.Length; i++)
        {
            float distance = (p_positions [i] - p_searchPoint).sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                retIndex = i;
            }
        }

        return m_values[retIndex];
    }

    private Vector2[] BuildPositions(int p_numPoints)
    {
        int max = p_numPoints;// Random.Range (K_MAX_ITENS / 2, K_MAX_ITENS);

        Vector2[] positions = new Vector2[max];

        for (int i = 0; i < max; i++)
        {
            positions [i].x = Random.Range (0f, 10f);
            positions [i].y = Random.Range (0f, 10f);
        }

        return positions;
    }

    private abstract class PointTest<T>
    {
        public Stopwatch Watch = new Stopwatch();
       
        public void FeedPoints(Vector2[] p_points, T[] p_values)
        {
            for (int i = 0; i < p_points.Length; i++)
                this.FeedPoint (p_points [i].x, p_points [i].y, p_values[i]);
        }

        public T[] SearchPoints(Vector2[] p_points)
        {
            T[] ret = new T[p_points.Length];

            for (int i = 0; i < p_points.Length; i++)
                ret [i] = this.SearchPoint (p_points [i].x, p_points [i].y);

            return ret;
        }

        public abstract void Clear();
        public abstract string GetName();

        protected abstract void FeedPoint(float p_x, float p_y, T p_value);
        protected abstract T SearchPoint(float p_x, float p_y);
    }

    private class ComponentQuadTreeTest<T> : PointTest<T> where T : Component
    {
        private ComponentQuadTree<T> m_tree = new ComponentQuadTree<T>(0.0f, 0.0f, 10.0f, 10.0f);
        private int m_pass = 0;

        public override void Clear ()
        {
            m_tree.Rebuild ();

            m_pass++;
        }

        protected override void FeedPoint (float p_x, float p_y, T p_value)
        {
            if (m_pass == 1)
                m_tree.Add (p_x, p_y, p_value);

        }

        protected override T SearchPoint (float p_x, float p_y)
        {
            return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
        }

        public override string GetName ()
        {
            return "ComponentQuadTree";
        }
    }

    private class ComponentQuadTreeTest2<T> : PointTest<T> where T : Component
    {
        private ComponentQuadTree2<T> m_tree = new ComponentQuadTree2<T>(0.0f, 0.0f, 10.0f, 10.0f);
        private int m_pass = 0;

        public override void Clear ()
        {
            m_tree.Rebuild ();

            m_pass++;
        }

        protected override void FeedPoint (float p_x, float p_y, T p_value)
        {
            if (m_pass == 1)
                m_tree.Add (p_x, p_y, p_value);

        }

        protected override T SearchPoint (float p_x, float p_y)
        {
            return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
        }

        public override string GetName ()
        {
            return "ComponentQuadTree2";
        }
    }

    private class QuadTree2Test<T> : PointTest<T>
    {
        private Quadtree2<T> m_tree = new Quadtree2<T>(0.0f, 0.0f, 10.0f, 10.0f);

        public override void Clear ()
        {
            m_tree.Clear ();
        }

        public override string GetName ()
        {
            return "Quadtree2";
        }

        protected override void FeedPoint (float p_x, float p_y, T p_value)
        {
            m_tree.Add(p_x, p_y, p_value);
        }

        protected override T SearchPoint (float p_x, float p_y)
        {
            return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
        }
    }

    private class QuadTree21Test<T> : PointTest<T>
    {
        private Quadtree21<T> m_tree = new Quadtree21<T>(0.0f, 0.0f, 10.0f, 10.0f);

        public override void Clear ()
        {
            m_tree.Clear ();
        }

        public override string GetName ()
        {
            return "Quadtree21";
        }

        protected override void FeedPoint (float p_x, float p_y, T p_value)
        {
            m_tree.Add(p_x, p_y, p_value);
        }

        protected override T SearchPoint (float p_x, float p_y)
        {
            return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
        }
    }


    private class QuadTree2_100x100Test<T> : PointTest<T>
    {
        private Quadtree2<T> m_tree = new Quadtree2<T>(0.0f, 0.0f, 100.0f, 100.0f);

        public override void Clear ()
        {
            m_tree.Clear ();
        }

        public override string GetName ()
        {
            return "Quadtree2_100x100";
        }

        protected override void FeedPoint (float p_x, float p_y, T p_value)
        {
            m_tree.Add(p_x, p_y, p_value);
        }

        protected override T SearchPoint (float p_x, float p_y)
        {
            return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
        }
    }

    private class OriginalQuadTreeTest<T> : PointTest<T>
    {
        private Quadtree<T> m_tree;

        public override void Clear ()
        {
            m_tree = new Quadtree<T> ();
        }

        public override string GetName ()
        {
            return "Quadtree";
        }

        protected override void FeedPoint (float p_x, float p_y, T p_value)
        {
            m_tree.Add(p_x, p_y, p_value);
        }

        protected override T SearchPoint (float p_x, float p_y)
        {
            return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
        }
    }
}