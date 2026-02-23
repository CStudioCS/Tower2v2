using UnityEngine;

/*Cette classe a pour but de définir tous les boutons sans action (Mot définit dans la classe ActionButton).
 La classe Button existant déja dans Unity j'ai préféré l'appellé comme ça pour la différencier de celle qu'on une action*/
public abstract class NoActionButton : MonoBehaviour
{
    public abstract void OnClick();
}
