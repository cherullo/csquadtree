using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class RandomPointGenerator : MonoBehaviour, IPointGenerator 
{
    private Vector2[] _positions;
    public virtual float GetEffectiveSideLength(float configuredSideLength)
    {
        return configuredSideLength;
    }

    public virtual Vector2[] GetPositions(int count, float sideLength)
    {
        _Allocate(ref _positions, count);

        for (int i = 0; i < count; i++)
        {
            _positions [i].x = Random.Range (0.0f, sideLength);
            _positions [i].y = Random.Range (0.0f, sideLength);
        }

        return _positions;
    }

    private void _Allocate(ref Vector2[] positions, int count)
    {
        if ((positions == null) || (positions.Length != count))
        {
            positions = new Vector2[count];
        }
    }
}