using System;
using UnityEngine;
using System.Collections;

namespace Impl.CQt5
{

    public class ComponentQuadtree5<T> : ComponentQuadtreeNode<T>, IRebuildableQuadtree<T> where T : Component
    {
        private SearchData<T> m_searchData = new SearchData<T>();
        public ComponentQuadtreeNode<T> m_firstDecisionPoint;

        public ComponentQuadtree5(float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY) : base(null, p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY)
        {
            m_parent = null;
        }

        public void Add(float p_keyx, float p_keyy, T p_value)
        {
            ComponentQuadNodeData<T> data = new ComponentQuadNodeData<T>(p_keyx, p_keyy, p_value);

            this.Add(ref data);
        }

        public new void Rebuild()
        {
            base.Rebuild();

            m_firstDecisionPoint = FindFirstDecisionPoint();
        }

        public ISearchResult<T> ClosestTo(float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null)
        {
            m_searchData.SetData(p_keyx, p_keyy, p_filter);

            if (m_firstDecisionPoint == null)
                m_firstDecisionPoint = this;

            m_firstDecisionPoint.Search(m_searchData);

            m_searchData.m_currentDistance = Mathf.Sqrt(m_searchData.m_currentDistance);

            return m_searchData;
        }

        protected ComponentQuadtreeNode<T> FindFirstDecisionPoint()
        {
            ComponentQuadtreeNode<T> ret = this;
            int total = m_childCount;

            while (true)
            {
                int i = 0;
                for (; i < 4; i++)
                {
                    ComponentQuadtreeNode<T> node = ret.m_nodes[i];

                    if (node.m_childCount == total)
                    {
                        ret = node;
                        break;
                    }

                    if (node.m_childCount > 0)
                        return ret;
                }

                if (i == 4)
                    break;
            }

            return ret;
        }
    }

    public class ComponentQuadtreeNode<T> where T : Component
    {
        private const int K_BUCKET_SIZE = 6;
        private const int K_RIGHT = 1;
        private const int K_TOP = 2;

        protected ComponentQuadtreeNode<T> m_parent;

        private float m_bottomLeftX;
        private float m_bottomLeftY;
        private float m_topRightX;
        private float m_topRightY;

        private float m_centerX;
        private float m_centerY;

        protected int m_bucketCount;
        private ComponentQuadNodeData<T>[] m_bucket;
        public ComponentQuadtreeNode<T>[] m_nodes;
        public int m_childCount = 0;

        // bool m_bucketMode = true;

        public ComponentQuadtreeNode(ComponentQuadtreeNode<T> p_parent, float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY)
        {
            this.m_parent = p_parent;
            this.m_bottomLeftX = p_bottomLeftX;
            this.m_bottomLeftY = p_bottomLeftY;
            this.m_topRightX = p_topRightX;
            this.m_topRightY = p_topRightY;

            m_centerX = (m_bottomLeftX + m_topRightX) * 0.5f;
            m_centerY = (m_bottomLeftY + m_topRightY) * 0.5f;

            m_bucket = new ComponentQuadNodeData<T>[K_BUCKET_SIZE];
            m_bucketCount = 0;
        }

        public void Clear()
        {
            // m_bucketMode = true;

            m_bucketCount = 0;
            m_childCount = 0;
        }

        public void Add(ref ComponentQuadNodeData<T> p_data)
        {
            if (m_bucketCount >= 0) // Bucket mode
            {
                if (m_bucketCount == K_BUCKET_SIZE) // Spill
                {
                    m_bucketCount = -1;

                    if (m_nodes == null)
                    {
                        m_nodes = _CreateChildNodes();
                    }
                    else
                    {
                        m_nodes[0].Clear();
                        m_nodes[1].Clear();
                        m_nodes[2].Clear();
                        m_nodes[3].Clear();
                    }

                    ComponentQuadNodeData<T>[] temp = m_bucket;

                    m_childCount -= K_BUCKET_SIZE;

                    for (int i = 0; i < K_BUCKET_SIZE; i++)
                    {
                        Add(ref temp[i]);
                    }

                    Add(ref p_data);
                }
                else
                {
                    m_childCount++;
                    m_bucket[m_bucketCount++] = p_data;
                }
            }
            else // Tree mode
            {
                m_childCount++;

                int quadrant = GetQuadrant(p_data.m_keyx, p_data.m_keyy);

                m_nodes[quadrant].Add(ref p_data);
            }
        }

        public void Search(SearchData<T> p_searchData)
        {
            //        if (m_childCount == 0)
            //            return;

            if (m_bucketCount >= 0) // Bucket mode
            {
                for (int i = 0; i < m_bucketCount; i++)
                    p_searchData.Feed(ref m_bucket[i]);
            }
            else // Tree mode
            {
                ComponentQuadtreeNode<T>[] nodes = m_nodes;
                int quadrant = GetQuadrant(p_searchData.m_keyx, p_searchData.m_keyy);

                nodes[quadrant].Search(p_searchData);

                int doneX = 0;

                float temp = p_searchData.m_keyx - m_centerX;

                if ((temp * temp) < p_searchData.m_currentDistance)
                {
                    doneX = 1;

                    int index = quadrant ^ K_RIGHT;

                    nodes[index].Search(p_searchData);
                }

                temp = p_searchData.m_keyy - m_centerY;
                if ((temp * temp) < p_searchData.m_currentDistance)
                {
                    int index = quadrant ^ K_TOP;

                    nodes[index].Search(p_searchData);

                    if (doneX == 1)
                    {
                        index = quadrant ^ (K_TOP | K_RIGHT);

                        nodes[index].Search(p_searchData);
                    }
                }
            }
        }

