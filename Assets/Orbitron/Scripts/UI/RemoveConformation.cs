using TMPro;
using UnityEngine;

public class RemoveConfirmationUI : MonoBehaviour
{
    [SerializeField] private PlanetBrowserUI browser;
    [SerializeField] private TMP_Text message;

    private string selectedPlanet;

    private void OnEnable()
    {
        BodyState body = browser.GetSelectedBody();

        if (body == null)
        {
            selectedPlanet = "";
            message.text = "No planet selected.";
            return;
        }

        selectedPlanet = body.name;
        message.text = $"Remove \"{selectedPlanet}\" ?";
    }

    public void Refresh()
{
    BodyState body = browser.GetSelectedBody();

    if (body == null)
    {
        selectedPlanet = "";
        message.text = "No planet selected.";
        return;
    }

    selectedPlanet = body.name;
    message.text = $"Remove \"{selectedPlanet}\" ?";
}

    public void ConfirmRemove()
    {
        if (string.IsNullOrEmpty(selectedPlanet))
            return;

        TCPClient.Instance.SendRemoveCommand(selectedPlanet);

        UIManager.Instance.CloseRemovePanel();
    }

    public void Cancel()
    {
        UIManager.Instance.CloseRemovePanel();
    }
}