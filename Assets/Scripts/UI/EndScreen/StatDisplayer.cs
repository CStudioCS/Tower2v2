using TMPro;
using UnityEngine;

public class StatDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    
    public void Initialize(Stat stat)
    {
        text.text = $"{stat.title}\\n<size=150%>{stat.value}</size>";
    }
}