        private ComponentQuadtreeNode<T>[] _CreateChildNodes()
        {
            ComponentQuadtreeNode<T>[] ret = new ComponentQuadtreeNode<T>[4];

            ret[0] = new ComponentQuadtreeNode<T>(this, m_bottomLeftX, m_bottomLeftY, m_centerX, m_centerY);
            ret[K_RIGHT] = new ComponentQuadtreeNode<T>(this, m_centerX, m_bottomLeftY, m_topRightX, m_centerY);
            ret[K_TOP] = new ComponentQuadtreeNode<T>(this, m_bottomLeftX, m_centerY, m_centerX, m_topRightY);
            ret[K_RIGHT + K_TOP] = new ComponentQuadtreeNode<T>(this, m_centerX, m_centerY, m_topRightX, m_topRightY);

            return ret;
        }

        private int GetQuadrant(float p_keyx, float p_keyy)
        {
            /**/
            return (Math.Sign(p_keyx - m_centerX) & 0x1) | ((Math.Sign(p_keyy - m_centerY) & 0x1) << 1);
            /*/
            int ret = 0;

            if (p_keyx > m_centerX)
                ret = K_RIGHT;

            if (p_keyy > m_centerY)
                ret |= K_TOP;

            return ret;
            /**/
        }

        public bool IsInside(float p_x, float p_y)
        {
            return (
                (m_bottomLeftX <= p_x) && (p_x <= m_topRightX) &&
                (m_bottomLeftY <= p_y) && (p_y <= m_topRightY)
            );
        }

        protected void Rebuild()
        {
            if (m_childCount == 0)
            {
                return;
            }

            if (m_bucketCount == -1) // reset tree mode
            {
                m_bucketCount = 0;
                //                Debug.Log ("Back to bucket mode");

                //if (m_nodes != null)
                {
                    m_nodes[0].Clear();
                    m_nodes[1].Clear();
                    m_nodes[2].Clear();
                    m_nodes[3].Clear();
                }
            }
        
            if (m_bucketCount >= 0)
            {
                for (int i = 0; i < m_bucketCount; i++)
                {
                    Vector3 pos = m_bucket[i].m_transform.position;

                    m_bucket[i].m_keyx = pos.x;
                    m_bucket[i].m_keyy = pos.y;

                    if (IsInside(pos.x, pos.y) == true)
                        continue;

                    m_childCount--;

                    ComponentQuadtreeNode<T> temp = m_parent;
                    while (temp != null)
                    {
                        temp.m_childCount--;
                        if (temp.IsInside(pos.x, pos.y))
                        {
                            temp.Add(ref m_bucket[i]);
                            break;
                        }
                        else
                        {
                            temp = temp.m_parent;
                        }
                    }

                    m_bucket[i--] = m_bucket[--m_bucketCount];
                }
            }
            else
            {
                m_nodes[0].Rebuild();
                m_nodes[1].Rebuild();
                m_nodes[2].Rebuild();
                m_nodes[3].Rebuild();
            }
        }
    }

    public class QuadNodeData<T>
    {
        public float m_keyx;
        public float m_keyy;
        public T m_value;

        public QuadNodeData(float p_keyx, float p_keyy, T p_value)
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

        public SearchData()
        {
        }

        public T GetResult()
        {
            return m_currentClosest;
        }

        public float GetDistance()
        {
            return m_currentDistance;
        }

        public void SetData(float m_keyx, float m_keyy, DQuadtreeFilter<T> p_filter = null)
        {
            this.m_keyx = m_keyx;
            this.m_keyy = m_keyy;
            this.m_filter = p_filter;

            this.m_currentClosest = default(T);
            this.m_currentDistance = Mathf.Infinity;
        }

        public void Feed(QuadNodeData<T> p_nodeData)
        {
            float distance = DistanceTo(p_nodeData);
            if (distance < m_currentDistance)
            {
                if ((m_filter != null) && (m_filter(p_nodeData.m_value) == false))
                    return;

                m_currentDistance = distance;
                m_currentClosest = p_nodeData.m_value;
            }
        }


        public void Feed(ref ComponentQuadNodeData<T> p_nodeData)
        {
            float distance = DistanceTo(ref p_nodeData);
            if (distance < m_currentDistance)
            {
                if ((m_filter != null) && (m_filter(p_nodeData.m_value) == false))
                    return;

                m_currentDistance = distance;
                m_currentClosest = p_nodeData.m_value;
            }
        }

        private float DistanceTo(QuadNodeData<T> p_nodeData)
        {
            float distX = (m_keyx - p_nodeData.m_keyx);
            float distY = (m_keyy - p_nodeData.m_keyy);
            return distX * distX + distY * distY;
        }


        private float DistanceTo(ref ComponentQuadNodeData<T> p_nodeData)
        {
            float distX = (m_keyx - p_nodeData.m_keyx);
            float distY = (m_keyy - p_nodeData.m_keyy);
            return distX * distX + distY * distY;
        }
    }


    public struct ComponentQuadNodeData<T> // where T : Component
    {
        public Transform m_transform;
        public float m_keyx;
        public float m_keyy;
        public T m_value;

        public ComponentQuadNodeData(float p_keyx, float p_keyy, T p_value)
        {
            m_value = p_value;
            m_transform = (p_value as Component).transform;

            m_keyx = p_keyx;
            m_keyy = p_keyy;
        }
    }

}