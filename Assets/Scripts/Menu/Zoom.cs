using UnityEngine;
using System.Collections;
using System;
public static class Zoom
{
    private const float zoomDuration = 1f;
    private const float zoomedInSize = 0.5f;
    
    private static float posZfinal;
    private static float posXfinal;
      
    public static IEnumerator TransitionZoomInCoroutine(Camera cam, float initialCamSize, Vector3 initialCamPosition, float posYFinal, Action<string>  action = null)
    {
    
        posZfinal = cam.transform.position.z;
        posXfinal = cam.transform.position.x;

        //Gere à chaque frame le zoom de la caméra 
        float time = 0f;
        while (time < zoomDuration)
        {
            time += Time.deltaTime;
            ZoomIn(cam, initialCamSize, initialCamPosition, time, posYFinal);
            yield return null;
            
        }

        //Utiliser uniquement pour le play et evite de réecrire la fonction de zoom uniquement pour le play
        action?.Invoke("Game");
    }

    private static void ZoomIn(Camera cam, float initialCamSize, Vector3 initialCamPosition, float time, float posYFinal)
    {
        //Modifie le zoom de la caméra
        cam.orthographicSize = Mathf.Lerp(initialCamSize, zoomedInSize, Easing.QuadOut(time));
        //Modifie la position de la camérapour qu'elle zoom au bon endroit 
        cam.transform.position = Vector3.Lerp(initialCamPosition, new Vector3(posXfinal, posYFinal, posZfinal), Easing.QuadOut(time));

    } 

    public static IEnumerator TransitionZoomOutCoroutine(Camera cam, float initialCamSize, Vector3 initialCamPosition, float posYFinal)
    {

        posZfinal = cam.transform.position.z;
        posXfinal = cam.transform.position.x;


        //Gere à chaque frame le zoom de la caméra 
        float time = 0f;
        while (time < zoomDuration)
        {
            time += Time.deltaTime;
            ZoomOut(cam, initialCamSize, initialCamPosition, time, posYFinal);
            yield return null;
            
        }
    }

    private static void ZoomOut(Camera cam, float initialCamSize, Vector3 initialCamPosition, float time, float posYFinal)
    {
        //Modifie le zoom de la caméra
        cam.orthographicSize = Mathf.Lerp(zoomedInSize, initialCamSize, Easing.QuadOut(time));
        //Modifie la position de la caméra pour qu'elle zoom au bon endroit
        cam.transform.position = Vector3.Lerp(new Vector3(posXfinal, posYFinal, posZfinal), initialCamPosition, Easing.QuadOut(time));
    }  
}
