using UnityEngine;

public enum PanelType
{
    Draw,
    Discard
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
        }

        return false;
    }
}
