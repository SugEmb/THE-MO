using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text statusDisplayText; // ข้อความแสดงผลด้านบน (เช่น "เควส: ยังไม่สำเร็จ")

    [Header("Quest States")]
    public bool isQuestCompleted = false; // สถานะเควส (true = สำเร็จ, false = ยังไม่สำเร็จ)

    private void Start()
    {
        // อัปเดตข้อความเมื่อเริ่มเกม
        UpdateQuestUI();
    }

    // ฟังก์ชันสั่งอัปเดตข้อความแสดงผลด้านบน
    public void UpdateQuestUI()
    {
        if (statusDisplayText == null) return;

        if (isQuestCompleted)
        {
            statusDisplayText.text = "<color=green>STATUS: QUEST COMPLETED!</color>"; // สำเร็จ (สีเขียว)
        }
        else
        {
            statusDisplayText.text = "<color=yellow>STATUS: QUEST IN PROGRESS...</color>"; // ยังไม่สำเร็จ (สีเหลือง)
        }
    }

    // เรียกใช้ฟังก์ชันนี้เมื่อทำภารกิจสำเร็จ (เช่น กดปุ่ม หรือไขรหัสผ่านถูกต้อง)
    public void CompleteQuest()
    {
        if (!isQuestCompleted)
        {
            isQuestCompleted = true;
            UpdateQuestUI();
            Debug.Log("Quest Has Been Completed!");
        }
    }

    // สำหรับรีเซ็ตเควสกลับมาทำใหม่
    public void ResetQuest()
    {
        isQuestCompleted = false;
        UpdateQuestUI();
    }
}