using UnityEngine;
using UnityEngine.UI;

/*Définit les boutons qui ont des actions c'est à dire qu'au click elle ne s'execute pas directement mais qui attend un évenement pour s'éxecuter*/
public abstract class ActionButton : NoActionButton
{
    public abstract void Action();
}
