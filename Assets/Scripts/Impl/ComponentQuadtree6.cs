using System;
using UnityEngine;
using System.Collections;

namespace Impl.CQt6
{
    public class ComponentQuadtree6<T> : IQuadtree<T> where T : Component
    {
        private const int K_RIGHT = 1;
        private const int K_TOP = 2;
        private const int K_BUCKET_SIZE = 6;
        // Inclusive.
        // Root node is depth 1
        private int _maxDepth = 8;
        private SearchData<T> m_searchData = new SearchData<T>();
        // public ComponentQuadtreeNode<T> m_firstDecisionPoint;
        private ComponentQuadtreeNode<T>[] _allNodes;

        // private float m_bottomLeftX;
        // private float m_bottomLeftY;
        // private float m_topRightX;
        // private float m_topRightY;

        // private float m_centerX;
        // private float m_centerY;

        public ComponentQuadtree6(float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY) 
        {
            // m_bottomLeftX = p_bottomLeftX;
            // m_bottomLeftY = p_bottomLeftY;
            // m_topRightX = p_topRightX;
            // m_topRightY = p_topRightY;

            // m_centerX = (m_bottomLeftX + m_topRightX) * 0.5f;
            // m_centerY = (m_bottomLeftY + m_topRightY) * 0.5f;

            int allNodeCount = _CalculateAllNodeCount(_maxDepth);

            _allNodes = new ComponentQuadtreeNode<T>[allNodeCount];

            _InitializeNode(0, p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY);
        }

        private void _InitializeNode(int nodeIndex, float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY)
        {
            _allNodes[nodeIndex].Init(p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY);

            int bottomLeftChild = (nodeIndex * 4) + 1;

            if (bottomLeftChild >= _allNodes.Length)
                return;

            float centerX = (p_bottomLeftX + p_topRightX) * 0.5f;
            float centerY = (p_bottomLeftY + p_topRightY) * 0.5f;

            _InitializeNode(bottomLeftChild, p_bottomLeftX, p_bottomLeftY, centerX, centerY);
            _InitializeNode(bottomLeftChild + K_RIGHT, centerX, p_bottomLeftY, p_topRightX, centerY);
            _InitializeNode(bottomLeftChild + K_TOP, p_bottomLeftX, centerY, p_bottomLeftX, p_topRightY);
            _InitializeNode(bottomLeftChild + K_RIGHT + K_TOP, centerX, centerY, p_topRightX, p_topRightY);
        }

        private int _CalculateAllNodeCount(int maxDepth)
        {
            int totalNodes = 0;
            int nodesThisLevel = 1;

            for (int i = 1; i <= maxDepth; i++)
            {
                totalNodes += nodesThisLevel;
                nodesThisLevel *= 4;
            }

            return totalNodes;
        }

        public void Clear()
        {
            _allNodes[0].Clear();
        }

        public void Add(float p_keyx, float p_keyy, T p_value)
        {
            ComponentQuadNodeData<T> data = new ComponentQuadNodeData<T>(p_keyx, p_keyy, p_value);

            Add(0, ref data);
        }

        public void Add(int node, ref ComponentQuadNodeData<T> p_data)
        {
            int bottomLeftChild = (4 * node) + 1;

            if (_allNodes[node].m_bucketCount >= 0) // Bucket mode
            {
                if ((_allNodes[node].m_bucketCount == K_BUCKET_SIZE) && (bottomLeftChild < _allNodes.Length)) // Spill
                {
                    _allNodes[node].m_bucketCount = -1;
                    
                    // Clear child nodes, if we are not at maxDepth
                    _allNodes[bottomLeftChild].Clear();
                    _allNodes[bottomLeftChild + K_RIGHT].Clear();
                    _allNodes[bottomLeftChild + K_TOP].Clear();
                    _allNodes[bottomLeftChild + K_RIGHT + K_TOP].Clear();
                    
                    ComponentQuadNodeData<T>[] temp = _allNodes[node].m_bucket;

                    _allNodes[node].m_childCount -= K_BUCKET_SIZE;

                    for (int i = 0; i < K_BUCKET_SIZE; i++)
                    {
                        Add(node, ref temp[i]);
                    }

                    Add(node, ref p_data);
                }
                else
                {
                    _allNodes[node].m_childCount++;
                    _allNodes[node].m_bucket[_allNodes[node].m_bucketCount++] = p_data;
                }
            }
            else // Tree mode
            {
                _allNodes[node].m_childCount++;

                int quadrant = _allNodes[node].GetQuadrant(p_data.m_keyx, p_data.m_keyy);

                Add(bottomLeftChild + quadrant, ref p_data);
            }
        }

        // public new void Rebuild()
        // {
        //     _allNodes[0].Rebuild();

        //     m_firstDecisionPoint = FindFirstDecisionPoint();
        // }

        public ISearchResult<T> ClosestTo(float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null)
        {
            m_searchData.SetData(p_keyx, p_keyy, p_filter);

            // if (m_firstDecisionPoint == null)
            //     m_firstDecisionPoint = this;

            //m_firstDecisionPoint.Search(m_searchData);
            Search(0, m_searchData);

            m_searchData.m_currentDistance = Mathf.Sqrt(m_searchData.m_currentDistance);

            return m_searchData;
        }

