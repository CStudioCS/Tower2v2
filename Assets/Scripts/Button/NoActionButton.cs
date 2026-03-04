using UnityEngine;
using UnityEngine.UI;

/*Cette classe a pour but de définir tous les boutons sans Action (Mot définit dans la classe ActionButton).
 *Elle evite qu'on déclare à chaque fois dans l'inspecteur l'action OnCLick() que doit effectuer le bouton*/
public abstract class NoActionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    protected Button Button => button;

    public void Awake() { button.onClick.AddListener(OnClick); }
    public abstract void OnClick();
}
