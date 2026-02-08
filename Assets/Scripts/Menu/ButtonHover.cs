using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ButtonHover : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject buttonPlay;
    
    [SerializeField] private GameObject textPlay;
    
    [SerializeField] private GameObject buttonSettings;

    
    [SerializeField] private GameObject textSettings;
    
    [SerializeField] private GameObject buttonQuit;
    
    [SerializeField] private GameObject textQuit;
    
    
    [SerializeField] private GameObject buttonQuitSettings;
    [SerializeField] private GameObject textQuitSettings;

    [SerializeField] private float scaleFactor = 1.2f;
    [SerializeField] private Color hoverColor = Color.red;

    [SerializeField] private float hoverDuration = 0.2f;
    
    private void HoverButton(GameObject button, GameObject text)
    {
        text.transform.DOScale(scaleFactor, hoverDuration).SetEase(Ease.OutQuad);
        button.GetComponent<Image>().DOColor(hoverColor, hoverDuration).SetEase(Ease.OutQuad);
    }

    private void ExitHoverButton(GameObject button, GameObject text)
    {
        text.transform.DOScale(1f, hoverDuration).SetEase(Ease.OutQuad);
        button.GetComponent<Image>().DOColor(Color.white, hoverDuration).SetEase(Ease.OutQuad);
    }
    public void ButtonPlayHover(){ HoverButton(buttonPlay, textPlay); }
    public void ButtonPlayExitHover(){ ExitHoverButton(buttonPlay, textPlay); }
    
    public void ButtonSettingsHover(){ HoverButton(buttonSettings, textSettings); }
    public void ButtonSettingsExitHover(){ ExitHoverButton(buttonSettings, textSettings); }
    
    public void ButtonQuitHover(){ HoverButton(buttonQuit, textQuit); }
    public void ButtonQuitExitHover(){ ExitHoverButton(buttonQuit, textQuit); }
    
    public void ButtonQuitSettingsHover(){ HoverButton(buttonQuitSettings, textQuitSettings); }
    public void ButtonQuitSettingsExitHover(){ ExitHoverButton(buttonQuitSettings, textQuitSettings); }


}

