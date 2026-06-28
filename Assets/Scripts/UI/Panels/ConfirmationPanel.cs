using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPanel : MonoBehaviour
{
    public Text messageText;
    public Button confirmButton;
    public Button cancelButton;

    private System.Action onConfirm;
    private System.Action onCancel;

    private static ConfirmationPanel _instance;
    public static ConfirmationPanel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ConfirmationPanel>();
            }
            return _instance;
        }
    }

    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        // Setup button listeners
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirm);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancel);
        }

        // Hide panel by default
        gameObject.SetActive(false);
    }

    public void Show(string message, System.Action onConfirmCallback, System.Action onCancelCallback = null)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        onConfirm = onConfirmCallback;
        onCancel = onCancelCallback;

        gameObject.SetActive(true);
    }

    private void OnConfirm()
    {
        gameObject.SetActive(false);
        onConfirm?.Invoke();
    }

    private void OnCancel()
    {
        gameObject.SetActive(false);
        onCancel?.Invoke();
    }
}
