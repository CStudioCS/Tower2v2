using UnityEngine;

public static class Easing
{
    public static float QuadOut(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }
}
