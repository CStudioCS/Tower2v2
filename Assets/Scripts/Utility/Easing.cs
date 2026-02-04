using UnityEngine;

public static class Easing
{
    public static float QuadOut(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }
    public static float RootOut(float t)
    {
        return 1 - Mathf.Pow((1 - t), 0.5f);
    }
}
