using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerChatManager : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField] private DialogueDatabase dialogueData; // ลากไฟล์ ScriptableObject มาใส่

    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text chatDisplayArea;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;  // ลากวัตถุ Content ใน Scroll View มาใส่
    [SerializeField] private Button sendButton;

    [Header("Settings")]
    [SerializeField] private string playerName = "Researcher";
    [SerializeField] private float typingSpeed = 0.03f; // ความเร็วในการพิมพ์ของ ECHO

    private bool isEchoTyping = false; // ป้องกันการกดส่งซ้ำขณะ ECHO กำลังพิมพ์

    private void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendMessageFromInput);
        }

        if (inputField != null)
        {
            inputField.onSubmit.AddListener(delegate { SendMessageFromInput(); });
        }
    }

    // ฟังก์ชันกดส่งข้อความของผู้เล่น
    public void SendMessageFromInput()
    {
        // หาก ECHO กำลังพิมพ์ข้อความอยู่ ไม่ให้กดส่งซ้ำ
        if (isEchoTyping) return;

        string userText = inputField.text.Trim();
        if (string.IsNullOrEmpty(userText)) return;

        // 1. แสดงข้อความผู้เล่น (YOU)
        chatDisplayArea.text += $"<color=#569CD6>YOU:</color> {userText}\n\n";
        ForceScrollToBottom();

        // 2. เคลียร์ช่องพิมพ์
        inputField.text = "";
        inputField.ActivateInputField();

        // 3. หาคำตอบของ ECHO แล้วเริ่มการพิมพ์
        string echoReply = GetEchoResponse(userText);
        StartCoroutine(TypeTextAndScroll(echoReply));
    }

    // ฟังก์ชันค้นหาคำตอบใน daialougedata
    private string GetEchoResponse(string playerInput)
    {
        string cleanedInput = playerInput.ToLower();

        if (dialogueData != null && dialogueData.responses != null)
        {
            foreach (var entry in dialogueData.responses)
            {
                foreach (var keyword in entry.keywords)
                {
                    if (cleanedInput.Contains(keyword.ToLower()))
                    {
                        string selectedReply = entry.randomReplies[Random.Range(0, entry.randomReplies.Count)];
                        return FormatReply(selectedReply);
                    }
                }
            }

            // ถ้าหาคีย์เวิร์ดไม่เจอ ให้ดึงคำตอบ Fallback
            if (dialogueData.fallbackReplies != null && dialogueData.fallbackReplies.Count > 0)
            {
                string fallback = dialogueData.fallbackReplies[Random.Range(0, dialogueData.fallbackReplies.Count)];
                return FormatReply(fallback);
            }
        }

        return FormatReply("I could not follow that question. Try asking again.");
    }

    private string FormatReply(string reply)
    {
        return reply.Replace("{name}", playerName);
    }

    // Coroutine พิมพ์ตัวอักษรทีละตัวของ ECHO พร้อมสั่ง Scroll
    private IEnumerator TypeTextAndScroll(string textToType)
    {
        isEchoTyping = true;

        // แสดงหัวข้อ ECHO
        chatDisplayArea.text += "<color=#4EC9B0>ECHO:</color> ";

        // วนลูปพิมพ์ทีละตัวอักษร
        foreach (char letter in textToType.ToCharArray())
        {
            chatDisplayArea.text += letter;
            ForceScrollToBottom(); // บังคับเลื่อน Scrollbar ตามทุกตัวอักษร
            yield return new WaitForSeconds(typingSpeed);
        }

        chatDisplayArea.text += "\n\n";
        ForceScrollToBottom();

        isEchoTyping = false;
    }

    // บังคับรีเฟรชความสูง UI และสั่งเลื่อน Scrollbar ลงล่างสุด
    // ปรับฟังก์ชัน Scroll ให้รอให้ Unity วาด Layout เสร็จก่อน 1 เฟรม
    private void ForceScrollToBottom()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        // 1. บังคับ Rebuild Layout ของ Content
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        // 2. รอจบ Frame ให้ UI คำนวณความสูงใหม่เรียบร้อย
        yield return new WaitForEndOfFrame();

        // 3. สั่ง Scroll ลงล่างสุด
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}