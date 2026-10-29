using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;

    // เรียกฟังก์ชันนี้เมื่อกดปุ่มเปิด/ปิด
    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            bool isActive = menuPanel.activeSelf;
            menuPanel.SetActive(!isActive);
        }
    }

    // สำหรับใช้แยกปุ่ม เปิด อย่างเดียว
    public void OpenMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    // สำหรับใช้แยกปุ่ม ปิด อย่างเดียว
    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }
}