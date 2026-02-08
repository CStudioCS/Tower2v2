using UnityEngine;
using System.Collections;
using System;
public static class Zoom
{
    private const float zoomDuration = 1f;
      
    public static IEnumerator TransitionZoomInCoroutine(Camera cam, float initialCamSize, Vector3 initialCamPosition, float posYFinal, float zoomSize, Action action)
    {
    
        float posZfinal = cam.transform.position.z;
        float posXfinal = cam.transform.position.x;
        Vector3 finalPosition = cam.transform.position;
        finalPosition.y = posYFinal;

        //Gere à chaque frame le zoom de la caméra 
        float time = 0f;
        while (time < zoomDuration)
        {
            time += Time.deltaTime;
            Zooming(cam, initialCamSize, initialCamPosition, time, finalPosition, zoomSize);
            yield return null;
            
        }
        
        action?.Invoke();
    }

    private static void Zooming(Camera cam, float initialCamSize, Vector3 initialCamPosition, float time, Vector3 finalPos, float zoomSize)
    {
        //Modifie le zoom de la caméra
        cam.orthographicSize = Mathf.Lerp(initialCamSize, zoomSize, Easing.QuadOut(time));
        //Modifie la position de la camérapour qu'elle zoom au bon endroit 
        cam.transform.position = Vector3.Lerp(initialCamPosition, finalPos, Easing.QuadOut(time));

    } 


}
