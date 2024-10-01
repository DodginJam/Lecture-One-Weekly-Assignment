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

    public int[,] MazeGrid
    { get; private set; }

    void Awake()
    {
        
    }

    void Start()
    {
        MazeGrid = GenerateGrid(GridHeight, GridWidth);

        float gridLength = TilePrefab.GetComponent<MeshRenderer>().localBounds.size.x;

        for (int i = 0; i < MazeGrid.GetLength(0); i++)
        {
            for (int j = 0; j < MazeGrid.GetLength(1); j++)
            {
                GameObject currentTile = Instantiate(TilePrefab, new Vector3(j, 0, i) * gridLength * GridSpacing, Quaternion.identity, transform);
            }
        }
    }

    void Update()
    {
        
    }

    int[,] GenerateGrid(int gridHeight, int gridWidth)
    {
        int[,] mazeGrid =  new int[gridHeight, gridWidth];

        int gridNumber = 1;

        for(int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                mazeGrid[i, j] = gridNumber;
                gridNumber++;
            }
        }

        return mazeGrid;
    }
}
