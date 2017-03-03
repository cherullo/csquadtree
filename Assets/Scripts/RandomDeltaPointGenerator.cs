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
    public float m_maxDelta = 0.1f;

    int m_iterationCounter = 0;
    PointTest[] m_tests;
    Transform[] m_items;

    void Awake()
    {
        m_tests = GetTests (m_reference);

        SetSideLength (m_tests, m_sideLength);

        m_items = BuildItems (m_numItems, m_sideLength);
    }

    void Update()
    {
        m_iterationCounter++;

        Vector2[] positions = RandomizeTransformPositions (m_items, m_sideLength);
        Vector2[] searchPoints = BuildRandomPositions(m_numSearches, m_sideLength);

        for (int i = 0; i < m_tests.Length; i++)
        {
            ClearCache();

            m_tests [i].RunTest (positions, m_items, searchPoints);
        }

        if (m_reference != null)
        {
            CheckResults (m_reference, m_tests);
        }

        PrintResults (m_iterationCounter, m_tests);
    }

    void PrintResults (int p_iterationCounter, PointTest[] p_tests)
    {
        StringBuilder sb = new StringBuilder ();
        sb.Append ("Iteration: ").Append (p_iterationCounter);

        for (int i = 0; i < p_tests.Length; i++) 
        {
            double millis = p_tests[i].Watch.Elapsed.TotalMilliseconds / p_iterationCounter;

            sb.Append(" ")
                .Append (p_tests[i].GetName ())
                .Append(": ")
                .Append(string.Format("{0:0.0000}", millis));
        }

        UnityEngine.Debug.Log (sb.ToString ());
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
            return (x.enabled == false);
        });

        return tests.ToArray ();
    }

    private Transform[] BuildItems(int p_numItems, float p_sideLength)
    {
        Transform[] ret = new Transform[p_numItems];

        for (int i = 0; i < p_numItems; i++)
        {
            GameObject ball = GameObject.CreatePrimitive (PrimitiveType.Sphere);
            ball.name = "Sphere " + i;
            ball.transform.localScale = 0.1f * Vector3.one;

            Vector2 temp;
            temp.x = Random.Range (0f, p_sideLength);
            temp.y = Random.Range (0f, p_sideLength);

            ball.transform.position = new Vector3(temp.x, temp.y, 0.0f);

            ret [i] = ball.transform;
        }

        return ret;
    }

    private void CheckResults (PointTest p_reference, PointTest[] p_tests)
    {
        Component[] results;
        Component[] expected = p_reference.GetResults ();

        for (int i = 0; i < p_tests.Length; i++) 
        {
            PointTest test = p_tests[i];

            if (test == p_reference)
                continue;

            results = test.GetResults();

            int index = getDifferentIndex (expected, results);

            if (index != -1) 
            {
                UnityEngine.Debug.LogError (test.GetName () + "  failed. Expected " + expected [index] + ", got " + results [index]);

                PaintBadResult(expected[index], results[index]);

                this.enabled = false;

                return;
            }
        }
    }

    private void ClearCache()
    {
        int[] buffer = new int[4000000];

        FakeUse(buffer);

        System.GC.Collect (System.GC.MaxGeneration, System.GCCollectionMode.Forced);
    }

    private bool FakeUse(int[] p_buffer)
    {
        return false;
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

    private Vector2[] RandomizeTransformPositions(Transform[] p_transforms, float p_sideLength)
    {
        Vector2[] ret = new Vector2[p_transforms.Length];

        for (int i = 0; i < p_transforms.Length; i++)
        {
            Vector3 pos = p_transforms [i].position;

            pos.x = Mathf.Clamp (pos.x + Random.Range (-m_maxDelta, m_maxDelta), 0f, p_sideLength);
            pos.y = Mathf.Clamp (pos.y + Random.Range (-m_maxDelta, m_maxDelta), 0f, p_sideLength);

            p_transforms [i].position = pos;
        
            ret [i].x = pos.x;
            ret [i].y = pos.y;
        }

        return ret;
    }

    private void PaintBadResult (Component p_expected, Component p_result)
    {
        p_expected.GetComponent<Renderer>().material.color = Color.blue;

        p_result.GetComponent<Renderer>().material.color = Color.red;
    }

    private Vector2[] BuildRandomPositions(int p_numPoints, float p_sideLength)
    {
        Vector2[] positions = new Vector2[p_numPoints];

        for (int i = 0; i < p_numPoints; i++)
        {
            positions [i].x = Random.Range (0f, p_sideLength);
            positions [i].y = Random.Range (0f, p_sideLength);
        }

        return positions;
    }
}