using UnityEngine;

public enum PanelType
{
    Draw,
    Combo,
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
                // No special behavior needed
                break;

            case PanelType.Combo:
                // Optional: trigger visual feedback or effects
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
            case PanelType.Combo:
                return true;

            case PanelType.Discard:
                Debug.Log("Manual drop into Discard is not allowed.");
                return false;
        }

        return false;
    }
}
