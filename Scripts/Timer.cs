using UnityEngine;

public class Timer
{
    public bool IsActive { get { return Time.time < timeEnd; } }
    public bool IsFinished { get { return Time.time >= timeEnd; } }
    public float Progress { get { return CalcProgress(); } }
    private float timeEnd = 0f, timeStart = 0f;


    public void Activate(float timeAmount)
    {
        timeStart = Time.time;
        timeEnd = timeStart + timeAmount;
    }

    public void Activate(float timeAmount, float progress = 0f)
    {
        timeStart = Time.time - timeAmount * progress;
        timeEnd = timeStart + timeAmount;
    }

    private float CalcProgress()
    {
        if (IsFinished)
            return 1f;
        else
            return (Time.time - timeStart) / (timeEnd - timeStart);
    }
}
