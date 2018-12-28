using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultsAggregator
{
    private double _fullTime = 0.0f;

    private int _totalResults = 0;

    public double Average
    {
        get
        {
            return _fullTime / _totalResults;
        }
    }

    public void FeedResult(RunResult result)
    {
        _fullTime += result.Time;
        _totalResults++;
    }    
}
