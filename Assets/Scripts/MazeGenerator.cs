using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [field: SerializeField, Header("Tile Object")] public GameObject TilePrefab
    { get; private set; }
    [field: SerializeField, Header("Grid Height & Width")] public int GridHeight
    { get; private set; } = 3;
    [field: SerializeField] public int GridWidth
    { get; private set; } = 4;
    [field: SerializeField] public float GridSpacing
    { get; private set; } = 1.1f;
    public GameObject[,] MazeGrid
    { get; private set; }

    void Awake()
    {
        
    }

    void Start()
    {
        MazeGrid = GenerateGrid(GridHeight, GridWidth);

        // Testing.
        List<GameObject> nearbyTiles = CheckDirectionalTiles(MazeGrid[0, 0]);
        foreach (GameObject tile in nearbyTiles)
        {
            Destroy(tile);
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
    List<GameObject> CheckDirectionalTiles(GameObject tileToCheck)
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

            GameObject currentAdjacentTile;

            // Need to check if I am grabbing a reference outside of the array before null checking.
            if (MazeGrid[adjacentCoordinates[i, 0], adjacentCoordinates[i, 1]] != null)
            {
                currentAdjacentTile = MazeGrid[adjacentCoordinates[i, 0], adjacentCoordinates[i, 1]];
                adjacentTiles.Add(currentAdjacentTile);
            }   
        }

        return adjacentTiles;
    }
}
