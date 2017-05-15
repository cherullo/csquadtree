using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingRandomDeltaPointGenerator : RandomDeltaPointGenerator 
{
    public float m_rotationSpeed = 5.0f; // degrees per frame
    public float m_radius = 20.0f;

    private Vector2 m_rotationPivot;

    Vector2[] m_boxedPositions;



	protected override float GetEffectiveSideLength (float p_sideLength)
    {
        m_rotationPivot = Vector2.one * (m_radius + 0.5f * p_sideLength);

        return m_sideLength + 2.0f * m_radius;
    }

    protected override Vector2[] RandomizeTransformPositions (Transform[] p_transforms, float p_sideLength)
    {
        if (m_boxedPositions == null)
        {
            m_boxedPositions = base.BuildRandomPositions (p_transforms.Length, p_sideLength);
        }

        Vector2 rotationCenter = GetRotationCenter (m_rotationPivot, m_radius);
        Vector2 delta = rotationCenter - (0.5f * m_sideLength) * Vector2.one;

        Vector2[] ret = new Vector2[p_transforms.Length];

        for (int i = 0; i < p_transforms.Length; i++)
        {
            m_boxedPositions[i].x = Mathf.Clamp (m_boxedPositions[i].x + Random.Range (-m_maxDelta, m_maxDelta), 0f, p_sideLength);
            m_boxedPositions[i].y = Mathf.Clamp (m_boxedPositions[i].y + Random.Range (-m_maxDelta, m_maxDelta), 0f, p_sideLength);

            ret[i] = m_boxedPositions[i] + delta;

            p_transforms [i].position = ret [i];
        }

        return ret;
    }

    protected override Vector2[] BuildRandomPositions (int p_numPoints, float p_sideLength)
    {
        Vector2[] positions = base.BuildRandomPositions (p_numPoints, p_sideLength);

        return Add (positions, GetRotationCenter (m_rotationPivot, m_radius));
    }

    protected Vector2 GetRotationCenter(Vector2 pivot, float radius)
    {
        float angle = Time.frameCount * m_rotationSpeed;
        angle *= Mathf.Deg2Rad;

        float cos = Mathf.Cos (angle);
        float sin = Mathf.Sin (angle);

        Vector2 ret = pivot + new Vector2 (cos, sin) * radius;

        return ret;
    }

    protected Vector2[] Add (Vector2[] positions, Vector2 delta)
    {
        Vector2[] ret = new Vector2[ positions.Length ];

        for (int i = 0; i < ret.Length; i++)
        {
            ret [i] = positions [i] + delta;
        }

        return ret;
    }
}