        public void Search(int node, SearchData<T> p_searchData)
        {
            if (_allNodes[node].m_bucketCount >= 0) // Bucket mode
            {
                for (int i = 0; i < _allNodes[node].m_bucketCount; i++)
                    p_searchData.Feed(ref _allNodes[node].m_bucket[i]);
            }
            else // Tree mode
            {
                int bottomLeftChild = (4 * node) + 1;
                int quadrant = _allNodes[node].GetQuadrant(p_searchData.m_keyx, p_searchData.m_keyy);

                Search(bottomLeftChild + quadrant, p_searchData);

                int doneX = 0;

                float temp = p_searchData.m_keyx - _allNodes[node].m_centerX;

                if ((temp * temp) < p_searchData.m_currentDistance)
                {
                    doneX = 1;

                    int index = quadrant ^ K_RIGHT;

                    Search(bottomLeftChild + index, p_searchData);
                }

                temp = p_searchData.m_keyy - _allNodes[node].m_centerY;
                if ((temp * temp) < p_searchData.m_currentDistance)
                {
                    int index = quadrant ^ K_TOP;

                    Search(bottomLeftChild + index, p_searchData);

                    if (doneX == 1)
                    {
                        index = quadrant ^ (K_TOP | K_RIGHT);

                        Search(bottomLeftChild + index, p_searchData);
                    }
                }
            }
        }

        // protected ComponentQuadtreeNode<T> FindFirstDecisionPoint()
        // {
        //     ComponentQuadtreeNode<T> ret = this;
        //     int total = m_childCount;

        //     while (true)
        //     {
        //         int i = 0;
        //         for (; i < 4; i++)
        //         {
        //             ComponentQuadtreeNode<T> node = ret.m_nodes[i];

        //             if (node.m_childCount == total)
        //             {
        //                 ret = node;
        //                 break;
        //             }

        //             if (node.m_childCount > 0)
        //                 return ret;
        //         }

        //         if (i == 4)
        //             break;
        //     }

        //     return ret;
        // }
    }

    public struct ComponentQuadtreeNode<T> where T : Component
    {
        private const int K_RIGHT = 1;
        private const int K_TOP = 2;
        private const int K_BUCKET_SIZE = 6;

        private float m_bottomLeftX;
        private float m_bottomLeftY;
        private float m_topRightX;
        private float m_topRightY;

        public float m_centerX;
        public float m_centerY;

        public int m_bucketCount;
        public ComponentQuadNodeData<T>[] m_bucket;
        public int m_childCount;

        public void Init(float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY)
        {
            this.m_bottomLeftX = p_bottomLeftX;
            this.m_bottomLeftY = p_bottomLeftY;
            this.m_topRightX = p_topRightX;
            this.m_topRightY = p_topRightY;

            m_centerX = (m_bottomLeftX + m_topRightX) * 0.5f;
            m_centerY = (m_bottomLeftY + m_topRightY) * 0.5f;

            m_bucket = new ComponentQuadNodeData<T>[K_BUCKET_SIZE];
            m_bucketCount = 0;
            m_childCount = 0;
        }

        public void Clear()
        {
            m_bucketCount = 0;

            m_childCount = 0;
        }

        public int GetQuadrant(float p_keyx, float p_keyy)
        {
            int ret = 0;

            if (p_keyx > m_centerX)
                ret = K_RIGHT;

            if (p_keyy > m_centerY)
                ret |= K_TOP;

            return ret;
        }

        // public bool IsInside(float p_x, float p_y)
        // {
        //     return (
        //         (m_bottomLeftX <= p_x) && (p_x <= m_topRightX) &&
        //         (m_bottomLeftY <= p_y) && (p_y <= m_topRightY)
        //     );
        // }

        // protected void Rebuild()
        // {
        //     if (m_childCount == 0)
        //     {
        //         return;
        //     }
        
        //     if (m_bucketCount >= 0)
        //     {
        //         for (int i = 0; i < m_bucketCount; i++)
        //         {
        //             Vector3 pos = (m_bucket[i].m_value as Component).transform.position;

        //             m_bucket[i].m_keyx = pos.x;
        //             m_bucket[i].m_keyy = pos.y;

        //             if (IsInside(pos.x, pos.y) == true)
        //                 continue;

        //             m_childCount--;

        //             ComponentQuadtreeNode<T> temp = m_parent;
        //             while (temp != null)
        //             {
        //                 temp.m_childCount--;
        //                 if (temp.IsInside(pos.x, pos.y))
        //                 {
        //                     temp.Add(ref m_bucket[i]);
        //                     break;
        //                 }
        //                 else
        //                 {
        //                     temp = temp.m_parent;
        //                 }
        //             }

        //             m_bucket[i--] = m_bucket[--m_bucketCount];
        //         }
        //     }
        //     else
        //     {
        //         m_nodes[0].Rebuild();
        //         m_nodes[1].Rebuild();
        //         m_nodes[2].Rebuild();
        //         m_nodes[3].Rebuild();

        //         if (m_childCount == 0)
        //             Clear();
        //     }
        // }
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
        // public Transform m_transform;
        public float m_keyx;
        public float m_keyy;
        public T m_value;

        public ComponentQuadNodeData(float p_keyx, float p_keyy, T p_value)
        {
            m_value = p_value;
            // m_transform = (p_value as Component).transform;

            m_keyx = p_keyx;
            m_keyy = p_keyy;
        }
    }

}