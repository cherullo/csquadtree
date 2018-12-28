using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Benchmark : MonoBehaviour
{
    public BruteForceTest Reference;
    public int m_numItems = 100;
    public int m_numSearches = 200;
    public float m_sideLength = 10.0f;

    [SerializeField]
    private int _iterationsToSkip = 10;

    private int m_iterationCounter = 0;
    private IPointTest[] _tests;
    private ResultsAggregator[] _aggregators;
    private Transform[] m_items;
    private IPointGenerator _pointGenerator;

    void Start()
    {
        _pointGenerator = GetComponent<IPointGenerator>();
        if (_pointGenerator == null)
            _BlowUp("No IPointGenerator in gameObject " + gameObject.name);

        m_sideLength = _pointGenerator.GetEffectiveSideLength(m_sideLength);

        _tests = GetTests (Reference);

        _aggregators = _CreateAggregators(_tests.Length);

        _Initialize (_tests, m_sideLength);

        m_items = BuildItems (m_numItems, m_sideLength);
    }

    private ResultsAggregator[] _CreateAggregators(int length)
    {
        ResultsAggregator[] ret = new ResultsAggregator[length];

        for (int i = 0; i < length; i++)
        {
            ret[i] = new ResultsAggregator();
        }

        return ret;
    }

    void Update()
    {
        m_iterationCounter++;

        Vector2[] keys = RandomizeTransformPositions (m_items, m_sideLength, _pointGenerator);
        Vector2[] searchPoints = BuildRandomPositions(m_numSearches, m_sideLength);

        RunResult referenceResult = null;
        if (Reference != null)
        {
            ClearCache();

            referenceResult = Reference.RunTest(keys, m_items, searchPoints);
        }

        for (int i = 0; i < _tests.Length; i++)
        {
            ClearCache();

            RunResult testResult = _tests [i].RunTest (keys, m_items, searchPoints);

            if (referenceResult != null)
                CheckResult(referenceResult, _tests[i], testResult);
            
            if (m_iterationCounter > _iterationsToSkip)
                _aggregators[i].FeedResult(testResult);
        }

        PrintResults (m_iterationCounter, _aggregators);
    }

    private Vector2[] RandomizeTransformPositions (Transform[] items, float sideLength, IPointGenerator pointGenerator)
    {
        Vector2[] positions = pointGenerator.GetPositions(items.Length, sideLength);

        for (int i = 0; i < items.Length; i++)
        {
            items[i].transform.position = positions[i];
        }

        return positions;
    }

    private void _BlowUp(string message)
    {
        enabled = false;
        throw new UnityException(message);
    }
    
    void PrintResults (int p_iterationCounter, ResultsAggregator[] aggregators)
    {
        StringBuilder sb = new StringBuilder ();
        sb.Append ("Iteration: ").Append (p_iterationCounter);

        for (int i = 0; i < aggregators.Length; i++) 
        {
            double millis = aggregators[i].Average;

            sb.Append(" ")
                .Append (_tests[i].GetName ())
                .Append(": ")
                .Append(string.Format("{0:0.0000}", millis));
        }

        UnityEngine.Debug.Log (sb.ToString ());
    }

    private void _Initialize(IPointTest[] tests, float sideLength)
    {
        foreach (IPointTest pt in tests)
        {
            pt.Initialize (sideLength);
        }
    }

    private IPointTest[] GetTests(BruteForceTest reference)
    {
        List<IPointTest> tests = new List<IPointTest> ();

        GetComponents<IPointTest>(tests);
        
        // tests.RemoveAll (x => {
        //     return (((MonoBehaviour)x).enabled == false);
        // });

        tests.Remove(reference as IPointTest);

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

            ret [i] = ball.transform;
        }

        return ret;
    }

    private bool CheckResult (RunResult reference, IPointTest test, RunResult testResult)
    {
        Component[] results = testResult.Results;
        Component[] expected = reference.Results;

        int index = GetDifferentIndex (expected, results);

        if (index != -1) 
        {
            PaintBadResult(expected[index], results[index]);

            _BlowUp(test.GetName () + "  failed. Expected " + expected [index] + ", got " + results [index]);

            return false;
        }

        return true;
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

    private int GetDifferentIndex(Component[] p_expected, Component[] p_results)
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

    private void PaintBadResult (Component p_expected, Component p_result)
    {
        if (p_expected != null)
            p_expected.GetComponent<Renderer>().material.color = Color.blue;

        if (p_result != null)
            p_result.GetComponent<Renderer>().material.color = Color.red;
    }

    
    protected virtual Vector2[] BuildRandomPositions(int p_numPoints, float p_sideLength)
    {
        Vector2[] positions = new Vector2[p_numPoints];

        for (int i = 0; i < p_numPoints; i++)
        {
            positions [i].x = UnityEngine.Random.Range (0f, p_sideLength);
            positions [i].y = UnityEngine.Random.Range (0f, p_sideLength);
        }

        return positions;
    }
}
