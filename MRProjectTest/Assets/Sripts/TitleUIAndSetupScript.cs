using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class TitleUIAndSetupScript : MonoBehaviour
{
    #region VARIABLES
    [Header("Settings")]
    [SerializeField] Transform targetCamera;
    [SerializeField] Vector3 cameraOffset = new Vector3(0, 0, 1.0f);
    private bool followRotation = true;

    [Header("Spatial Anchor")]
    [SerializeField] SpatialAnchorCoreBuildingBlock anchorSystem;
    [SerializeField] Vector3 boardOffset = new Vector3(0, 0, 1.0f);
    [SerializeField] GameObject chessSystemPrefab;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject playButton;
    [SerializeField] GameObject confirmPlacementButton;
    [SerializeField] GameObject resetAnchorsButton;

    private GameObject activeChessBoard;
    #endregion

    #region UNITY LIFE CYCLE
    void Start()
    {
        // Get components
        if (targetCamera == null) targetCamera = Camera.main.transform;
        if (anchorSystem == null) anchorSystem = GetComponent<SpatialAnchorCoreBuildingBlock>();

        // Subscribe create and load events for spatial anchors.
        anchorSystem.OnAnchorCreateCompleted.AddListener(HandleAnchorCreated);
        anchorSystem.OnAnchorsLoadCompleted.AddListener(HandleAnchorsLoaded);

        // Set initial text overlay.
        statusText.text = "Press 'Start' To Begin!";
    }

    void LateUpdate() // Using LateUpdate so the game only 
    {
        if (targetCamera != null)
        {
            transform.position = targetCamera.TransformPoint(cameraOffset);

            if (followRotation)
            {
                transform.rotation = targetCamera.rotation;
            }
        }
    }
    #endregion

    #region SPATIAL ANCHOR LOGIC
    public void OnUserPressedStart()
    {
        LoadLastAnchor();
        startButton.SetActive(false);
    }

    private void LoadLastAnchor()
    {
        string savedId = PlayerPrefs.GetString("LastSavedAnchor", "");

        if (!string.IsNullOrEmpty(savedId))
        {
            statusText.text = "Searching for saved board...";
            Guid uuid = new Guid(savedId);
            anchorSystem.LoadAndInstantiateAnchors(chessSystemPrefab, new List<Guid> { uuid });
        }
        else
        {
            // No saved anchor found: Manual Setup Mode
            EnterManualSetupMode();
        }

    }

    // Call this from your "Delete/Relocate" Button
    public void RelocateBoard()
    {
        if (activeChessBoard != null)
        {
            resetAnchorsButton.SetActive(false);
            playButton.SetActive(false);
            // 1. Capture the current position before the anchor logic destroys it
            Vector3 lastPos = activeChessBoard.transform.position;
            Quaternion lastRot = activeChessBoard.transform.rotation;

            // 2. Clear from Meta's memory and PlayerPrefs
            anchorSystem.EraseAllAnchors();
            PlayerPrefs.DeleteKey("LastSavedAnchor");

            // 3. The Building Block will destroy the anchored board. 
            // We spawn a "dumb" grabbable copy at the exact same spot.
            activeChessBoard = Instantiate(chessSystemPrefab, lastPos, lastRot);
            SetBoardGrabbable(true);

            statusText.text = "Adjust placement, then press 'Confirm'.";
            confirmPlacementButton.SetActive(true);
        }
    }



    // --- CREATION LOGIC ---

    private void EnterManualSetupMode()
    {
        statusText.text = "Place your board in the room. Then press 'Confirm'.";
        Vector3 playerPos = targetCamera.position;
        // Ensure the board is level (upright) even if the user is looking down
        Vector3 flatForward = targetCamera.forward;
        flatForward.y = 0;
        flatForward.Normalize();

        // 3. Define the distance and a height offset (chest height)
        float spawnDistance = 0.5f;
        float heightOffset = -0.3f; // 30cm below eye level

        Vector3 spawnPos = playerPos + (flatForward * spawnDistance);
        spawnPos.y += heightOffset;

        // 4. Face the player (level rotation)
        Quaternion spawnRot = Quaternion.LookRotation(flatForward);

        activeChessBoard = Instantiate(chessSystemPrefab, spawnPos, spawnRot);
        SetBoardGrabbable(true);

        confirmPlacementButton.SetActive(true);
    }

    // Triggered by your UI Button
    public void ConfirmPlacement()
    {
        if (activeChessBoard != null)
        {
            confirmPlacementButton.SetActive(false);
            // Turn this specific instance into a Spatial Anchor
            anchorSystem.InstantiateSpatialAnchor(activeChessBoard, activeChessBoard.transform.position, activeChessBoard.transform.rotation);
            statusText.text = "Saving position...";
        }
    }
    private void HandleAnchorsLoaded(List<OVRSpatialAnchor> loadedAnchors)
    {
        if (loadedAnchors != null && loadedAnchors.Count > 0)
        {
            // SUCCESS: Board is in its physical home
            activeChessBoard = loadedAnchors[0].gameObject;
            SetBoardGrabbable(false);

            statusText.text = "Board located! Ready to play.";
            resetAnchorsButton.SetActive(true);
            playButton.SetActive(true);
        }
        else
        {
            // FAIL: ID exists but Meta couldn't find it in the room
            EnterManualSetupMode();
        }
    }
    #endregion

    private void HandleAnchorCreated(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if (result == OVRSpatialAnchor.OperationResult.Success)
        {
            // Save the ID for next time the app opens
            PlayerPrefs.SetString("LastSavedAnchor", anchor.Uuid.ToString());
            PlayerPrefs.Save();

            // CLEANUP: We now have two boards in the scene (the temporary one and the anchored one).
            // We destroy the temporary 'activeChessBoard'.
            if (activeChessBoard != null)
            {
                Destroy(activeChessBoard);
            }

            // The 'anchor' variable passed into this function IS the new board.
            // We set it as the active board and make sure it isn't grabbable.
            activeChessBoard = anchor.gameObject;
            SetBoardGrabbable(false);
            confirmPlacementButton.SetActive(false);
            playButton.SetActive(true);
            resetAnchorsButton.SetActive(true);

            statusText.text = "Board Anchor Set!";
        }
        else
        {
            statusText.text = "Failed to save. Try moving slightly.";
            confirmPlacementButton.SetActive(true); // Let them try again
        }
    }

    // --- HELPER ---

    private void SetBoardGrabbable(bool canGrab)
    {
        if (activeChessBoard == null) return;

        // Replace 'OVRGrabbable' with whatever grab script you are using
        var grabScript = activeChessBoard.GetComponentInChildren<Grabbable>();
        if (grabScript != null)
        {
            grabScript.enabled = canGrab;
        }

        // Optionally disable the Rigidbody gravity so it stays put
        // var rb = activeChessBoard.GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //     rb.isKinematic = !canGrab;
        // }
    }
}
