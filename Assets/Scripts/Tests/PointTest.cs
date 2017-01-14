using UnityEngine;
using System.Diagnostics;
using System.Collections;

public abstract class PointTest : MonoBehaviour
{
    public float m_sideLengthMultiplier = 1.0f;

    public Stopwatch Watch = new Stopwatch();

    protected float m_sideLength = 1.0f;

    protected Component[] m_results;

    public void SetSideLength(float p_sideLength)
    {
        m_sideLength = p_sideLength * m_sideLengthMultiplier;
    }

    public virtual Component[] RunTest(Vector2[] p_points, Component[] p_values, Vector2[] p_searches)
    {
        if ((m_results == null) || (m_results.Length != p_searches.Length))
            m_results = new Component[p_searches.Length];

        Watch.Start ();

        ClearTree();

        for (int i = 0; i < p_points.Length; i++)
        {
            this.FeedPoint (p_points [i].x, p_points [i].y, p_values[i]);
        }

        for (int i = 0; i < p_points.Length; i++)
        {
            m_results [i] = this.SearchPoint (p_points [i].x, p_points [i].y);
        }

        Watch.Stop ();

        return m_results;
    }

    public string GetName()
    {
        return GetType ().Name + "(" + m_sideLength + ")";
    }

    public Component[] GetResults()
    {
        return m_results;
    }

    protected abstract void ClearTree();
    protected abstract void FeedPoint(float p_x, float p_y, Component p_value);
    protected abstract Component SearchPoint(float p_x, float p_y);

    void Start()
    {
    }
}