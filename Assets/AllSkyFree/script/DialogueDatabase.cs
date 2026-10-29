using System;
using System.Collections.Generic;
using UnityEngine;

// โครงสร้างคำตอบแต่ละหมวดหมู่
[Serializable]
public class DialogueResponse
{
    public string categoryName;             // ชื่อหมวดหมู่ เช่น Who, Where, Trust
    public List<string> keywords;           // รายการคำค้นหา (คีย์เวิร์ด)
    [TextArea(2, 5)]
    public List<string> randomReplies;     // รายการข้อความตอบกลับแบบสุ่ม
}

// โครงสร้างสำหรับเก็บไฟล์ Asset บน Unity
[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "ScriptableObjects/DialogueDatabase")]
public class DialogueDatabase : ScriptableObject
{
    [Header("Mission Dialogue Categories")]
    public List<DialogueResponse> responses;

    [Header("Off-topic / Fallback Replies")]
    [TextArea(2, 5)]
    public List<string> fallbackReplies;    // คำตอบเมื่อผู้เล่นพิมพ์คำที่ไม่ตรงคีย์เวิร์ด
}