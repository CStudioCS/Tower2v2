using UnityEngine;
using UnityEngine.EventSystems;

public class SetDefaultSelectedButton : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelectedGameObject;

    private GameObject lastSelected;
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(defaultSelectedGameObject);
    }

    void Update()
    {
        var current = EventSystem.current.currentSelectedGameObject;

        if (current == null)
            EventSystem.current.SetSelectedGameObject(lastSelected ?? defaultSelectedGameObject);
        else
            lastSelected = current;
    }
}