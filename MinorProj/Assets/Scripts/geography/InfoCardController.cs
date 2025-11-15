// This script lives on your InfoCard_Prefab. It controls all the elements
// inside the card and handles its own buttons.
/*using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoCardController : MonoBehaviour
{
    [Header("Card UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image mountainPicture;
    public Button learnMoreButton;
    public Button closeButton;

    private string currentURL;

    void Start()
    {
        // Make the buttons call methods within this script
        closeButton.onClick.AddListener(CloseCard);
        learnMoreButton.onClick.AddListener(OpenURL);
    }

    // The GameManager calls this to fill the card with the correct information
    public void Populate(MountainData data)
    {
        titleText.text = data.displayName;
        descriptionText.text = data.description;
        mountainPicture.sprite = data.picture;
        currentURL = data.wikipediaURL;
    }

    void CloseCard()
    {
        gameObject.SetActive(false);
    }

    void OpenURL()
    {
        if (!string.IsNullOrEmpty(currentURL))
        {
            Application.OpenURL(currentURL);
        }
    }
}
*/