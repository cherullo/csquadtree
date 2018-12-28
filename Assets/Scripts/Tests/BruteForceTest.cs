using UnityEngine;
using System.Diagnostics;
using System.Collections;

public class BruteForceTest : MonoBehaviour, IPointTest
{
    private Vector2[] m_points;
    private Component[] m_values;
    private Component[] _results;

    public RunResult RunTest(Vector2[] p_points, Component[] p_values, Vector2[] searches)
    {
        if ((_results == null) || (_results.Length != searches.Length))
            _results = new Component[searches.Length];

        Stopwatch watch = new Stopwatch();
        watch.Start ();

        m_points = p_points;
        m_values = p_values;

        for (int i = 0; i < searches.Length; i++)
        {
            _results [i] = this.SearchPoint (searches [i].x, searches [i].y);
        }

        watch.Stop ();

        return new RunResult(_results, watch.Elapsed.TotalMilliseconds);
    }

    public string GetName()
    {
        return gameObject.name + "." + GetType().FullName;
    }

    public void Initialize(float sideLength)
    {
    }

    protected Component SearchPoint (float p_x, float p_y)
    {
        int retIndex = 0;
        float minDistance = Mathf.Infinity;
        Vector2 searchPoint = new Vector2 (p_x, p_y);

        for (int i = 0; i < m_points.Length; i++)
        {
            float distance = (m_points [i] - searchPoint).sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                retIndex = i;
            }
        }

        return m_values[retIndex];
    }
}
