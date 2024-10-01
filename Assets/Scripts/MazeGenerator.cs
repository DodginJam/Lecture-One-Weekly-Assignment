using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [field: SerializeField, Header("Tile Object")] public GameObject TilePrefab
    { get; private set; }
    [field: SerializeField, Header("Grid Height & Width")] public int GridHeight
    { get; private set; } = 20;
    [field: SerializeField] public int GridWidth
    { get; private set; } = 20;
    [field: SerializeField] public float GridSpacing
    { get; private set; } = 1.1f;
    public GameObject[,] MazeGrid
    { get; private set; }
    [field: SerializeField] public List<GameObject> AdjacentTiles
    { get; private set; }
    [field: SerializeField] public List<GameObject> TileStack
    { get; private set; } = new List<GameObject>();

    void Awake()
    {
        
    }

    void Start()
    {
        MazeGrid = GenerateGrid(GridHeight, GridWidth);

        GameObject startingTile = SetStartingTile();
        TileContent startingTileScript = startingTile.GetComponent<TileContent>();
        startingTileScript.Status = TileContent.TileStatus.Visted;
        TileStack.Add(startingTile);

        AdjacentTiles = GetDirectionalTiles(MazeGrid[startingTileScript.GridCoordinate[0], startingTileScript.GridCoordinate[1]]);

        foreach(GameObject tile in AdjacentTiles)
        {
            tile.GetComponent<TileContent>().Status = TileContent.TileStatus.Visted;
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

            // Null checking before adding the valid adjacent tile to the list of avaiable adjacent tile.
            if (MazeGrid[adjacentCoordinates[i, 0], adjacentCoordinates[i, 1]] != null)
            {
                currentAdjacentTile = MazeGrid[adjacentCoordinates[i, 0], adjacentCoordinates[i, 1]];
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
    /// Select a random starting tile as the start point. Start point is locked to the boundry of the grid.
    /// </summary>
    GameObject SetStartingTile()
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
}
