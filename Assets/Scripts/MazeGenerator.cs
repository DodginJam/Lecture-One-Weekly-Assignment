using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static TileContent;

public class MazeGenerator : MonoBehaviour
{
    [field: SerializeField, Header("Tile Object")] public GameObject TilePrefab
    { get; private set; }
    [field: SerializeField, Range(1, 100), Header("Grid Height & Width")] public int GridHeight
    { get; private set; } = 20;
    [field: SerializeField, Range(1, 100)] public int GridWidth
    { get; private set; } = 20;
    [field: SerializeField, Range(1.0f, 2.0f)] public float GridSpacing
    { get; private set; } = 1.1f;

    public GameObject[,] MazeGrid
    { get; private set; }
    public GameObject CurrentTile
    { get; private set; }
    public List<GameObject> AdjacentTiles
    { get; private set; }
    public List<GameObject> TileStack
    { get; private set; } = new List<GameObject>();

    void Awake()
    {
        
    }

    void Start()
    {
        StartCoroutine(StepByStep());
    }

    IEnumerator StepByStep()
    {
        MazeGrid = GenerateGrid(GridHeight, GridWidth);

        CurrentTile = SetStartingTile();
        TileContent tileScript = CurrentTile.GetComponent<TileContent>();
        tileScript.Status = TileContent.TileStatus.Visited;
        TileStack.Add(CurrentTile);
        Debug.Log($"Tile Chosen: {CurrentTile.GetComponent<TileContent>().GridCoordinate[0]}, {CurrentTile.GetComponent<TileContent>().GridCoordinate[1]}");

        while (TileStack.Count > 0)
        {
            tileScript = CurrentTile.GetComponent<TileContent>();
            AdjacentTiles = GetDirectionalTiles(MazeGrid[tileScript.GridCoordinate[0], tileScript.GridCoordinate[1]]);
            ChooseNextTile(AdjacentTiles);
            Debug.Log($"Tile Chosen: {CurrentTile.GetComponent<TileContent>().GridCoordinate[0]}, {CurrentTile.GetComponent<TileContent>().GridCoordinate[1]}");
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Update()
    {

    }

    /// <summary>
    /// Generate and return an instantiated grid of Tile gameObjects.
    /// </summary>
    /// <param name="gridHeight"></param>
    /// <param name="gridWidth"></param>
    /// <returns></returns>
    GameObject[,] GenerateGrid(int gridHeight, int gridWidth)
    {
        GameObject[,] mazeGrid =  new GameObject[gridHeight, gridWidth];

        float tileLength = TilePrefab.GetComponent<MeshRenderer>().localBounds.size.x;

        for (int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                mazeGrid[i, j] = Instantiate(TilePrefab, new Vector3(j, 0, i) * tileLength * GridSpacing, Quaternion.identity, transform);
                mazeGrid[i, j].GetComponent<TileContent>().GridCoordinate = new int[] {i, j};
            }
        }

        return mazeGrid;
    }

    /// <summary>
    /// Return a list of adjacent Tiles around the current selected Tile.
    /// </summary>
    /// <param name="tileToCheck"></param>
    List<GameObject> GetDirectionalTiles(GameObject tileToCheck)
    {
        List<GameObject> adjacentTiles = new List<GameObject>();

        int[] indexOfCurrentTile = tileToCheck.GetComponent<TileContent>().GridCoordinate;

        // Four arrays containing 2 length arrays each, which are to be the GridCoordinates of surrounding tiles.
        int[,] adjacentCoordinates = new int[4, 2];

        int[,] adjacentCoordinateOffsets = new int [4, 2]
        {
            {1, 0},
            {0, 1},
            {-1, 0},
            {0, -1}
        };

        // Add the adjacent gameObjects to the list if they exist and within bounds of grid.
        for (int i = 0; i < adjacentCoordinates.GetLength(0); i++)
        {
            adjacentCoordinates[i, 0] = indexOfCurrentTile[0] + adjacentCoordinateOffsets[i,0];
            adjacentCoordinates[i, 1] = indexOfCurrentTile[1] + adjacentCoordinateOffsets[i,1];

            // End this current iteration of the for loop if the co-ordinate falls outside of the arrays scope.
            if (adjacentCoordinates[i, 0] >= MazeGrid.GetLength(0) || adjacentCoordinates[i, 0] < 0 || adjacentCoordinates[i, 1] >= MazeGrid.GetLength(1) || adjacentCoordinates[i, 1] < 0)
            {
                continue;
            }

            GameObject currentAdjacentTile;
            TileContent currentAdjacentTileScript;

            // Null checking before adding the valid adjacent tile to the list of avaiable adjacent tile.
            if (MazeGrid[adjacentCoordinates[i, 0], adjacentCoordinates[i, 1]] != null)
            {
                currentAdjacentTile = MazeGrid[adjacentCoordinates[i, 0], adjacentCoordinates[i, 1]];
                currentAdjacentTileScript = currentAdjacentTile.GetComponent<TileContent>();

                // If the tile has been visted before, end this current iteraton i.e. don't add the tile to the list of adjacent tiles to visit.
                if (currentAdjacentTileScript.Status == TileStatus.Visited || currentAdjacentTileScript.Status == TileStatus.Returned)
                {
                    continue;
                }

                adjacentTiles.Add(currentAdjacentTile);
            }
            else
            {
                Debug.LogError($"Grid Tile at position {adjacentCoordinates[i, 0]} : {adjacentCoordinates[i, 1]} is null.");
            }
        }

        return adjacentTiles;
    }

    /// <summary>
    /// Selects a starting tile as the start point. Start point is locked to the boundry of the grid. Default sets to a random tile along the boundry. Can be set manually to custom non-boundry tile.
    /// </summary>
    GameObject SetStartingTile(int yCoord = -1, int xCoord = -1)
    {
        // By default both paramters are set to -1 leading to random starting tile selection. When any parameter is out of grid range, also then select a random starting tile.
        if ((yCoord == -1 && xCoord == -1) || yCoord < -1 || yCoord >= GridHeight || xCoord < -1 || xCoord >= GridWidth)
        {
            int coinFlip = UnityEngine.Random.Range(0, 2);
            int startingXPosition = 0;
            int startingYPosition = 0;

            if (coinFlip == 0)
            {
                startingXPosition = UnityEngine.Random.Range(0, GridWidth);
                startingYPosition = UnityEngine.Random.Range(0, 2) == 0 ? 0 : GridHeight - 1;
            }
            else if (coinFlip == 1)
            {
                startingYPosition = UnityEngine.Random.Range(0, GridHeight);
                startingXPosition = UnityEngine.Random.Range(0, 2) == 0 ? 0 : GridWidth - 1;
            }

            return MazeGrid[startingYPosition, startingXPosition];
        }
        else
        {
            return MazeGrid[yCoord, xCoord];
        }   
    }

    /// Set the CurrentTile variable to an adjacent tile within the list provided.
    void ChooseNextTile(List<GameObject> tilesToChoose)
    {
        // With no provided adjacent tiles to choose from, revert to the last tile that had an adjacent unvisited tile until none remain.
        if (tilesToChoose.Count <= 0)
        {
            if (TileStack.Count > 1)
            {
                TileContent tileScript = CurrentTile.GetComponent<TileContent>();
                tileScript.Status = TileContent.TileStatus.Returned;

                TileStack.RemoveAt(TileStack.Count - 1);
                CurrentTile = TileStack[TileStack.Count - 1];

                tileScript = CurrentTile.GetComponent<TileContent>();
                tileScript.Status = TileContent.TileStatus.Returned;
            }
            else
            {
                // End choosing - maze generation should be over.
                TileStack.RemoveAt(TileStack.Count - 1);
                Debug.Log("End choosing - maze generation should be over.");
            }
        }
        else
        {
            if (tilesToChoose.Count > 1)
            {
                CurrentTile = tilesToChoose[UnityEngine.Random.Range(0, tilesToChoose.Count)];
            }
            else
            {
                CurrentTile =  tilesToChoose[0];
            }

            TileContent tileScript = CurrentTile.GetComponent<TileContent>();
            tileScript.Status = TileContent.TileStatus.Visited;
            TileStack.Add(CurrentTile);
        }
    }
}
