using UnityEngine;
using System.Collections;

public class ComponentQuadtreeTest2 : PointTest
{
    private ComponentQuadtree2<Component> m_tree;
    private int m_pass = 0;

    void Start()
    {
        m_tree = new ComponentQuadtree2<Component>(0.0f, 0.0f, m_sideLength, m_sideLength);
    }

    protected override void ClearTree ()
    {
        m_tree.Rebuild ();

        m_pass++;
    }

    protected override void FeedPoint (float p_x, float p_y, Component p_value)
    {
        if (m_pass == 1)
            m_tree.Add (p_x, p_y, p_value);
    }

    protected override Component SearchPoint (float p_x, float p_y)
    {
        return m_tree.ClosestTo (p_x, p_y).m_currentClosest;
    }
}