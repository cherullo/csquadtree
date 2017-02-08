using UnityEngine;
using System.Diagnostics;
using System.Collections;

public abstract class PointTest : MonoBehaviour
{
    public float m_sideLengthMultiplier = 1.0f;

    protected float m_sideLength = 1.0f;

    public Stopwatch Watch = new Stopwatch();

    public void SetSideLength(float p_sideLength)
    {
        m_sideLength = p_sideLength * m_sideLengthMultiplier;
    }

    public virtual void FeedItemsAndPositions(Vector2[] p_points, Component[] p_values)
    {
        Watch.Start ();

        for (int i = 0; i < p_points.Length; i++)
            this.FeedPoint (p_points [i].x, p_points [i].y, p_values[i]);

        Watch.Stop ();
    }

    public virtual Component[] SearchItemsClosestToPoints(Vector2[] p_points)
    {
        Watch.Start ();

        Component[] ret = new Component[p_points.Length];

        for (int i = 0; i < p_points.Length; i++)
            ret [i] = this.SearchPoint (p_points [i].x, p_points [i].y);

        Watch.Stop ();

        return ret;
    }

    public virtual void Clear()
    {
        Watch.Start ();

        ClearTree();

        Watch.Stop ();
    }

    public virtual string GetName()
    {
        return GetType ().Name + "(" + m_sideLength + ")";
    }

    protected abstract void ClearTree();
    protected abstract void FeedPoint(float p_x, float p_y, Component p_value);
    protected abstract Component SearchPoint(float p_x, float p_y);

    // This only exists to force the 'enabled' chackbox in the inspector
    void Start()
    {
    }
}