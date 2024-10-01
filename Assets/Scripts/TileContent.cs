using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TileContent : MonoBehaviour
{
    public enum TileStatus
    {
        Unvisted,
        Visted,
        Returned
    }

    private TileStatus status;
    /// <summary>
    /// Gets the TitleStatus of the Title. Set the Status to a new TitleStatus to update its colour.
    /// </summary>
    public TileStatus Status
    { 
        get { return status; }
        set
        {
            if (value != status)
            {
                switch (value)
                {
                    case TileStatus.Unvisted:
                        status = TileStatus.Unvisted;
                        UpdateTileColour(Color.white);
                        break;
                    case TileStatus.Visted:
                        status = TileStatus.Visted;
                        UpdateTileColour(Color.green);
                        break;
                    case TileStatus.Returned:
                        status = TileStatus.Returned;
                        UpdateTileColour(Color.red);
                        break;
                    default:
                        Debug.LogError($"Error with new status of this tiles status at {transform.position} position. Name {gameObject.name}");
                        break;
                }
            }
        }
    }
    private int[] gridCoordinate;
    public int[] GridCoordinate
    { 
        get { return gridCoordinate; }
        set
        {
            if (value.Length == 2)
            {
                gridCoordinate = value;
            }
            else
            {
                Debug.LogError($"Attempted to set gridCoordinate of {gameObject.name} with a array length not equal to 2.");
            }
        }
    }

    public TextMeshProUGUI TileTextDisplay
    { get; private set; }

    public Renderer Renderer 
    { get; private set; }

    private void Awake()
    {
        Renderer = GetComponent<Renderer>();
        Status = TileStatus.Unvisted;
        TileTextDisplay = transform.Find("Canvas/GridCoord").GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        TileTextDisplay.text = string.Empty;
        foreach (int coord in GridCoordinate)
        {
            TileTextDisplay.text += coord + " ";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Status = TileStatus.Unvisted;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Status = TileStatus.Visted;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Status = TileStatus.Returned;
        }
    }

    void UpdateTileColour(Color newColor)
    {
        Renderer.material.color = newColor;
    }
}
