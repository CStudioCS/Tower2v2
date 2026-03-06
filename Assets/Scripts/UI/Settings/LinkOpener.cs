using UnityEngine;

public class LinkOpener : MonoBehaviour
{
	[SerializeField] private string url = "https://www.google.com/";
	
	public void OpenLink() => Application.OpenURL(url);
}
