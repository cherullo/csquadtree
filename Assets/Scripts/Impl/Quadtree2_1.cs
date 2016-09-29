using UnityEngine;
using System.Collections;

public class Quadtree2_1<T> : QuadTree2_1Node<T>
{
    private SearchData<T> m_searchData = new SearchData<T>();

    private QuadNodeData<T> m_roller;

    public Quadtree2_1 (float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY) : base (p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY)
    {
    }
    

    public void Add(float p_keyx, float p_keyy, T p_value)
    {
        // QuadNodeData<T> data = new QuadNodeData<T> (p_keyx, p_keyy, p_value);

        if (m_roller == null)
            m_roller = new QuadNodeData<T> (p_keyx, p_keyy, p_value);
        else
        {
            m_roller.m_keyx = p_keyx;
            m_roller.m_keyy = p_keyy;
            m_roller.m_value = p_value;
        }

        m_roller = this.Add (m_roller);
    }

    public SearchData<T> ClosestTo(float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null)
    {
        m_searchData.SetData (p_keyx, p_keyy, p_filter);

        Search (m_searchData);

        m_searchData.m_currentDistance = Mathf.Sqrt (m_searchData.m_currentDistance);

        return m_searchData;
    }
}

public class QuadTree2_1Node<T>
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

    private QuadTree2_1Node<T>[] m_nodes;
    private QuadNodeData<T>[] m_bucket;
    private int m_bucketCount;

    // bool m_bucketMode = true;

    public QuadTree2_1Node (float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY)
    {
        this.m_bottomLeftX = p_bottomLeftX;
        this.m_topRightX = p_topRightX;
        m_centerX = (p_bottomLeftX + p_topRightX) * 0.5f;

        this.m_bottomLeftY = p_bottomLeftY;
        this.m_topRightY = p_topRightY;
        m_centerY = (p_bottomLeftY + p_topRightY) * 0.5f;

        m_bucket = new QuadNodeData<T>[K_BUCKET_SIZE];
        m_bucketCount = 0;
    }

    public void Clear()
    {
        // m_bucketMode = true;

        m_bucketCount = 0;
    }

    public QuadNodeData<T> Add(QuadNodeData<T> p_data)
    {
        if (m_bucketCount <= K_BUCKET_SIZE) // Bucket mode
        {
            if (m_bucketCount == K_BUCKET_SIZE) // Spill
            {
                // m_bucketMode = false;
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
                    m_bucket [i] = Add (m_bucket [i]);
                }

                return Add (p_data);
            }
            else
            {
                QuadNodeData<T> temp = m_bucket [m_bucketCount];
                m_bucket [m_bucketCount++] = p_data;
                return temp;
            }
        }
        else // Tree mode
        {
            int quadrant = GetQuadrant (p_data.m_keyx, p_data.m_keyy);

            return m_nodes [quadrant].Add (p_data);
        }
    }

    public void Search(SearchData<T> p_searchData)
    {
        if (m_bucketCount <= K_BUCKET_SIZE) // Bucket mode
        {
            /*/
            for (int i = 0; i < m_bucketCount; i++)
                p_searchData.Feed (m_bucket [i]);
            /*/
            for (int i = m_bucketCount - 1; i >= 0; i--)
                p_searchData.Feed (m_bucket [i]);
            /**/
        }
        else // Tree mode
        {
            /*/

            QuadTree21Node<T>[] nodes = m_nodes;
            int quadrant = GetQuadrant (p_searchData.m_keyx, p_searchData.m_keyy);

            nodes [quadrant].Search (p_searchData);

            float temp = p_searchData.m_keyx - m_centerX;

            int both = 0;

            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                nodes [quadrant ^ K_RIGHT].Search (p_searchData);

                both = 1;
            }

            temp = p_searchData.m_keyy - m_centerY;
            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                nodes [quadrant ^ K_TOP].Search (p_searchData);

                both++;
            }    

            if (both == 2)
            {
                int index = quadrant ^ (K_TOP | K_RIGHT);

                nodes [index].Search (p_searchData);
            }
            /*/
            // QuadTree21Node<T>[] nodes = m_nodes;
            int quadrant = GetQuadrant (p_searchData.m_keyx, p_searchData.m_keyy);

            m_nodes [quadrant].Search (p_searchData);

            int doneX = 0;

            float temp = p_searchData.m_keyx - m_centerX;

            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_RIGHT;

                m_nodes [index].Search (p_searchData);

                doneX = 1;
            }

            temp = p_searchData.m_keyy - m_centerY;
            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_TOP;

                m_nodes [index].Search (p_searchData);

                if (doneX == 1)
                {
                    index = quadrant ^ (K_TOP | K_RIGHT);

                    m_nodes [index].Search (p_searchData);
                }
            }  
            /**/
        }
    }

    private void CreateChildNodes()
    {
        m_nodes = new QuadTree2_1Node<T>[4];

        m_nodes [0]                 = new QuadTree2_1Node<T> (m_bottomLeftX, m_bottomLeftY, m_centerX, m_centerY);
        m_nodes [K_RIGHT]           = new QuadTree2_1Node<T> (m_centerX, m_bottomLeftY, m_topRightX, m_centerY);
        m_nodes [K_TOP]             = new QuadTree2_1Node<T> (m_bottomLeftX, m_centerY, m_centerX, m_topRightY);
        m_nodes [K_RIGHT + K_TOP]   = new QuadTree2_1Node<T> (m_centerX, m_centerY, m_topRightX, m_topRightY);
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
