using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SnapInteractableSpawner : MonoBehaviour
{
    [SerializeField] GameObject snapInteractablePrefab;
    [SerializeField] List<GameObject> boardSquares;
    [SerializeField] List<GameObject> startPieces;
    [SerializeField] List<GameObject> spawnedPieces;
    [SerializeField] List<Material> materials;
    private Vector3 firstSpawnPos = new Vector3(-0.21f, 0.036f, -0.21f);
    private float spacing = 0.06f;
    private int gridSize = 8;


    void Start()
    {
        SpawnGrid();
        StartCoroutine(SpawnPiecesAfterGrid());
    }

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

    IEnumerator SpawnPiecesAfterGrid()
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
    }

    public void DestroyPieces()
    {
        foreach (GameObject piece in spawnedPieces)
        {
            Destroy(piece);
        }
    }
}
