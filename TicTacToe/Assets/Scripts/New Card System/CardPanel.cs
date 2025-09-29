using UnityEngine;

public enum PanelType
{
    Draw,
    Discard,
    Pull // novo painel de puxar cartas
}

public class CardPanel : MonoBehaviour
{
    public PanelType panelType;

    public virtual void OnCardDropped(CardInteraction card)
    {
        switch (panelType)
        {
            case PanelType.Draw:
                break;

            case PanelType.Discard:
                Debug.Log("Carta enviada para o descarte.");
                break;

            case PanelType.Pull:
                Debug.Log("Cartas não podem ser dropadas no Pull Panel.");
                break;
        }
    }

    public virtual bool AcceptsCard(CardInteraction card)
    {
        switch (panelType)
        {
            case PanelType.Draw:
                return true;

            case PanelType.Discard:
                Debug.Log("Manual drop into Discard is not allowed.");
                return false;

            case PanelType.Pull:
                return false; // não aceita nenhuma carta sendo solta aqui
        }

        return false;
    }
}
