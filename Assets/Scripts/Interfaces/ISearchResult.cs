using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISearchResult<T>
{
    T GetResult();
    float GetDistance();
}
