using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunResult 
{
    public Component[] Results { get; private set; }

    public double Time { get; private set; }

    public RunResult(Component[] results, double time)
    {
        Results = results;
        Time = time;
    }
}
