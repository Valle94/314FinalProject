using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class SnapInteractableSpawner : MonoBehaviour
{
    [SerializeField] GameObject snapInteractablePrefab;
    [SerializeField] List<GameObject> boardSquares;
    [SerializeField] List<GameObject> startPieces;
    [SerializeField] List<GameObject> spawnedPieces;
    [SerializeField] List<Material> materials;
    [SerializeField] GameObject playerOneButton;
    [SerializeField] GameObject playerTwoButton;
    [SerializeField] GameObject rotateBoardButton;

    private Vector3 firstSpawnPos = new Vector3(-0.21f, 0.036f, -0.21f);
    private float spacing = 0.06f;
    private int gridSize = 8;
    public bool piecesSpawned = false;

    void SpawnGrid()
    {
        Rigidbody parentRB = GetComponent<Rigidbody>();
        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // 1. Calculate the local position relative to the parent
                float xPos = firstSpawnPos.x + (x * spacing);
                float zPos = firstSpawnPos.z + (z * spacing);
                Vector3 localPos = new Vector3(xPos, firstSpawnPos.y, zPos);

                // 2. Convert that local position to a World Position 
                // based on where this script's object actually is.
                Vector3 worldSpawnPos = transform.TransformPoint(localPos);

                char columnLetter = (char)('a' + x);
                // Convert z (0-7) to '1'-'8'
                int rowNumber = z + 1;
                string chessCoord = $"{columnLetter}{rowNumber}";

                // 3. Spawn the object at that world position
                GameObject newSpawnPoint = Instantiate(snapInteractablePrefab, worldSpawnPos, transform.rotation, this.transform);

                newSpawnPoint.name = chessCoord;
                boardSquares.Add(newSpawnPoint);

                FieldInfo rbField = typeof(SnapInteractable).GetField("_rigidbody",
                                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (newSpawnPoint.TryGetComponent(out SnapInteractable snapScript))
                {
                    // We use Reflection to find the private field named "_rigidbody" 
                    // and force it to hold our parentRB reference.

                    if (rbField != null)
                    {
                        rbField.SetValue(snapScript, parentRB);
                    }
                }
            }
        }
    }

    public void SpawnGridAndPieces()
    {
        DestroySquaresAndPieces();
        SpawnGrid();
        StartCoroutine(SpawnPiecesAfterGrid());
    }

    IEnumerator SpawnPiecesAfterGrid()
    {
        if (!piecesSpawned)
        {
            yield return null; // Wait one frame for grid to fully initialize

            for (int i = 0; i < boardSquares.Count; i++)
            {
                if (startPieces[i] != null)
                {
                    // Spawn piece at EXACT socket position
                    GameObject newPiece = Instantiate(startPieces[i],
                                boardSquares[i].transform.position,
                                boardSquares[i].transform.rotation,
                                this.transform);

                    if (i < boardSquares.Count / 2)
                    {
                        newPiece.GetComponentInChildren<MeshRenderer>().material = materials[0];
                    }
                    else
                    {
                        newPiece.GetComponentInChildren<MeshRenderer>().material = materials[1];
                    }

                    spawnedPieces.Add(newPiece);
                }
            }
            piecesSpawned = true;
        }
    }

    public void DestroySquaresAndPieces()
    {
        foreach (GameObject piece in spawnedPieces)
        {
            if (piece != null) Destroy(piece);
        }
        spawnedPieces.Clear();
        piecesSpawned = false;

        foreach (GameObject square in boardSquares)
        {
            if (square != null) Destroy(square);
        }
        boardSquares.Clear();
    }

    public void RotateBoard()
    {
        // Important: Stop any existing rotation so they don't fight
        StopAllCoroutines();

        // Calculate a NEW target based on where we are now + 180 degrees
        Quaternion target = transform.rotation * Quaternion.Euler(0, 180, 0);
        StartCoroutine(RotateBoardCoroutine(target, 1f));
    }

    IEnumerator RotateBoardCoroutine(Quaternion targetRotation, float duration)
    {
        Button rotateButton = rotateBoardButton.GetComponent<Button>();
        rotateButton.interactable = false;
        Vector3 buttonOnePos = playerOneButton.transform.position;
        Vector3 buttonTwoPos = playerTwoButton.transform.position;

        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Smoothly interpolate
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            playerOneButton.transform.position = Vector3.Slerp(buttonOnePos, buttonTwoPos, t);
            playerTwoButton.transform.position = Vector3.Slerp(buttonTwoPos, buttonOnePos, t);

            // PAUSE here and wait for the next frame to draw
            yield return null;
        }

        rotateButton.interactable = true;

        // Snap to exact final value to clear any floating point errors
        transform.rotation = targetRotation;
    }
}
