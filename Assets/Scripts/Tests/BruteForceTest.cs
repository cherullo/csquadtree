using UnityEngine;
using System.Collections;

public class BruteForceTest : PointTest 
{
    private Vector2[] m_points;
    private Component[] m_values;

	public override void FeedItemsAndPositions (Vector2[] p_points, Component[] p_values)
    {
        m_points = p_points;
        m_values = p_values;
    }

    public override void Clear ()
    {
    }

    protected override void ClearTree ()
    {
    }

    protected override void FeedPoint (float p_x, float p_y, Component p_value)
    {
    }

    protected override Component SearchPoint (float p_x, float p_y)
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
