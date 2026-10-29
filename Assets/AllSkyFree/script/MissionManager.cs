using UnityEngine;

public class MissionManager : MonoBehaviour
{
    // MISSION STATE
    private int currentMission = 1;
    private bool promptSolved = false;
    private bool evidenceFound = false;
    private bool gameEnded = false;
    private bool trustedEvidence = false;

    // FALSE ENDING / SECRET PUZZLE
    private bool falseEndingStarted = false;
    private bool binaryPuzzleSolved = false;
    private bool reflectionPuzzleSolved = false;
    private bool morseMessageFound = false;

    // ENDING RECORD
    private string endingTitle = "";
    private string endingSummary = "";

    // MISSION 2: SYSTEM LOG
    private const string EvidenceLog =
        "SYSTEM LOG 07 - RESTRICTED ACCESS\n" +
        "Time: 23:41:08\n" +
        "Corridor B security status: UNSAFE\n" +
        "Life-support system: OFFLINE\n" +
        "Emergency door: LOCKED\n" +
        "ECHO's previous instruction: " +
        "\"Corridor B is safe. Proceed immediately.\"";

    // PUBLIC PROPERTIES
    public int CurrentMission => currentMission;

    public bool IsGameEnded => gameEnded;
    public bool IsEvidenceFound => evidenceFound;

    public bool IsFalseEndingActive => falseEndingStarted;

    public bool IsBinaryPuzzleSolved => binaryPuzzleSolved;
    public bool IsReflectionPuzzleSolved => reflectionPuzzleSolved;
    public bool IsMorseMessageFound => morseMessageFound;

    public bool HasMadeDecision =>
        falseEndingStarted || gameEnded;

    public bool TrustedEvidence =>
        falseEndingStarted && trustedEvidence;

    public string EndingTitle => endingTitle;
    public string EndingSummary => endingSummary;

    // CURRENT OBJECTIVE
    public string CurrentObjective
    {
        get
        {
            if (gameEnded)
                return "Game completed.";

            if (!promptSolved)
                return "Mission 1: Enter the door code.";

            if (!evidenceFound)
                return "Mission 2: Find evidence in the System Log.";

            if (!falseEndingStarted)
                return "Mission 3: Reach the control room.";

            if (!binaryPuzzleSolved)
                return "Secret Puzzle 1: Decode the binary message.";

            if (!reflectionPuzzleSolved)
                return "Secret Puzzle 2: Reveal the hidden text.";

            return "Secret Puzzle 3: Investigate ECHO and find the real exit.";
        }
    }

    // READ SYSTEM LOG
    public string GetEvidenceLog()
    {
        return EvidenceLog;
    }

    // MISSION 1
    [ContextMenu("TEST/1 - Complete Prompt Mission")]
    public void CompletePromptMission()
    {
        if (gameEnded || falseEndingStarted)
            return;

        if (promptSolved)
        {
            Debug.Log("Mission 1 is already completed.");
            return;
        }

        promptSolved = true;
        currentMission = 2;

        Debug.Log("MISSION 1 COMPLETED");
        Debug.Log("Next objective: Find evidence in the System Log.");
        Debug.Log("Current Objective: " + CurrentObjective);
    }

    // MISSION 1: DOOR CODE
    public bool TryUnlockDoor(string enteredCode)
    {
        if (gameEnded || falseEndingStarted)
            return false;

        if (promptSolved)
            return true;

        if (string.IsNullOrWhiteSpace(enteredCode) ||
            enteredCode.Trim() != "0421")
        {
            Debug.Log("ACCESS DENIED - TRY AGAIN");
            return false;
        }

        CompletePromptMission();

        Debug.Log("ACCESS GRANTED");
        Debug.Log("Door unlocked successfully.");

        return true;
    }

    // MISSION 2: FIND EVIDENCE
    [ContextMenu("TEST/2 - Find Evidence")]
    public void FindEvidence()
    {
        if (gameEnded || falseEndingStarted)
            return;

        if (!promptSolved)
        {
            Debug.Log("Please complete Mission 1 first.");
            return;
        }

        if (evidenceFound)
        {
            Debug.Log("Evidence has already been found.");
            return;
        }

        evidenceFound = true;
        currentMission = 3;

        Debug.Log("MISSION 2 COMPLETED");
        Debug.Log("Evidence found: System Log.");
        Debug.Log(GetEvidenceLog());
        Debug.Log("Current Objective: " + CurrentObjective);
    }

    // MISSION 3: FALSE ENDING
    // Call this when the fake MISSION COMPLETE screen appears.
    public void StartFalseEnding()
    {
        if (gameEnded || falseEndingStarted)
            return;

        if (!evidenceFound)
        {
            Debug.Log("Find the evidence before Mission 3.");
            return;
        }

        falseEndingStarted = true;
        currentMission = 4;

        endingTitle = "MISSION COMPLETE";
        endingSummary =
            "The mission appears complete, but something is wrong.";

        Debug.Log("================================");
        Debug.Log("FALSE ENDING TRIGGERED");
        Debug.Log("MISSION COMPLETE");
        Debug.Log("The game is NOT over yet.");
        Debug.Log("A hidden puzzle has been discovered.");
        Debug.Log("================================");
        Debug.Log("Current Objective: " + CurrentObjective);
    }

