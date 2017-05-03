using UnityEngine;
using System.Collections;

namespace Impl.CQt1
{

public class ComponentQuadtree<T> : ComponentQuadtreeNode<T>, IQuadtree<T> where T : Component
{
    private SearchData<T> m_searchData = new SearchData<T> ();

    public ComponentQuadtree (float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY) : base (null, p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY)
    {
        m_root = this;
    }


    public void Add (float p_keyx, float p_keyy, T p_value)
    {
        ComponentQuadNodeData<T> data = new ComponentQuadNodeData<T> (p_keyx, p_keyy, p_value);

        this.Add (data);
    }

    public ISearchResult<T> ClosestTo (float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null)
    {
        m_searchData.SetData (p_keyx, p_keyy, p_filter);

        Search (m_searchData);

        m_searchData.m_currentDistance = Mathf.Sqrt (m_searchData.m_currentDistance);

        return m_searchData;
    }
}

public class ComponentQuadtreeNode<T> where T : Component
{
    private const int K_BUCKET_SIZE = 6;
    private const int K_RIGHT = 1;
    private const int K_TOP = 2;

    protected ComponentQuadtreeNode<T> m_root;

    private float m_bottomLeftX;
    private float m_bottomLeftY;
    private float m_topRightX;
    private float m_topRightY;

    private float m_centerX;
    private float m_centerY;

    private ComponentQuadtreeNode<T>[] m_nodes;
    private ComponentQuadNodeData<T>[] m_bucket;
    private int m_bucketCount;

    bool m_bucketMode = true;

    public ComponentQuadtreeNode (ComponentQuadtreeNode<T> p_root, float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY)
    {
        this.m_root = p_root;
        this.m_bottomLeftX = p_bottomLeftX;
        this.m_bottomLeftY = p_bottomLeftY;
        this.m_topRightX = p_topRightX;
        this.m_topRightY = p_topRightY;

        m_centerX = (m_bottomLeftX + m_topRightX) * 0.5f;
        m_centerY = (m_bottomLeftY + m_topRightY) * 0.5f;

        m_bucket = new ComponentQuadNodeData<T>[K_BUCKET_SIZE];
        m_bucketCount = 0;
    }

    public void Clear ()
    {
        m_bucketMode = true;

        m_bucketCount = 0;
    }

    public void Add (ComponentQuadNodeData<T> p_data)
    {
        if (m_bucketMode) // Bucket mode
        {
            if (m_bucketCount == K_BUCKET_SIZE) // Spill
            {
                m_bucketMode = false;

                if (m_nodes == null)
                    CreateChildNodes ();
                else
                {
                    for (int i = 0; i < m_nodes.Length; i++)
                    {
                        m_nodes [i].Clear ();
                    }
                }

                ComponentQuadNodeData<T>[] temp = m_bucket;

                for (int i = 0; i < K_BUCKET_SIZE; i++)
                {
                    Add (temp [i]);
                }

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

            m_nodes [quadrant].Add (p_data);
        }
    }

    public void Search (SearchData<T> p_searchData)
    {
        if (m_bucketMode) // Bucket mode
        {
            for (int i = 0; i < m_bucketCount; i++)
                p_searchData.Feed (m_bucket [i]);
        }
        else // Tree mode
        {
            ComponentQuadtreeNode<T>[] nodes = m_nodes;
            int quadrant = GetQuadrant (p_searchData.m_keyx, p_searchData.m_keyy);

            nodes [quadrant].Search (p_searchData);

            float temp = p_searchData.m_keyx - m_centerX;

            int both = 0;

            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_RIGHT;

                nodes [index].Search (p_searchData);

                both++;
            }

            temp = p_searchData.m_keyy - m_centerY;
            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_TOP;

                nodes [index].Search (p_searchData);

                both++;
            }    

            if (both == 2)
            {
                int index = quadrant ^ (K_TOP | K_RIGHT);

                nodes [index].Search (p_searchData);
            }
        }
    }

    private void CreateChildNodes ()
    {
        m_nodes = new ComponentQuadtreeNode<T>[4];

        m_nodes [0] = new ComponentQuadtreeNode<T> (m_root, m_bottomLeftX, m_bottomLeftY, m_centerX, m_centerY);
        m_nodes [K_RIGHT] = new ComponentQuadtreeNode<T> (m_root, m_centerX, m_bottomLeftY, m_topRightX, m_centerY);
        m_nodes [K_TOP] = new ComponentQuadtreeNode<T> (m_root, m_bottomLeftX, m_centerY, m_centerX, m_topRightY);
        m_nodes [K_RIGHT + K_TOP] = new ComponentQuadtreeNode<T> (m_root, m_centerX, m_centerY, m_topRightX, m_topRightY);
    }

    private int GetQuadrant (float p_keyx, float p_keyy)
    {
        int ret = 0;

        if (p_keyx > m_centerX)
            ret += K_RIGHT;

        if (p_keyy > m_centerY)
            ret += K_TOP;

        return ret;
    }

    public bool IsInside (float p_x, float p_y)
    {
        return (
            (m_bottomLeftX <= p_x) && (p_x <= m_topRightX) &&
            (m_bottomLeftY <= p_y) && (p_y <= m_topRightY)
        );
    }

    public void Rebuild ()
    {
        if (m_bucketMode == true)
        {
            for (int i = 0; i < m_bucketCount; i++)
            {
                ComponentQuadNodeData<T> data = m_bucket [i];

                Vector3 pos = data.m_transform.position;

                data.m_keyx = pos.x;
                data.m_keyy = pos.y;

                if (IsInside (pos.x, pos.y) == true)
                    continue;

                m_bucket [i--] = m_bucket [--m_bucketCount];
                // m_bucket [m_bucketCount] = null;

                m_root.Add (data);
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

public class ComponentQuadNodeData<T> : QuadNodeData<T> where T : Component
{
    public Transform m_transform;

    public ComponentQuadNodeData (float p_keyx, float p_keyy, T p_value) : base (p_keyx, p_keyy, p_value)
    {
        m_transform = p_value.transform;
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