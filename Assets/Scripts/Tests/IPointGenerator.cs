using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointGenerator 
{
    Vector2[] GetPositions(int count, float sideLength);
    float GetEffectiveSideLength(float configuredSideLength);
}
