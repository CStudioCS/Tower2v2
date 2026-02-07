using UnityEngine;
using System.Collections;
using System;
public static class Zoom
{
    private static void ZoomIn(Camera cam, float initialCamSize, Vector3 initialCamPosition, float time, float posyFinal)
    {
        cam.orthographicSize = Mathf.Lerp(initialCamSize, 0.5f, Easing.QuadOut(time));
        
        cam.transform.position = Vector3.Lerp(initialCamPosition, new Vector3(0, posyFinal, -10f), Easing.QuadOut(time));

    }   

    public static IEnumerator TransitionZoomInCoroutine(Camera cam, float initialCamSize, Vector3 initialCamPosition, float posyFinal, Action<string>  action = null)
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime;
            ZoomIn(cam, initialCamSize, initialCamPosition, time, posyFinal);
            yield return null;
            
        }
        action?.Invoke("Game");
    }

    private static void ZoomOut(Camera cam, float initialCamSize, Vector3 initialCamPosition, float time, float posyFinal)
    {
        cam.orthographicSize = Mathf.Lerp(0.5f, initialCamSize, Easing.QuadOut(time));
        
        cam.transform.position = Vector3.Lerp(new Vector3(0, posyFinal, -10f), initialCamPosition, Easing.QuadOut(time));

    }  

    public static IEnumerator TransitionZoomOutCoroutine(Camera cam, float initialCamSize, Vector3 initialCamPosition, float posyFinal)
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime;
            ZoomOut(cam, initialCamSize, initialCamPosition, time, posyFinal);
            yield return null;
            
        }
    }
}
