using UnityEngine;
using System.Diagnostics;
using System.Collections;

public interface IPointTest
{
    void Initialize(float sideLength);
    string GetName();

    RunResult RunTest (Vector2[] p_points, Component[] p_values, Vector2[] p_searches);
}

public abstract class PointTest : MonoBehaviour
{
    public float m_sideLengthMultiplier = 1.0f;

    public Stopwatch Watch = new Stopwatch();

    protected float m_sideLength = 1.0f;

    protected Component[] m_results;

    public void SetSideLength(float p_sideLength)
    {
        m_sideLength = p_sideLength * m_sideLengthMultiplier;
    }

    public virtual string GetName()
    {
        return GetType ().Name + "(" + m_sideLength + ")";
    }

    public Component[] GetResults()
    {
        return m_results;
    }

    public abstract Component[] RunTest (Vector2[] p_points, Component[] p_values, Vector2[] p_searches);

    // This only exists to force the 'enabled' chackbox in the inspector
    void Start()
    {
    }
}