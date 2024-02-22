using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Tilemaps;

namespace Pathfinding
{
    // The PathFinder class is designed to find paths on a tile-based map.
    public class PathFinder
    {
        // Limits the number of iterations to prevent infinite loops.
        public int calculatorPatience = 1000;

        // Struct to store data relevant to pathfinding calculations for each node.
        struct nodeData
        {
            public float g; // Actual distance from the start node.
            public float h; // Heuristic distance to the end node.

            public float f { get => g + h; } // Total cost of the node.
        }

        // Dictionary to hold the map's tiles and their navigability status.
        private Dictionary<Vector3Int, TileNav> tileMap;

        // Constructor that initializes the tileMap with a given map.
        public PathFinder(Dictionary<Vector3Int, TileNav> map)
        {
            tileMap = map;
        }

        // Retrieves a TileNav object at a given position if it exists.
        public TileNav GetTileAtPosition(Vector3Int position)
        {
            if (tileMap.TryGetValue(position, out TileNav tile))
            {
                return tile;
            }
            return null;   
        }

        // Returns a dictionary of walkable neighbour tiles and their step costs from a given tile.
        public Dictionary<TileNav, float> GetNeighboursAndStepCosts(TileNav currentTile)
        {
            Dictionary<TileNav, float> neighbours = new Dictionary<TileNav, float>();
            Vector3Int[] directions = {
                Vector3Int.up,    // (0, 1, 0)
                Vector3Int.down,  // (0, -1, 0)
                Vector3Int.left,  // (-1, 0, 0)
                Vector3Int.right, // (1, 0, 0)
                Vector3Int.up + Vector3Int.left,    // (-1, 1, 0)
                Vector3Int.up + Vector3Int.right,   // (1, 1, 0)
                Vector3Int.down + Vector3Int.left,  // (-1, -1, 0)
                Vector3Int.down + Vector3Int.right  // (1, -1, 0)
            };

            foreach (Vector3Int direction in directions)
            {
                TileNav neighbour = GetTileAtPosition(currentTile.Position + direction);
                if (neighbour != null && neighbour.IsWalkable)
                {
                    // Diagonals have a higher cost, approximating the square root of 2.
                    float cost = direction.x != 0 && direction.y != 0 ? 1.41421f : 1.0f;
                    neighbours.Add(neighbour, cost);
                }
            }
            return neighbours;
        }

        // Calculates the heuristic distance between two tiles.
        public float GetHeuristicDistance(TileNav start, TileNav end)
        {
            return Vector3Int.Distance(start.Position, end.Position);
        }

        // Attempts to generate a path from the start node to the end node, outputting the path as a list of tiles.
        public bool GeneratePath(TileNav startNode, TileNav endNode, out List<TileNav> path)
        {
            int patience = calculatorPatience;
            HashSet<TileNav> CLOSED = new HashSet<TileNav>();
            Dictionary<TileNav, nodeData> OPEN = new Dictionary<TileNav, nodeData> { { startNode, new nodeData { g = 0f, h = GetHeuristicDistance(startNode, endNode) } } };
            Dictionary<TileNav, TileNav> DIRECTIONS = new Dictionary<TileNav, TileNav>();

            while (patience > 0) 
            {
                patience--;
                if (OPEN.Count == 0) break; // Exit if no path is found.

                TileNav currentNode = OPEN.Aggregate((l, r) => l.Value.f < r.Value.f ? l : r).Key;
                nodeData currentNodeData = OPEN[currentNode];
                OPEN.Remove(currentNode);
                CLOSED.Add(currentNode);

                // If the end node is reached, backtrack to create the path.
                if(currentNode.Equals(endNode))
                {
                    List<TileNav> finalPath = new List<TileNav>();
                    TileNav tracebackStep = currentNode;
                    while (!tracebackStep.Equals(startNode))
                    {
                        finalPath.Add(tracebackStep);
                        tracebackStep = DIRECTIONS[tracebackStep];
                    }
                    finalPath.Reverse(); // Reverse to get the correct order.
                    path = finalPath;
                    return true;
                }

                // Explore neighbours, updating costs and directions.
                foreach (KeyValuePair<TileNav, float> neighbourDistancePair in GetNeighboursAndStepCosts(currentNode))
                {
                    TileNav neighbour = neighbourDistancePair.Key;
                    float currentToNeighbourDistance = neighbourDistancePair.Value;
                    if (CLOSED.Contains(neighbour)) continue;

                    float startToNeighbourDistance = currentToNeighbourDistance + currentNodeData.g;

                    if (!OPEN.ContainsKey(neighbour) || OPEN[neighbour].g > startToNeighbourDistance) 
                    {
                        DIRECTIONS[neighbour] = currentNode;
                        float heuristicDistance = GetHeuristicDistance(neighbour, endNode);
                        if(!OPEN.ContainsKey(neighbour)) 
                        {
                            OPEN.Add(neighbour, new nodeData { g = heuristicDistance, h = heuristicDistance });
                        }
                        else
                        {
                            OPEN[neighbour] = new nodeData { g = heuristicDistance, h = heuristicDistance };
                        }
                    }
                }
            }

            path = new List<TileNav>();
            return false; // Return false if a path cannot be found.
        }
    }
}
