using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Data & Configuration")]
    [SerializeField] private DialogueDatabase currentDatabase;
    [SerializeField] private string playerName = "Researcher";

    [Header("UI References")]
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private TMP_Text dialogueDisplayArea;

    // ฟังก์ชันเรียกใช้งานเมื่อกดส่งข้อความ (On Submit / Click Send)
    public void OnPlayerSubmitInput()
    {
        string userInput = playerInputField.text.Trim();
        if (string.IsNullOrEmpty(userInput)) return;

        // แสดงข้อความของผู้เล่น
        DisplayMessage($"YOU: {userInput}");

        // ประมวลผลและตอบกลับจาก ECHO
        string response = ProcessInput(userInput);
        DisplayMessage($"ECHO: {response}");

        // ล้างช่องป้อนคำตอบ
        playerInputField.text = "";
    }

    private string ProcessInput(string input)
    {
        string cleanedInput = input.ToLower();

        // 1. ค้นหาคีย์เวิร์ดใน Database
        foreach (var entry in currentDatabase.responses)
        {
            foreach (var keyword in entry.keywords)
            {
                if (cleanedInput.Contains(keyword.ToLower()))
                {
                    // สุ่มดึงคำตอบ 1 ข้อจากรายการคำตอบของหมวดหมู่นั้น
                    string rawReply = entry.randomReplies[Random.Range(0, entry.randomReplies.Count)];
                    return FormatText(rawReply);
                }
            }
        }

        // 2. หากไม่เจอคีย์เวิร์ดเลย ให้ดึงคำตอบแบบ Fallback
        if (currentDatabase.fallbackReplies.Count > 0)
        {
            string fallback = currentDatabase.fallbackReplies[Random.Range(0, currentDatabase.fallbackReplies.Count)];
            return FormatText(fallback);
        }

        return FormatText("I could not follow that question. Try asking again.");
    }

    // ฟังก์ชันสำหรับใส่ชื่อผู้เล่นแทนที่ Tag {name}[cite: 1, 2]
    private string FormatText(string text)
    {
        return text.Replace("{name}", playerName);
    }

    private void DisplayMessage(string message)
    {
        dialogueDisplayArea.text += message + "\n";
    }
}