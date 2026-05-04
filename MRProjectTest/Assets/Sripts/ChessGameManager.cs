using System;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChessGameManager : MonoBehaviour
{
    [Header("Player 1")]
    [SerializeField] TextMeshProUGUI playerOneTimerText;
    [SerializeField] GameObject playerOneTimerPanel;
    [SerializeField] GameObject playerOneButton;
    [Header("Player 2")]
    [SerializeField] TextMeshProUGUI playerTwoTimerText;
    [SerializeField] GameObject playerTwoTimerPanel;
    [SerializeField] GameObject playerTwoButton;

    [Header("Time Control")]
    [SerializeField] Button fivePlusOneButton;
    [SerializeField] Button tenPlusOneButton;
    [SerializeField] Button fifteenPlusOneButton;


    [SerializeField] Material redButton;
    [SerializeField] Material greenButton;
    [SerializeField] SnapInteractableSpawner chessBoard;

    public double playerOneTime = 0;
    public double playerTwoTime = 0;
    private double increment = 3.0;

    private double startTime;
    private bool isPlayerOneTurn = false;
    private bool clockRunning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (!clockRunning) return;

        if (isPlayerOneTurn)
            playerOneTime -= Time.deltaTime;
        else
            playerTwoTime -= Time.deltaTime;

        if (playerOneTime <= 0 || playerTwoTime <= 0)
            EndGame();

        playerOneTimerText.text = FormatTime(playerOneTime);
        playerTwoTimerText.text = FormatTime(playerTwoTime);

        UpdatePanelColors(playerOneTimerPanel, playerOneTime);
        UpdatePanelColors(playerTwoTimerPanel, playerTwoTime);
    }

    public void SetTimers(int time)
    {
        playerOneTime = playerTwoTime = startTime = (double)time;
        playerOneTimerText.text = FormatTime(playerOneTime);
        playerTwoTimerText.text = FormatTime(playerTwoTime);
        fivePlusOneButton.interactable = tenPlusOneButton.interactable = fifteenPlusOneButton.interactable = false;
    }

    public void SwitchTurn(bool wasPlayerOne)
    {
        // Only switch if the correct player pressed their button
        if (wasPlayerOne == isPlayerOneTurn)
        {
            if (isPlayerOneTurn)
            {
                if (clockRunning) playerOneTime += increment;
                playerOneButton.GetComponent<PokeInteractable>().enabled = false;
                playerTwoButton.GetComponent<PokeInteractable>().enabled = true;
                playerOneButton.GetComponentInChildren<MeshRenderer>().material = redButton;
                playerTwoButton.GetComponentInChildren<MeshRenderer>().material = greenButton;
            }
            else
            {
                if (clockRunning) playerTwoTime += increment;
                playerOneButton.GetComponent<PokeInteractable>().enabled = true;
                playerTwoButton.GetComponent<PokeInteractable>().enabled = false;
                playerOneButton.GetComponentInChildren<MeshRenderer>().material = greenButton;
                playerTwoButton.GetComponentInChildren<MeshRenderer>().material = redButton;
            }

            isPlayerOneTurn = !isPlayerOneTurn;
            clockRunning = true; // Starts the clock on the first press
        }
    }

    public void ResetBoard()
    {
        StopClock();
        playerOneTime = playerTwoTime = startTime = (double)0;
        fivePlusOneButton.interactable = tenPlusOneButton.interactable = fifteenPlusOneButton.interactable = true;
        playerOneButton.GetComponent<PokeInteractable>().enabled = playerTwoButton.GetComponent<PokeInteractable>().enabled = true;
        playerOneButton.GetComponentInChildren<MeshRenderer>().material = playerTwoButton.GetComponentInChildren<MeshRenderer>().material = greenButton;
        chessBoard.DestroyPieces();
    }

    public void SwitchClock()
    {
        clockRunning = !clockRunning;
    }

    public void StopClock()
    {
        clockRunning = false;
    }

    private void EndGame()
    {
        clockRunning = false;
        Debug.Log("Flag Fall!");
    }

    private void UpdatePanelColors(GameObject playerPanel, double playerTime)
    {
        if (clockRunning)
        {
            if(playerTime >= startTime / 2)
            {
                playerPanel.GetComponent<Image>().color = Color.green;                
            }
            else if (playerTime >= startTime / 4)
            {
                playerPanel.GetComponent<Image>().color = Color.yellow;                
            }
            else if (playerTime >= startTime / 10)
            {
                playerPanel.GetComponent<Image>().color = Color.orange;                
            }
            else
            {
                playerPanel.GetComponent<Image>().color = Color.red;                
            }
        }
    }

    private string FormatTime(double seconds)
    {
        if (seconds < 0) seconds = 0;
        TimeSpan t = TimeSpan.FromSeconds(seconds);

        // Opening tag for fixed-width characters to prevent jitter
        string result = "<mspace=0.6em>";

        if (seconds >= 60)
        {
            // Format: M:SS
            result += string.Format("{0:D1}:{1:D2}", (int)t.TotalMinutes, t.Seconds);
        }
        else
        {
            // Format: SS.ff (hundredths)
            // t.Milliseconds / 10 gives you the 00-99 range
            result += string.Format("{0:D2}.{1:D2}", t.Seconds, t.Milliseconds / 10);
        }

        return result + "</mspace>";
    }
}
