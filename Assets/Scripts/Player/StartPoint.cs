using Unity.VisualScripting;
using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [SerializeField] private PlayerTeam.Team team;
    public PlayerTeam.Team Team => team;
}
