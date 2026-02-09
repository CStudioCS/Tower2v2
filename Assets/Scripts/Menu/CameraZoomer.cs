using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
public class CameraZoomer : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float zoomSize;
    [SerializeField] private float zoomDuration = 1f;

    private float initialCamSize;
    private Vector3 initialCamPosition;

    void Start()
    {
        initialCamSize = cam.orthographicSize;
        initialCamPosition = cam.transform.position;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    public IEnumerator ZoomIn(Vector3 finalPos)
    {
        finalPos.z = this.transform.position.z;
        LMotion.Create(this.transform.position, finalPos, zoomDuration).WithEase(Ease.OutQuad).Bind(x => this.transform.position = x);
        LMotion.Create(cam.orthographicSize, zoomSize, zoomDuration).WithEase(Ease.OutQuad).Bind(x => cam.orthographicSize = x);
        yield return new WaitForSeconds(zoomDuration);
    }

    public IEnumerator ReturnToNormalState()
    {
        LMotion.Create(this.transform.position, initialCamPosition, zoomDuration).WithEase(Ease.OutQuad).Bind(x => this.transform.position = x);
        LMotion.Create(cam.orthographicSize, initialCamSize, zoomDuration).WithEase(Ease.OutQuad).Bind(x => cam.orthographicSize = x);
        yield return new WaitForSeconds(zoomDuration);
    }
}
