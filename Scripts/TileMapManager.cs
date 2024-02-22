using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap obstacles;
    public Transform character;
    private Dictionary<Vector3Int, TileNav> tileNavData;
    private PathFinder pathFinder;
    private List<Vector3> pathToFollow;
    public LineRenderer lineRenderer;
    public TileNav tileNav;
    public Vector3[] currentPath;

    void Start()
    {
        // Initialize navigation data for all tiles and setup the pathfinder
        InitializeTileNavData();
        pathFinder = new PathFinder(tileNavData);
    }

    private void InitializeTileNavData()
    {
        // Populate tileNavData with information about each tile's navigability
        tileNavData = new Dictionary<Vector3Int, TileNav>();
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(position)) continue;

            bool isWalkable = !obstacles.HasTile(position);
            TileNav tileNav = new TileNav(position, isWalkable);
            tileNavData.Add(position, tileNav);
        }
    }

    public void InteractWithTile(Vector3Int position)
    {
        // Toggle the walkability of a tile at the given position
        if (tileNavData.TryGetValue(position, out TileNav tileNav))
        {
            tileNav.IsWalkable = !tileNav.IsWalkable;
        }
    }

    void Update()
    {
        // Handle input to either set a new end node or interact with tiles
        if (Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            SetEndNode();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            Debug.Log(GetTileCenter(cellPosition).ToString());
        }
    }

    private void SetEndNode()
    {
        // Determine the path from the character to the clicked position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

        if (tileNavData.TryGetValue(cellPosition, out TileNav endTile))
        {
            Vector3Int startCellPosition = tilemap.WorldToCell(character.position);
            if (tileNavData.TryGetValue(startCellPosition, out TileNav startTile))
            {
                if (pathFinder.GeneratePath(startTile, endTile, out List<TileNav> path))
                {
                    // Update the path to follow and visualize it
                    pathToFollow = path.Select(tileNav => GetTileCenter(tileNav.Position)).ToList();
                    currentPath = pathToFollow.ToArray();
                    StartCoroutine(FollowPath());
                    DrawPath(path);
                }
            }
        }
    }

    private void DrawPath(List<TileNav> path)
    {
        // Visualize the path using lines
        if (path.Count == 0) return;

        Vector3 characterPosition = GetTileCenter(tilemap.WorldToCell(character.position));
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = GetTileCenter(path[i].Position);
            Vector3 end = GetTileCenter(path[i + 1].Position);
            Debug.DrawLine(start, end, UnityEngine.Color.red, 5.0f);
        }
    }

    private IEnumerator FollowPath()
    {
        // Move the character along the calculated path
        foreach (Vector3 waypoint in pathToFollow)
        {
            while (Vector3.Distance(character.position, waypoint) > 0.01f)
            {
                character.position = Vector3.MoveTowards(character.position, waypoint, 1.5f * Time.deltaTime);
                yield return null;
            }
        }
    }

    private Vector3 GetTileCenter(Vector3Int cellPosition)
    {
        // Calculate the center position of a given cell
        Vector3 cellCenterOffset = new Vector3(tilemap.cellSize.x / 2, tilemap.cellSize.y / 2, 0);
        return tilemap.CellToWorld(cellPosition) + cellCenterOffset;
    }
}
