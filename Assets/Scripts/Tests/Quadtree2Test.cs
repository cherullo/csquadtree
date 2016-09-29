using UnityEngine;
using System.Collections;

public class Quadtree2Test : PointTest
{
    private Quadtree2<Component> m_tree;

    void Start ()
    {
        m_tree = new Quadtree2<Component>(0.0f, 0.0f, m_sideLength, m_sideLength);
    }

    protected override void ClearTree ()
    {
        m_tree.Clear ();
    }

    protected override void FeedPoint (float p_x, float p_y, Component p_value)
    {
        m_tree.Add(p_x, p_y, p_value);
    }

    protected override Component SearchPoint (float p_x, float p_y)
    {
        return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
    }
}
