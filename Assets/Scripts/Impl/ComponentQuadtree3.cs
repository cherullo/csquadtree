using UnityEngine;
using System.Collections;

public class ComponentQuadtree3<T> : ComponentQuadtreeNode3<T>, IRebuildableQuadtree<T> where T : Component
{
    private SearchData<T> m_searchData = new SearchData<T>();

    public ComponentQuadtree3 (float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY) : base (null, p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY)
    {
        m_parent = null;
    }

    public void Add(float p_keyx, float p_keyy, T p_value)
    {
        ComponentQuadNodeData3<T> data = new ComponentQuadNodeData3<T> (p_keyx, p_keyy, p_value);

        this.Add (ref data);
    }

    public SearchData<T> ClosestTo(float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null)
    {
        m_searchData.SetData (p_keyx, p_keyy, p_filter);

        Search (m_searchData);

        m_searchData.m_currentDistance = Mathf.Sqrt (m_searchData.m_currentDistance);

        return m_searchData;
    }
}

public class ComponentQuadtreeNode3<T> where T : Component
{
    private const int K_BUCKET_SIZE = 6;
    private const int K_RIGHT = 1;
    private const int K_TOP = 2;

    protected ComponentQuadtreeNode3<T> m_parent;

    private float m_bottomLeftX;
    private float m_bottomLeftY;
    private float m_topRightX;
    private float m_topRightY;

    private float m_centerX;
    private float m_centerY;

    private int m_bucketCount;
    private ComponentQuadNodeData3<T>[] m_bucket;
    private ComponentQuadtreeNode3<T>[] m_nodes;

   // bool m_bucketMode = true;

    public ComponentQuadtreeNode3 (ComponentQuadtreeNode3<T> p_parent, float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY)
    {
        this.m_parent = p_parent;
        this.m_bottomLeftX = p_bottomLeftX;
        this.m_bottomLeftY = p_bottomLeftY;
        this.m_topRightX = p_topRightX;
        this.m_topRightY = p_topRightY;

        m_centerX = (m_bottomLeftX + m_topRightX) * 0.5f;
        m_centerY = (m_bottomLeftY + m_topRightY) * 0.5f;

        m_bucket = new ComponentQuadNodeData3<T>[K_BUCKET_SIZE];
        m_bucketCount = 0;
    }

    public void Clear()
    {
        // m_bucketMode = true;

        m_bucketCount = 0;
    }

    public void Add(ref ComponentQuadNodeData3<T> p_data)
    {
        if (m_bucketCount >= 0) // Bucket mode
        {
            if (m_bucketCount == K_BUCKET_SIZE) // Spill
            {
                m_bucketCount = -1;

                if (m_nodes == null)
                    CreateChildNodes ();
                else
                {
                    for (int i = 0; i < m_nodes.Length; i++)
                    {
                        m_nodes [i].Clear ();
                    }
                }

                ComponentQuadNodeData3<T>[] temp = m_bucket;

                for (int i = 0; i < K_BUCKET_SIZE; i++)
                {
                    Add (ref temp [i]);
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
        if (m_bucketCount >= 0) // Bucket mode
        {
            for (int i = 0; i < m_bucketCount; i++)
                p_searchData.Feed (ref m_bucket [i]);
        }
        else // Tree mode
        {
            ComponentQuadtreeNode3<T>[] nodes = m_nodes;
            int quadrant = GetQuadrant (p_searchData.m_keyx, p_searchData.m_keyy);

            nodes [quadrant].Search (p_searchData);

            int doneX = 0;

            float temp = p_searchData.m_keyx - m_centerX;

            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                doneX = 1;

                int index = quadrant ^ K_RIGHT;

                nodes [index].Search (p_searchData);
            }

            temp = p_searchData.m_keyy - m_centerY;
            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_TOP;

                nodes [index].Search (p_searchData);

                if (doneX == 1)
                {
                    index = quadrant ^ (K_TOP | K_RIGHT);

                    nodes [index].Search (p_searchData);
                }
            }    
        }
    }

    private void CreateChildNodes()
    {
        m_nodes = new ComponentQuadtreeNode3<T>[4];

        m_nodes [0]                 = new ComponentQuadtreeNode3<T> (this, m_bottomLeftX, m_bottomLeftY, m_centerX, m_centerY);
        m_nodes [K_RIGHT]           = new ComponentQuadtreeNode3<T> (this, m_centerX, m_bottomLeftY, m_topRightX, m_centerY);
        m_nodes [K_TOP]             = new ComponentQuadtreeNode3<T> (this, m_bottomLeftX, m_centerY, m_centerX, m_topRightY);
        m_nodes [K_RIGHT + K_TOP]   = new ComponentQuadtreeNode3<T> (this, m_centerX, m_centerY, m_topRightX, m_topRightY);
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

    public bool IsInside(float p_x, float p_y)
    {
        return (
            (m_bottomLeftX <= p_x) && (p_x <= m_topRightX) &&
            (m_bottomLeftY <= p_y) && (p_y <= m_topRightY)
        );
    }

    public void Rebuild()
    {
        if (m_bucketCount >= 0)
        {
            for (int i = 0; i < m_bucketCount; i++)
            {
//                ComponentQuadNodeData3<T> data = m_bucket [i];

                Vector3 pos = m_bucket [i].m_transform.position;

                m_bucket [i].m_keyx = pos.x;
                m_bucket [i].m_keyy = pos.y;

                if (IsInside (pos.x, pos.y) == true)
                    continue;
                
                // m_bucket [m_bucketCount] = null;

                ComponentQuadtreeNode3<T> temp = m_parent;
                while (temp != null)
                {
                    if (temp.IsInside (pos.x, pos.y))
                    {
                        temp.Add (ref m_bucket [i]);
                        break;
                    }
                    else
                    {
                        temp = temp.m_parent;
                    }
                }

                m_bucket [i--] = m_bucket [--m_bucketCount];
            }
        }
        else
        {
            m_nodes [0].Rebuild ();
            m_nodes [1].Rebuild ();
            m_nodes [2].Rebuild ();
            m_nodes [3].Rebuild ();
        }
    }
}

public struct ComponentQuadNodeData3<T> // where T : Component
{
    public Transform m_transform;
    public float m_keyx;
    public float m_keyy;
    public T m_value;

    public ComponentQuadNodeData3 (float p_keyx, float p_keyy, T  p_value)
    {
        m_value = p_value;
        m_transform = (p_value as Component).transform;

        m_keyx = p_keyx;
        m_keyy = p_keyy;
    }
}