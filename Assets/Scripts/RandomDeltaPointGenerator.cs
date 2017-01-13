using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class RandomDeltaPointGenerator : MonoBehaviour {

    public PointTest m_reference;
    public int m_numItems = 100;
    public int m_numSearches = 200;
    public float m_sideLength = 10.0f;

    int m_iterationCounter = 0;
    PointTest[] m_tests;
    Transform[] m_items;

    void Awake()
    {
        m_tests = GetTests (m_reference);

        SetSideLength (m_tests, m_sideLength);

        m_items = BuildItems (m_numItems);
    }

    private void SetSideLength(PointTest[] p_tests, float p_sideLength)
    {
        foreach (PointTest pt in p_tests)
        {
            pt.SetSideLength (p_sideLength);
        }
    }

    private PointTest[] GetTests(PointTest p_reference)
    {
        List<PointTest> tests = new List<PointTest> ();

        tests.AddRange(GetComponents<PointTest> ());

        tests.RemoveAll (x => {
            return (x.enabled == false || x == p_reference);
        });

        return tests.ToArray ();
    }

    private Transform[] BuildItems(int p_numItems)
    {
        Transform[] ret = new Transform[p_numItems];

        for (int i = 0; i < p_numItems; i++)
        {
            GameObject ball = GameObject.CreatePrimitive (PrimitiveType.Sphere);
            ball.name = "Sphere " + i;
            ball.transform.localScale = 0.1f * Vector3.one;

            Vector2 temp;
            temp.x = Random.Range (0f, m_sideLength);
            temp.y = Random.Range (0f, m_sideLength);

            ball.transform.position = new Vector3(temp.x, temp.y, 0.0f);

            ret [i] = ball.transform;
        }

        return ret;
    }

    void Update()
    {
        Vector2[] positions = RandomizeTransformPositions (m_items);
        Vector2[] searchPoints = BuildPositions(m_numSearches);

        System.GC.Collect (System.GC.MaxGeneration, System.GCCollectionMode.Forced);

        Component[] expected = null;

        if (m_reference != null)
        {
            m_reference.Clear ();
            m_reference.FeedItemsAndPositions (positions, m_items);
            expected = m_reference.SearchItemsClosestToPoints (searchPoints);
        }

        for (int i = 0; i < m_tests.Length; i++)
        {
            ClearCache();

            PointTest test = m_tests [i];

            test.Clear ();

            test.FeedItemsAndPositions (positions, m_items);

            Component[] results = test.SearchItemsClosestToPoints (searchPoints);

            if (expected != null)
            {
                int index = getDifferentIndex (expected, results);
                if (index != -1)
                {
                    UnityEngine.Debug.LogError (test.GetName () + "  failed. Expected " + expected [index] + ", got " + results [index]);

                    this.enabled = false;

                    return;
                }
            }
        }

        m_iterationCounter++;

        StringBuilder sb = new StringBuilder ();
        sb.Append ("Iteration: ").Append(m_iterationCounter);

        if (m_reference != null)
            AppendTest (sb, m_reference);

        for (int i = 0; i < m_tests.Length; i++)
        {
            AppendTest(sb, m_tests [i]);
        }

        UnityEngine.Debug.Log (sb.ToString ());
    }

    private void ClearCache()
    {
        int[] buffer = new int[4000000];

        FakeUse(buffer);

        System.GC.Collect (System.GC.MaxGeneration, System.GCCollectionMode.Forced);
    }

    bool FakeUse(int[] p_buffer)
    {
        return false;
    }

    private void AppendTest(StringBuilder p_stringBuilder, PointTest p_test)
    {
        double millis = p_test.Watch.Elapsed.TotalMilliseconds / m_iterationCounter;

        p_stringBuilder.Append(" ")
            .Append (p_test.GetName ())
            .Append(": ")
            .Append(string.Format("{0:0.0000}", millis));
    }

    private int getDifferentIndex(Component[] p_expected, Component[] p_results)
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

            pos.x = Mathf.Clamp (pos.x + Random.Range (-0.1f, 0.1f), 0f, m_sideLength);
            pos.y = Mathf.Clamp (pos.y + Random.Range (-0.1f, 0.1f), 0f, m_sideLength);

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

    private Vector2[] BuildPositions(int p_numPoints)
    {
        int max = p_numPoints;// Random.Range (K_MAX_ITENS / 2, K_MAX_ITENS);

        Vector2[] positions = new Vector2[max];

        for (int i = 0; i < max; i++)
        {
            positions [i].x = Random.Range (0f, m_sideLength);
            positions [i].y = Random.Range (0f, m_sideLength);
        }

        return positions;
    }


   
}