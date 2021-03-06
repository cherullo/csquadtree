﻿using UnityEngine;
using System.Collections;

namespace Impl.Qt22
{

public struct QuadNodeData2<T>
{
    public float m_keyx;
    public float m_keyy;
    public T m_value;

    public QuadNodeData2 (float p_keyx, float p_keyy, T p_value)
    {
        this.m_keyx = p_keyx;
        this.m_keyy = p_keyy;
        this.m_value = p_value;
    }
}

public class Quadtree2_2<T> : QuadTree2_2Node<T>, IQuadtree<T>
{
    private SearchData<T> m_searchData = new SearchData<T>();

    private QuadNodeData2<T> m_roller;

    public Quadtree2_2 (float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY) : base (p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY)
    {
    }

    public void Add(float p_keyx, float p_keyy, T p_value)
    {
        m_roller.m_keyx = p_keyx;
        m_roller.m_keyy = p_keyy;
        m_roller.m_value = p_value;

        this.Add (ref m_roller);
    }

    public ISearchResult<T> ClosestTo(float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null)
    {
        m_searchData.SetData (p_keyx, p_keyy, p_filter);

        Search (m_searchData);

        m_searchData.m_currentDistance = Mathf.Sqrt (m_searchData.m_currentDistance);

        return m_searchData;
    }
}

public class QuadTree2_2Node<T>
{
    private const int K_BUCKET_SIZE = 4;
    private const int K_RIGHT = 1;
    private const int K_TOP = 2;

    private float m_bottomLeftX;
    private float m_bottomLeftY;
    private float m_topRightX;
    private float m_topRightY;

    private float m_centerX;
    private float m_centerY;

    private QuadTree2_2Node<T>[] m_nodes;
    private QuadNodeData2<T>[] m_bucket;
    private int m_bucketCount;

    public QuadTree2_2Node (float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY)
    {
        this.m_bottomLeftX = p_bottomLeftX;
        this.m_topRightX = p_topRightX;
        m_centerX = (p_bottomLeftX + p_topRightX) * 0.5f;

        this.m_bottomLeftY = p_bottomLeftY;
        this.m_topRightY = p_topRightY;
        m_centerY = (p_bottomLeftY + p_topRightY) * 0.5f;

        m_bucket = new QuadNodeData2<T>[K_BUCKET_SIZE];
        m_bucketCount = 0;
    }

    public void Clear()
    {
        m_bucketCount = 0;
    }

    public void Add(ref QuadNodeData2<T> p_data)
    {
        if (m_bucketCount <= K_BUCKET_SIZE) // Bucket mode
        {
            if (m_bucketCount == K_BUCKET_SIZE) // Spill
            {
                m_bucketCount++;

                if (m_nodes == null)
                    CreateChildNodes ();
                else
                {
                    m_nodes[0].Clear();
                    m_nodes[1].Clear();
                    m_nodes[2].Clear();
                    m_nodes[3].Clear();
                }

                for (int i = K_BUCKET_SIZE - 1; i >= 0; i--)
                {
                    Add (ref m_bucket [i]);
                }

                Add (ref p_data);
            }
            else
            {
                m_bucket [m_bucketCount++] = p_data;
            }
        }
        else // Tree mode
        {
            int quadrant = GetQuadrant (p_data.m_keyx, p_data.m_keyy);

            m_nodes [quadrant].Add (ref p_data);
        }
    }

    public void Search(SearchData<T> p_searchData)
    {
        if (m_bucketCount <= K_BUCKET_SIZE) // Bucket mode
        {
            for (int i = m_bucketCount - 1; i >= 0; i--)
                p_searchData.Feed (ref m_bucket [i]);
        }
        else // Tree mode
        {
            int quadrant = GetQuadrant (p_searchData.m_keyx, p_searchData.m_keyy);

            m_nodes [quadrant].Search (p_searchData);

            bool doneX = false;

            float temp = p_searchData.m_keyx - m_centerX;

            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_RIGHT;

                m_nodes [index].Search (p_searchData);

                doneX = true;
            }

            temp = p_searchData.m_keyy - m_centerY;
            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_TOP;

                m_nodes [index].Search (p_searchData);

                if (doneX == true)
                {
                    index = quadrant ^ (K_TOP | K_RIGHT);

                    m_nodes [index].Search (p_searchData);
                }
            }  
        }
    }

    private void CreateChildNodes()
    {
        m_nodes = new QuadTree2_2Node<T>[4];

        m_nodes [0]                 = new QuadTree2_2Node<T> (m_bottomLeftX, m_bottomLeftY, m_centerX, m_centerY);
        m_nodes [K_RIGHT]           = new QuadTree2_2Node<T> (m_centerX, m_bottomLeftY, m_topRightX, m_centerY);
        m_nodes [K_TOP]             = new QuadTree2_2Node<T> (m_bottomLeftX, m_centerY, m_centerX, m_topRightY);
        m_nodes [K_RIGHT | K_TOP]   = new QuadTree2_2Node<T> (m_centerX, m_centerY, m_topRightX, m_topRightY);
    }

    private int GetQuadrant(float p_keyx, float p_keyy)
    {
        int ret = 0;

        if (p_keyx > m_centerX)
            ret = K_RIGHT;

        if (p_keyy > m_centerY) 
            ret |= K_TOP;

        return ret;
    }
}

public class SearchData<T> : ISearchResult<T>
{
    public float m_keyx;
    public float m_keyy;
    public T m_currentClosest;
    public float m_currentDistance;
    public DQuadtreeFilter<T> m_filter;

    public SearchData ()
    {
    }

    public T GetResult ()
    {
        return m_currentClosest;
    }

    public float GetDistance ()
    {
        return m_currentDistance;
    }

    public void SetData (float m_keyx, float m_keyy, DQuadtreeFilter<T> p_filter = null)
    {
        this.m_keyx = m_keyx;
        this.m_keyy = m_keyy;
        this.m_filter = p_filter;

        this.m_currentClosest = default(T);
        this.m_currentDistance = Mathf.Infinity;
    }

    public void Feed (ref QuadNodeData2<T> p_nodeData)
    {
        float distance = DistanceTo (ref p_nodeData);
        if (distance < m_currentDistance)
        {
            if ((m_filter != null) && (m_filter (p_nodeData.m_value) == false))
                return;

            m_currentDistance = distance;
            m_currentClosest = p_nodeData.m_value;
        }
    }

    private float DistanceTo (ref QuadNodeData2<T> p_nodeData)
    {
        float distX = (m_keyx - p_nodeData.m_keyx);
        float distY = (m_keyy - p_nodeData.m_keyy);
        return distX * distX + distY * distY;
    }

}
}