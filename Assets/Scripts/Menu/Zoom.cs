using UnityEngine;
using System.Collections;
using System;
public static class Zoom
{
    public static IEnumerator TransitionZoomInCoroutine(Camera cam, float zoomDuration, float initialCamSize, Vector3 initialCamPosition, Vector3 finalPos, float zoomSize)
    {
        finalPos.z = cam.transform.position.z;
        //Gere à chaque frame le zoom de la caméra 
        float time = 0f;
        while (time < zoomDuration)
        {
            time += Time.deltaTime;
            ModifyZoom(cam, initialCamSize, initialCamPosition, time, finalPos, zoomSize);
            yield return null;
            
        }

    }

    private static void ModifyZoom(Camera cam, float initialCamSize, Vector3 initialCamPosition, float time, Vector3 finalPos, float zoomSize)
    {
        //Modifie le zoom de la caméra
        cam.orthographicSize = Mathf.Lerp(initialCamSize, zoomSize, Easing.QuadOut(time));
        //Modifie la position de la camérapour qu'elle zoom au bon endroit 
        cam.transform.position = Vector3.Lerp(initialCamPosition, finalPos, Easing.QuadOut(time));

    } 


}
