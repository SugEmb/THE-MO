using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ChatToggleController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject chatWindowPanel;
    [SerializeField] private TMP_InputField inputField;

    [Header("Input Action")]
    [SerializeField] private InputAction toggleAction = new InputAction("ToggleChat", binding: "<Keyboard>/q");

    private void OnEnable()
    {
        toggleAction.Enable();
        toggleAction.performed += OnTogglePressed;
    }

    private void OnDisable()
    {
        toggleAction.performed -= OnTogglePressed;
        toggleAction.Disable();
    }

    private void OnTogglePressed(InputAction.CallbackContext context)
    {
        ToggleChatWindow();
    }

    public void ToggleChatWindow()
    {
        if (chatWindowPanel == null) return;

        bool newState = !chatWindowPanel.activeSelf;
        chatWindowPanel.SetActive(newState);

        if (newState && inputField != null)
        {
            inputField.ActivateInputField();
        }
    }
}