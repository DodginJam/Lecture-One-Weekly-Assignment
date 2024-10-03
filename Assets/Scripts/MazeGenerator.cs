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
    [field: SerializeField, Range(1, 100), Header("Maze Generation Parameters")] public int GridHeight
    { get; private set; } = 20;
    [field: SerializeField, Range(1, 100)] public int GridWidth
    { get; private set; } = 20;
    [field: SerializeField, Range(1.0f, 2.0f)] public float GridSpacing
    { get; private set; } = 1.1f;
    [field: SerializeField, Range(0.0f, 2.0f)] public float TimePerStep
    { get; private set; } = 0.0f;

    public GameObject[,] MazeGrid
    { get; private set; }
    public GameObject CurrentTile
    { get; private set; }
    public List<GameObject> AdjacentTiles
    { get; private set; }
    public List<GameObject> TileStack
    { get; private set; } = new List<GameObject>();

    public List<LineRenderer> LineRenders
    { get; private set; } = new List<LineRenderer>();
    public GameObject LineRendererContainer
    { get; private set; }
    public int LineRendererCount
    { get; private set; } = 0;
    public bool StartNewLine
    { get; private set; } = false;

    void Awake()
    {
        LineRendererContainer = new GameObject("LineRendererContainer");
        LineRendererContainer.transform.SetParent(transform, true);

        StartNewMazeLine();
    }

    /// <summary>
    /// Extend the current line renderer by adding a new position to extend towards.
    /// </summary>
    /// <param name="positionToExtendTo"></param>
    void ExtendMazeLine(Vector3 positionToExtendTo)
    {
        LineRenders[LineRenders.Count - 1].positionCount += 1;
        Vector3 yPositionOffset = new Vector3(0, 0.2f, 0);
        LineRenders[LineRenders.Count - 1].SetPosition(LineRenders[LineRenders.Count - 1].positionCount - 1, positionToExtendTo + yPositionOffset);
    }

    /// <summary>
    /// Create a new gameObject containing a new LineRenderer. Future ExtendsMazeLine method calls will use the latest created LineRenderer.
    /// </summary>
    void StartNewMazeLine()
    {
        // Set a new GameObject as a child of the LineRenderer Container gameObject.
        LineRendererCount++;
        GameObject newLineRendererGameObject = new GameObject($"LineRenderer{LineRendererCount}");
        newLineRendererGameObject.transform.SetParent(LineRendererContainer.transform);

        // Attached LineRenderer to the new GameObject and add it to list of LineRenders.
        LineRenderer newLineRenderer = newLineRendererGameObject.AddComponent<LineRenderer>();
        newLineRenderer.positionCount = 0;
        LineRenders.Add(newLineRenderer);
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

        // Testing line renderer
        ExtendMazeLine(CurrentTile.transform.position);

        while (TileStack.Count > 0)
        {
            tileScript = CurrentTile.GetComponent<TileContent>();
            AdjacentTiles = GetDirectionalTiles(MazeGrid[tileScript.GridCoordinate[0], tileScript.GridCoordinate[1]]);
            ChooseNextTileAsCurrent(AdjacentTiles);

            yield return new WaitForSeconds(TimePerStep);
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

        // Container to throw all the instantiated tiles into to keep hierarchy clean.
        GameObject tileContainer = new GameObject("TileContainer");
        tileContainer.transform.SetParent(gameObject.transform);

        for (int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                mazeGrid[i, j] = Instantiate(TilePrefab, new Vector3(j, 0, i) * tileLength * GridSpacing, Quaternion.identity, transform);
                mazeGrid[i, j].GetComponent<TileContent>().GridCoordinate = new int[] {i, j};

                mazeGrid[i, j].transform.SetParent(tileContainer.transform);
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
    void ChooseNextTileAsCurrent(List<GameObject> tilesToChoose)
    {
        // With no provided adjacent tiles to choose from, revert to the last tile that had an adjacent unvisited tile until none remain.
        if (tilesToChoose.Count <= 0)
        {
            // Flag that the a new line will be drawn, rather then extend the old one. New line will be drawn only once a visitable tile is found.
            StartNewLine = true;

            if (TileStack.Count > 1)
            {
                TileContent tileScript = CurrentTile.GetComponent<TileContent>();
                tileScript.Status = TileContent.TileStatus.Returned;

                TileStack.RemoveAt(TileStack.Count - 1);
                CurrentTile = TileStack[TileStack.Count - 1];
            }
            else
            {
                TileContent tileScript = CurrentTile.GetComponent<TileContent>();
                tileScript.Status = TileContent.TileStatus.Returned;

                // End choosing - maze generation should be over.
                TileStack.RemoveAt(TileStack.Count - 1);
                Debug.Log("End choosing - maze generation should be over.");
            }
        }
        else
        {
            // The (not-quite-yet) previous Vector is stored here for if a new line needs to be drawn due to more then 2 indices.
            Vector3 lastTileVector = CurrentTile.transform.position;

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

            if (StartNewLine == false)
            {
                ExtendMazeLine(CurrentTile.transform.position);
            }
            else
            {
                // When starting a new line, used the prior tiles Vector3 and current tiles Vector three to start drawing a new line.
                StartNewMazeLine();
                ExtendMazeLine(lastTileVector);
                ExtendMazeLine(CurrentTile.transform.position);

                // Flag set to false to ensure future drawn lines are extentions of current line renderer, if valid.
                StartNewLine = false;
            }
        }
    }
}
