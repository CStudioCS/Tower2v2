using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionLayerChanger : MonoBehaviour
{
    [SerializeField] private PlayerTeam playerTeam;
    [SerializeField] private GameObject collisionGameObject;
    [SerializeField] private string playerBlueCollisionLayerMaskName = "PlayerBlueCollision";
    [SerializeField] private string playerRedCollisionLayerMaskName = "PlayerRedCollision";
    private LayerMask playerBlueCollisionLayerMask = -1;
    private LayerMask playerRedCollisionLayerMask = -1;
    
    private LayerMask PlayerBlueCollisionLayerMask
    {
        get
        {
            if (playerBlueCollisionLayerMask == -1)
                playerBlueCollisionLayerMask = LayerMask.NameToLayer(playerBlueCollisionLayerMaskName);
            return playerBlueCollisionLayerMask;
        }
    }
    
    private LayerMask PlayerRedCollisionLayerMask
    {
        get
        {
            if (playerRedCollisionLayerMask == -1)
                playerRedCollisionLayerMask = LayerMask.NameToLayer(playerRedCollisionLayerMaskName);
            return playerRedCollisionLayerMask;
        }
    }

    private Dictionary<PlayerTeam.Team, LayerMask> teamLayerDictionary;
    private Dictionary<PlayerTeam.Team, LayerMask> TeamLayerDictionary
    {
        get
        {
            teamLayerDictionary ??= new Dictionary<PlayerTeam.Team, LayerMask>
            {
                { PlayerTeam.Team.Left, PlayerBlueCollisionLayerMask },
                { PlayerTeam.Team.Right, PlayerRedCollisionLayerMask }
            };
            return teamLayerDictionary;
        }
    }

    private void Start()
    {
        playerTeam.TeamChanged += OnTeamChanged;
        UpdateLayer();
    }

    private void OnTeamChanged()  => UpdateLayer();

    private LayerMask CurrentTeamLayerMask => TeamLayerDictionary[playerTeam.CurrentTeam];
    
    private void UpdateLayer() => SetLayer(CurrentTeamLayerMask);
    
    private void SetLayer(LayerMask layerMask) => collisionGameObject.layer = layerMask;

    private void OnDisable()
    {
        playerTeam.TeamChanged -= OnTeamChanged;
    }
}
