using UnityEngine;
using System.Collections;

namespace Impl.Qt1
{

public class Quadtree<T> : IQuadtree<T>
{
    private Node m_root;

    private SearchData<T> m_searchData = new SearchData<T> ();

    public void Clear ()
    {
        m_root = null;
    }

    public void Add (float p_keyx, float p_keyy, T p_value)
    {
        QuadNodeData<T> data = new QuadNodeData<T> (p_keyx, p_keyy, p_value);

        if (m_root == null)
        {
            m_root = new Node (data);
        }
        else
        {
            m_root.Add (data);
        }
    }

    public ISearchResult<T> ClosestTo (float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null)
    {
        m_searchData.SetData (p_keyx, p_keyy, p_filter);

        if (m_root != null)
        {
            m_root.Search (m_searchData);
        }

        m_searchData.m_currentDistance = Mathf.Sqrt (m_searchData.m_currentDistance);

        return m_searchData;
    }

    private class Node
    {
        private const int K_BUCKET_SIZE = 4;
        private const int K_RIGHT = 1;
        private const int K_TOP = 2;

        public QuadNodeData<T> m_data;

        private Node[] m_nodes;

        private QuadNodeData<T>[] m_bucket;
        private int m_bucketCount;

        public Node (QuadNodeData<T> p_data)
        {
            m_data = p_data;

            m_bucket = new QuadNodeData<T>[K_BUCKET_SIZE];
            m_bucketCount = 0;
        }

        public void Add (QuadNodeData<T> p_data)
        {
            if (m_bucket != null) // Bucket mode
            {
                if (m_bucketCount == K_BUCKET_SIZE) // Spill
                {
                    QuadNodeData<T>[] temp = m_bucket;
                    m_bucket = null;
                    m_nodes = new Node[4];

                    for (int i = 0; i < K_BUCKET_SIZE; i++)
                        Add (temp [i]);

                    Add (p_data);
                }
                else
                {
                    m_bucket [m_bucketCount++] = p_data;
                }
            }
            else // Tree mode
            {
                int quadrant = GetQuadrant (p_data.m_keyx, p_data.m_keyy);

                Node targetNode = m_nodes [quadrant];

                if (targetNode == null)
                {
                    m_nodes [quadrant] = new Node (p_data);
                }
                else
                {
                    targetNode.Add (p_data);
                }
            }
        }

        public void Search (SearchData<T> p_searchData)
        {
            p_searchData.Feed (m_data);

            if (m_bucket != null) // Bucket mode
            {
                for (int i = 0; i < m_bucketCount; i++)
                    p_searchData.Feed (m_bucket [i]);
            }
            else // Tree mode
            {
                Node[] nodes = m_nodes;
                int quadrant = GetQuadrant (p_searchData.m_keyx, p_searchData.m_keyy);

                if (nodes [quadrant] != null)
                {
                    nodes [quadrant].Search (p_searchData);
                }

                float temp = p_searchData.m_keyx - m_data.m_keyx;

                if ((temp * temp) < p_searchData.m_currentDistance)
                {
                    int index = quadrant ^ K_RIGHT;

                    if (nodes [index] != null)
                        nodes [index].Search (p_searchData);
                }

                temp = p_searchData.m_keyy - m_data.m_keyy;
                if ((temp * temp) < p_searchData.m_currentDistance)
                {
                    int index = quadrant ^ K_TOP;

                    if (nodes [index] != null)
                        nodes [index].Search (p_searchData);
                }    
            }
        }

        private int GetQuadrant (float p_keyx, float p_keyy)
        {
            int ret = 0;
            QuadNodeData<T> data = m_data;

            if (p_keyx > data.m_keyx)
                ret += K_RIGHT;

            if (p_keyy > data.m_keyy)
                ret += K_TOP;

            return ret;
        }
    }
}

public class QuadNodeData<T>
{
    public float m_keyx;
    public float m_keyy;
    public T m_value;

    public QuadNodeData (float p_keyx, float p_keyy, T p_value)
    {
        this.m_keyx = p_keyx;
        this.m_keyy = p_keyy;
        this.m_value = p_value;
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

    public void Feed (QuadNodeData<T> p_nodeData)
    {
        float distance = DistanceTo (p_nodeData);
        if (distance < m_currentDistance)
        {
            if ((m_filter != null) && (m_filter (p_nodeData.m_value) == false))
                return;

            m_currentDistance = distance;
            m_currentClosest = p_nodeData.m_value;
        }
    }


    private float DistanceTo (QuadNodeData<T> p_nodeData)
    {
        float distX = (m_keyx - p_nodeData.m_keyx);
        float distY = (m_keyy - p_nodeData.m_keyy);
        return distX * distX + distY * distY;
    }

}
}