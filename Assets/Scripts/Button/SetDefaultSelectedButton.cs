using UnityEngine;
using UnityEngine.EventSystems;

public class SetDefaultSelectedButton : MonoBehaviour
{
    [SerializeField] private GameObject defaultButton;

    private GameObject lastSelected;
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(defaultButton);
    }

    void Update()
    {
        var current = EventSystem.current.currentSelectedGameObject;

        if (current != null)
            lastSelected = current;

        if (current == null)
            EventSystem.current.SetSelectedGameObject(lastSelected ?? defaultButton);
    }
}