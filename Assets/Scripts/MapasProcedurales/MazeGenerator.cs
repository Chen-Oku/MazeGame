using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private MazeCell _mazeCellPrefab;

    [SerializeField]
    private int _mazeWitdth;

    [SerializeField]
    private int _mazeDepth;

    private MazeCell[,] _mazeGrid;


    void Start()
    {
        _mazeGrid = new MazeCell[_mazeWitdth, _mazeDepth];

        for (int x = 0; x < _mazeWitdth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                _mazeGrid[x, z] = Instantiate(_mazeCellPrefab, new Vector3(x, 0, z), Quaternion.identity);
            }
        }

        // Start generation from (0,0)
        if (_mazeWitdth > 0 && _mazeDepth > 0)
        {
            GenerateMaze(null, _mazeGrid[0, 0]);
        }
    }

    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        if (currentCell == null) return;

        currentCell.Visit();
        if (previousCell != null)
        {
            ClearWalls(previousCell, currentCell);
        }

        // Continue exploring until there are no unvisited neighbors
        var neighbors = GetUnvisitedCells(currentCell).ToList();
        while (neighbors.Count > 0)
        {
            var next = neighbors[Random.Range(0, neighbors.Count)];
            GenerateMaze(currentCell, next);
            neighbors = GetUnvisitedCells(currentCell).ToList();
        }
    }
    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell).ToList();
        if (unvisitedCells.Count == 0) return null;
        return unvisitedCells[Random.Range(0, unvisitedCells.Count)];
    }

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;
        var list = new List<MazeCell>();

        if (x + 1 < _mazeWitdth)
        {
            var cellToRight = _mazeGrid[x + 1, z];
            if (cellToRight != null && cellToRight.IsVisited == false)
            {
                list.Add(cellToRight);
            }
        }
        if (x - 1 >= 0)
        {
            var cellToLeft = _mazeGrid[x - 1, z];
            if (cellToLeft != null && cellToLeft.IsVisited == false)
            {
                list.Add(cellToLeft);
            }
        }
        if (z + 1 < _mazeDepth)
        {
            var cellToFront = _mazeGrid[x, z + 1];
            if (cellToFront != null && cellToFront.IsVisited == false)
            {
                list.Add(cellToFront);
            }
        }
        if (z - 1 >= 0)
        {
            var cellToBack = _mazeGrid[x, z - 1];
            if (cellToBack != null && cellToBack.IsVisited == false)
            {
                list.Add(cellToBack);
            }
        }

        return list;
    }


    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
        {
            return;
        }

        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }

        if (previousCell.transform.position.z < currentCell.transform.position.z)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }

        if (previousCell.transform.position.z > currentCell.transform.position.z)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
        
    }

//     void GenerateMaze()
//     {
//         grid = new Cell[width, height];

//         // Inicializar celdas
//         for (int x = 0; x < width; x++)
//             for (int y = 0; y < height; y++)
//                 grid[x, y] = new Cell();

//         Vector2Int current = new Vector2Int(0, 0);
//         grid[0, 0].visited = true;
//         stack.Push(current);

//         while (stack.Count > 0)
//         {
//             current = stack.Peek();
//             List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

//             if (neighbors.Count > 0)
//             {
//                 Vector2Int chosen = neighbors[rand.Next(neighbors.Count)];
//                 RemoveWall(current, chosen);
//                 grid[chosen.x, chosen.y].visited = true;
//                 stack.Push(chosen);
//             }
//             else
//             {
//                 stack.Pop();
//             }
//         }
//     }

//     List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
//     {
//         List<Vector2Int> neighbors = new List<Vector2Int>();

//         if (cell.x > 0 && !grid[cell.x - 1, cell.y].visited) neighbors.Add(new Vector2Int(cell.x - 1, cell.y));
//         if (cell.x < width - 1 && !grid[cell.x + 1, cell.y].visited) neighbors.Add(new Vector2Int(cell.x + 1, cell.y));
//         if (cell.y > 0 && !grid[cell.x, cell.y - 1].visited) neighbors.Add(new Vector2Int(cell.x, cell.y - 1));
//         if (cell.y < height - 1 && !grid[cell.x, cell.y + 1].visited) neighbors.Add(new Vector2Int(cell.x, cell.y + 1));

//         return neighbors;
//     }

//     void RemoveWall(Vector2Int a, Vector2Int b)
//     {
//         if (a.x == b.x)
//         {
//             if (a.y > b.y)
//             { grid[a.x, a.y].walls[2] = false; grid[b.x, b.y].walls[0] = false; } // S y N
//             else
//             { grid[a.x, a.y].walls[0] = false; grid[b.x, b.y].walls[2] = false; }
//         }
//         else if (a.y == b.y)
//         {
//             if (a.x > b.x)
//             { grid[a.x, a.y].walls[3] = false; grid[b.x, b.y].walls[1] = false; } // O y E
//             else
//             { grid[a.x, a.y].walls[1] = false; grid[b.x, b.y].walls[3] = false; }
//         }
//     }

//     void DrawMaze()
//     {
//         for (int x = 0; x < width; x++)
//         {
//             for (int y = 0; y < height; y++)
//             {
//                 Vector3 cellPos = new Vector3(x * cellSize, 0, y * cellSize);
//                 Instantiate(floorPrefab, cellPos, Quaternion.identity, transform);

//                 if (grid[x, y].walls[0]) Instantiate(wallPrefab, cellPos + new Vector3(0, 0, cellSize / 2), Quaternion.identity, transform); // N
//                 if (grid[x, y].walls[1]) Instantiate(wallPrefab, cellPos + new Vector3(cellSize / 2, 0, 0), Quaternion.Euler(0, 90, 0), transform); // E
//                 if (grid[x, y].walls[2]) Instantiate(wallPrefab, cellPos + new Vector3(0, 0, -cellSize / 2), Quaternion.identity, transform); // S
//                 if (grid[x, y].walls[3]) Instantiate(wallPrefab, cellPos + new Vector3(-cellSize / 2, 0, 0), Quaternion.Euler(0, 90, 0), transform); // O
//             }
//         }
//     }
// }

// [System.Serializable]
// public class Cell
// {
//     public bool visited = false;
//     public bool[] walls = { true, true, true, true };
}
