using UnityEngine;

public class RotatingRandomDeltaPointGenerator : RandomDeltaPointGenerator 
{
    public float m_rotationSpeed = 5.0f; // degrees per frame
    public float m_radius = 20.0f;

    private Vector2 m_rotationPivot;


	public override float GetEffectiveSideLength (float sideLength)
    {
        m_rotationPivot = Vector2.one * (m_radius + 0.5f * sideLength);

        return sideLength + 2.0f * m_radius;
    }

    public override Vector2[] GetPositions(int count, float sideLength)
    {
        Vector2[] positions = base.GetPositions (count, sideLength);

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
