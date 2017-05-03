using UnityEngine;
using System.Collections;

public interface IQuadtree<T>
{
    void Add (float p_keyx, float p_keyy, T p_value);

    ISearchResult<T> ClosestTo (float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null);

    void Clear();
}
