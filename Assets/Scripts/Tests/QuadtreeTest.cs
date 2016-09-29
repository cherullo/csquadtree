using UnityEngine;
using System.Collections;

public class QuadtreeTest : PointTest
{
    private Quadtree<Component> m_tree;

    protected override void ClearTree ()
    {
        m_tree = new Quadtree<Component> ();
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