using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class RandomDeltaPointGenerator : MonoBehaviour, IPointGenerator {
    public float m_maxDelta = 0.1f;

    private Vector2[] _positions;

    public virtual float GetEffectiveSideLength(float configuredSideLength)
    {
        return configuredSideLength;
    }

    public virtual Vector2[] GetPositions(int count, float sideLength)
    {
        _AllocateAndInitialize(ref _positions, count, sideLength);

        for (int i = 0; i < count; i++)
        {
            _positions [i].x = Mathf.Clamp (_positions[i].x + Random.Range (-m_maxDelta, m_maxDelta), 0f, sideLength);
            _positions [i].y = Mathf.Clamp (_positions[i].y + Random.Range (-m_maxDelta, m_maxDelta), 0f, sideLength);
        }

        return _positions;
    }

    private void _AllocateAndInitialize(ref Vector2[] positions, int count, float sideLength)
    {
        if ((positions == null) || (positions.Length != count))
        {
            positions = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                positions [i].x = Random.Range (0.0f, sideLength);
                positions [i].y = Random.Range (0.0f, sideLength);
            }   
        }
    }
}