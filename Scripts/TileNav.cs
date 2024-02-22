using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    // Represents a navigable tile within a grid-based pathfinding system.
    public class TileNav
    {
        // Property representing the grid position of the tile.
        public Vector3Int Position { get; private set; }
        
        // Indicates if the tile is traversable.
        public bool IsWalkable { get; set; }
        
        // Represents the cost of moving through this tile, useful for implementing terrain costs.
        public float MovementCost { get; set; }

        // Initializes a new instance of the TileNav class with specified position, walkability, and movement cost.
        public TileNav(Vector3Int position, bool isWalkable = true, float movementCost = 1.0f)
        {
            Position = position;
            IsWalkable = isWalkable;
            MovementCost = movementCost;
        }

        // Determines whether the specified object is equal to the current object.
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            TileNav other = (TileNav)obj;
            return Position.Equals(other.Position);
        }

        // Serves as the default hash function.
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}