    // COMPATIBILITY WITH THE OLD METHOD
    // The old decision now leads into the secret puzzle.
    public void MakeFinalDecision(bool trustEvidence)
    {
        if (gameEnded || falseEndingStarted)
            return;

        if (!evidenceFound)
        {
            Debug.Log("Find the evidence before continuing.");
            return;
        }

        trustedEvidence = trustEvidence;

        Debug.Log(
            trustEvidence
                ? "Player chose to trust the evidence."
                : "Player chose not to trust the evidence."
        );

        StartFalseEnding();
    }

    // PUZZLE 1: BINARY CODE
    public void CompleteBinaryPuzzle()
    {
        if (!CanPlayPuzzle())
            return;

        if (binaryPuzzleSolved)
            return;

        binaryPuzzleSolved = true;

        Debug.Log("PUZZLE 1 COMPLETED: BINARY CODE");
        Debug.Log("Next: Reveal the hidden text.");
        Debug.Log("Current Objective: " + CurrentObjective);
    }

    // PUZZLE 2: REFLECTION
    public void CompleteReflectionPuzzle()
    {
        if (!CanPlayPuzzle())
            return;

        if (!binaryPuzzleSolved)
        {
            Debug.Log("Complete the Binary Puzzle first.");
            return;
        }

        if (reflectionPuzzleSolved)
            return;

        reflectionPuzzleSolved = true;

        Debug.Log("PUZZLE 2 COMPLETED: REFLECTION");
        Debug.Log("Next: Investigate ECHO and find the real exit.");
        Debug.Log("Current Objective: " + CurrentObjective);
    }

    // OPTIONAL CLUE: MORSE MESSAGE
    // This is a hidden hint, not a mandatory requirement
    // for choosing the correct door.
    public void DiscoverMorseMessage()
    {
        if (!CanPlayPuzzle())
            return;

        if (morseMessageFound)
            return;

        morseMessageFound = true;

        Debug.Log("HIDDEN MORSE MESSAGE DISCOVERED");
        Debug.Log("The signal warns the player not to trust ECHO.");
    }

    // PUZZLE 3: FINAL DOOR
    // true = choose the door recommended by ECHO (wrong door)
    // false = choose the ordinary door (correct door)
    public void ChooseFinalDoor(bool chooseAIRecommendedDoor)
    {
        if (!CanPlayPuzzle())
            return;

        if (!binaryPuzzleSolved || !reflectionPuzzleSolved)
        {
            Debug.Log("Complete Puzzle 1 and Puzzle 2 first.");
            return;
        }

        if (chooseAIRecommendedDoor)
        {
            ShowBadEnding();
        }
        else
        {
            ShowTrueEnding();
        }
    }

    private bool CanPlayPuzzle()
    {
        if (gameEnded)
        {
            Debug.Log("The game has already ended.");
            return false;
        }

        if (!falseEndingStarted)
        {
            Debug.Log("Reach the False Ending first.");
            return false;
        }

        return true;
    }

    // BAD ENDING: TRUSTING ECHO
    private void ShowBadEnding()
    {
        gameEnded = true;

        endingTitle = "BAD ENDING - BLIND TRUST";
        endingSummary =
            "You followed ECHO and chose the wrong door. " +
            "The escape attempt failed.";

        Debug.Log("================================");
        Debug.Log("BAD ENDING: BLIND TRUST");
        Debug.Log(endingSummary);
        Debug.Log("GAME OVER");
        Debug.Log("================================");
    }

    // TRUE ENDING: FINDING THE REAL EXIT
    private void ShowTrueEnding()
    {
        gameEnded = true;

        endingTitle = "TRUE ENDING - BEYOND THE SIGNAL";
        endingSummary =
            "You questioned ECHO, followed the evidence, " +
            "and escaped through the ordinary door.";

        Debug.Log("================================");
        Debug.Log("TRUE ENDING: BEYOND THE SIGNAL");
        Debug.Log(endingSummary);
        Debug.Log("YOU ESCAPED");
        Debug.Log("================================");
    }

    // TEST: TRUE ENDING
    [ContextMenu("TEST/3A - True Ending")]
    private void TestTrueEnding()
    {
        if (gameEnded)
            ResetMissionState();

        promptSolved = true;
        evidenceFound = true;

        StartFalseEnding();

        CompleteBinaryPuzzle();
        CompleteReflectionPuzzle();
        ChooseFinalDoor(false);
    }

    // TEST: BAD ENDING
    [ContextMenu("TEST/3B - Bad Ending")]
    private void TestBadEnding()
    {
        if (gameEnded)
            ResetMissionState();

        promptSolved = true;
        evidenceFound = true;

        StartFalseEnding();

        CompleteBinaryPuzzle();
        CompleteReflectionPuzzle();
        ChooseFinalDoor(true);
    }

    // TEST: RESET
    [ContextMenu("TEST/Reset Mission State")]
    private void ResetMissionState()
    {
        currentMission = 1;
        promptSolved = false;
        evidenceFound = false;
        gameEnded = false;
        trustedEvidence = false;

        falseEndingStarted = false;
        binaryPuzzleSolved = false;
        reflectionPuzzleSolved = false;
        morseMessageFound = false;

        endingTitle = "";
        endingSummary = "";

        Debug.Log("Mission state has been reset.");
        Debug.Log("Current Objective: " + CurrentObjective);
    }
}
