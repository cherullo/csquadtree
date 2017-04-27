using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRebuildableQuadtree<T> : IQuadtree<T> where T : Component
{
    void Rebuild();
}
