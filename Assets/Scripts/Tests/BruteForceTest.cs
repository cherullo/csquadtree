using UnityEngine;
using System.Collections;

public class BruteForceTest : PointTest 
{
    private Vector2[] m_points;
    private Component[] m_values;

    public override Component[] RunTest(Vector2[] p_points, Component[] p_values, Vector2[] p_searches)
    {
        if ((m_results == null) || (m_results.Length != p_searches.Length))
            m_results = new Component[p_searches.Length];

        Watch.Start ();

        m_points = p_points;
        m_values = p_values;

        for (int i = 0; i < p_points.Length; i++)
        {
            m_results [i] = this.SearchPoint (p_points [i].x, p_points [i].y);
        }

        Watch.Stop ();

        return m_results;
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
