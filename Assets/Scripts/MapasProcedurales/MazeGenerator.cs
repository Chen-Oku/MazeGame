using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public float cellSize = 2f;

    private Cell[,] grid;
    private Stack<Vector2Int> stack = new Stack<Vector2Int>();
    private System.Random rand = new System.Random();

    void Start()
    {
        GenerateMaze();
        DrawMaze();
    }

    void GenerateMaze()
    {
        grid = new Cell[width, height];

        // Inicializar celdas
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new Cell();

        Vector2Int current = new Vector2Int(0, 0);
        grid[0, 0].visited = true;
        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Peek();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                Vector2Int chosen = neighbors[rand.Next(neighbors.Count)];
                RemoveWall(current, chosen);
                grid[chosen.x, chosen.y].visited = true;
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (cell.x > 0 && !grid[cell.x - 1, cell.y].visited) neighbors.Add(new Vector2Int(cell.x - 1, cell.y));
        if (cell.x < width - 1 && !grid[cell.x + 1, cell.y].visited) neighbors.Add(new Vector2Int(cell.x + 1, cell.y));
        if (cell.y > 0 && !grid[cell.x, cell.y - 1].visited) neighbors.Add(new Vector2Int(cell.x, cell.y - 1));
        if (cell.y < height - 1 && !grid[cell.x, cell.y + 1].visited) neighbors.Add(new Vector2Int(cell.x, cell.y + 1));

        return neighbors;
    }

    void RemoveWall(Vector2Int a, Vector2Int b)
    {
        if (a.x == b.x)
        {
            if (a.y > b.y)
            { grid[a.x, a.y].walls[2] = false; grid[b.x, b.y].walls[0] = false; } // S y N
            else
            { grid[a.x, a.y].walls[0] = false; grid[b.x, b.y].walls[2] = false; }
        }
        else if (a.y == b.y)
        {
            if (a.x > b.x)
            { grid[a.x, a.y].walls[3] = false; grid[b.x, b.y].walls[1] = false; } // O y E
            else
            { grid[a.x, a.y].walls[1] = false; grid[b.x, b.y].walls[3] = false; }
        }
    }

    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 cellPos = new Vector3(x * cellSize, 0, y * cellSize);
                Instantiate(floorPrefab, cellPos, Quaternion.identity, transform);

                if (grid[x, y].walls[0]) Instantiate(wallPrefab, cellPos + new Vector3(0, 0, cellSize / 2), Quaternion.identity, transform); // N
                if (grid[x, y].walls[1]) Instantiate(wallPrefab, cellPos + new Vector3(cellSize / 2, 0, 0), Quaternion.Euler(0, 90, 0), transform); // E
                if (grid[x, y].walls[2]) Instantiate(wallPrefab, cellPos + new Vector3(0, 0, -cellSize / 2), Quaternion.identity, transform); // S
                if (grid[x, y].walls[3]) Instantiate(wallPrefab, cellPos + new Vector3(-cellSize / 2, 0, 0), Quaternion.Euler(0, 90, 0), transform); // O
            }
        }
    }
}

[System.Serializable]
public class Cell
{
    public bool visited = false;
    public bool[] walls = { true, true, true, true };
}
