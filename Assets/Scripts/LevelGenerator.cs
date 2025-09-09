using System.Collections.Generic;
using UnityEngine;
using nsMaze;

[System.Serializable]
public class DecorationOption
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 1f;       //relative spawn chance
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f); //random scale range
}

public class LevelGenerator : MonoBehaviour
{
    [Header("Maze Setup")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject startPrefab;
    public GameObject goalPrefab;
    public int height = 10, width = 10;
    public float wallHeight = 2f, wallSeparation = 1f;

    [Header("Collectibles & Obstacles")]
    public GameObject starPrefab;
    public int starsToSpawn = 3;
    public int maxObstacles = 6;
    public List<GameObject> obstaclePrefabs = new();

    [Header("Decorations")]
    public int maxDecorations = 20;
    [Range(0f, 1f)] public float decorationChance = 0.6f;
    public List<DecorationOption> decorations = new();

    [Header("Randomness")]
    public int seed = 0;

    //Internal
    GameObject player;
    Cell[,] maze;
    Stack<Vector2Int> cellRecord;
    int usedTiles;
    public GameObject currentGoal { get; private set; }

    //Parents for hierarchy organization
    Transform mazePartsParent;
    Transform collectiblesParent;
    Transform obstaclesParent;
    Transform decorationsParent;

    void Start()
    {
        if (seed != 0) Random.InitState(seed);

        //Create hierarchy groups
        mazePartsParent = new GameObject("MazeParts").transform;
        collectiblesParent = new GameObject("Collectibles").transform;
        obstaclesParent = new GameObject("Obstacles").transform;
        decorationsParent = new GameObject("Decorations").transform;

        //Scale floor & walls
        wallPrefab.transform.localScale = new Vector3(wallSeparation, wallHeight, wallSeparation);
        floorPrefab.transform.localScale = new Vector3((height * wallSeparation) * 2.3f, 1, (width * wallSeparation) * 2.3f);

        //Place floor
        Instantiate(floorPrefab,
            new Vector3(height * wallSeparation, -0.5f, width * wallSeparation),
            Quaternion.identity,
            mazePartsParent);

        player = GameObject.FindGameObjectWithTag("Player");

        GenerateMaze();
        CreateMaze();

        SpawnStars();
        SpawnObstacles();
        SpawnDecorations();
    }

    // ------------------- Maze Helpers -------------------

    Vector3 CellToWorld_Floor(int i, int j) =>
        new Vector3(i * 2 * wallSeparation, 0f, j * 2 * wallSeparation);

    void GenerateMaze()
    {
        maze = new Cell[height, width];
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                maze[i, j] = new Cell();

        cellRecord = new Stack<Vector2Int>();
        Vector2Int position = new(0, 0);
        usedTiles = 1;
        maze[position.x, position.y].isUsed = true;
        cellRecord.Push(position);

        // Start tile
        Instantiate(startPrefab, CellToWorld_Floor(position.x, position.y), Quaternion.identity, mazePartsParent);
        player.transform.position = CellToWorld_Floor(position.x, position.y);

        while (usedTiles < height * width)
        {
            List<int> availableDir = CheckAvailable(cellRecord.Peek());
            if (availableDir.Count != 0)
            {
                int nextDir = availableDir[Random.Range(0, availableDir.Count)];
                switch (nextDir)
                {
                    case 0: maze[position.x, position.y].connected[0] = true; maze[position.x - 1, position.y].connected[2] = true; position.x--; break;
                    case 1: maze[position.x, position.y].connected[1] = true; maze[position.x, position.y + 1].connected[3] = true; position.y++; break;
                    case 2: maze[position.x, position.y].connected[2] = true; maze[position.x + 1, position.y].connected[0] = true; position.x++; break;
                    case 3: maze[position.x, position.y].connected[3] = true; maze[position.x, position.y - 1].connected[1] = true; position.y--; break;
                }
                maze[position.x, position.y].isUsed = true;
                cellRecord.Push(position);
                usedTiles++;
            }
            else
            {
                cellRecord.Pop();
                position = cellRecord.Peek();
            }
        }
    }

    List<int> CheckAvailable(Vector2Int position)
    {
        List<int> availableDir = new();
        int X = position.x, Y = position.y;
        if (X > 0 && !maze[X - 1, Y].isUsed) availableDir.Add(0);
        if (Y < width - 1 && !maze[X, Y + 1].isUsed) availableDir.Add(1);
        if (X < height - 1 && !maze[X + 1, Y].isUsed) availableDir.Add(2);
        if (Y > 0 && !maze[X, Y - 1].isUsed) availableDir.Add(3);
        return availableDir;
    }

    void CreateMaze()
    {
        //Outer borders
        for (int i = 0; i < 2 * height; i++)
        {
            Instantiate(wallPrefab, new Vector3(-wallSeparation, 0.5f, i * wallSeparation), Quaternion.identity, mazePartsParent);
            Instantiate(wallPrefab, new Vector3(2 * wallSeparation * width, 0.5f, i * wallSeparation), Quaternion.identity, mazePartsParent);
        }
        for (int i = 0; i < 2 * width; i++)
        {
            Instantiate(wallPrefab, new Vector3(i * wallSeparation, 0.5f, -wallSeparation), Quaternion.identity, mazePartsParent);
            Instantiate(wallPrefab, new Vector3(i * wallSeparation, 0.5f, 2 * wallSeparation * height), Quaternion.identity, mazePartsParent);
        }

        //Inner maze walls
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Instantiate(wallPrefab, new Vector3(wallSeparation * (j * 2 + 1), 0.5f, wallSeparation * (i * 2 + 1)), Quaternion.identity, mazePartsParent);
                if (!maze[i, j].connected[2]) Instantiate(wallPrefab, new Vector3(j * 2 * wallSeparation, 0.5f, wallSeparation * (i * 2 + 1)), Quaternion.identity, mazePartsParent);
                if (!maze[i, j].connected[1]) Instantiate(wallPrefab, new Vector3(wallSeparation * (j * 2 + 1), 0.5f, i * 2 * wallSeparation), Quaternion.identity, mazePartsParent);
            }
        }

        //Goal tile
        Vector2Int goalPos = new Vector2Int(Random.Range(0, height), Random.Range(0, width));
        currentGoal = Instantiate(goalPrefab, CellToWorld_Floor(goalPos.x, goalPos.y), Quaternion.identity, mazePartsParent);
        currentGoal.SetActive(false);
    }

    // ------------------- Stars -------------------

    void SpawnStars()
    {
        List<Vector2Int> all = new();
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                all.Add(new Vector2Int(i, j));

        //remove start & goal
        all.Remove(new Vector2Int(0, 0));
        if (currentGoal != null)
        {
            var gi = Mathf.RoundToInt(currentGoal.transform.position.x / (2f * wallSeparation));
            var gj = Mathf.RoundToInt(currentGoal.transform.position.z / (2f * wallSeparation));
            all.Remove(new Vector2Int(gi, gj));
        }

        //shuffle
        for (int n = all.Count - 1; n > 0; n--)
        {
            int k = Random.Range(0, n + 1);
            (all[n], all[k]) = (all[k], all[n]);
        }

        for (int i = 0; i < Mathf.Min(starsToSpawn, all.Count); i++)
            Instantiate(starPrefab, CellToWorld_Floor(all[i].x, all[i].y), Quaternion.identity, collectiblesParent);
    }

    // ------------------- Obstacles -------------------

    void SpawnObstacles()
    {
        if (obstaclePrefabs.Count == 0) return;

        List<Vector2Int> all = new();
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                all.Add(new Vector2Int(i, j));

        //Remove start & goal
        all.Remove(new Vector2Int(0, 0));
        if (currentGoal != null)
        {
            var gi = Mathf.RoundToInt(currentGoal.transform.position.x / (2f * wallSeparation));
            var gj = Mathf.RoundToInt(currentGoal.transform.position.z / (2f * wallSeparation));
            all.Remove(new Vector2Int(gi, gj));
        }

        // shuffle
        for (int n = all.Count - 1; n > 0; n--)
        {
            int k = Random.Range(0, n + 1);
            (all[n], all[k]) = (all[k], all[n]);
        }

        int count = Mathf.Min(maxObstacles, all.Count);
        for (int i = 0; i < count; i++)
        {
            var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            var pos = CellToWorld_Floor(all[i].x, all[i].y);
            Instantiate(prefab, pos, Quaternion.identity, obstaclesParent);
        }
    }

    // ------------------- Decorations -------------------

    void SpawnDecorations()
    {
        if (decorations.Count == 0) return;

        int spawned = 0;
        float edgeOffset = wallSeparation * 0.5f; //distance to wall
        float wallLength = wallSeparation * 2f;   //length of the cell wall

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (spawned >= maxDecorations) return;
                if (Random.value > decorationChance) continue;

                // avoid start & goal
                if (i == 0 && j == 0) continue;
                if (currentGoal != null)
                {
                    var gi = Mathf.RoundToInt(currentGoal.transform.position.x / (2f * wallSeparation));
                    var gj = Mathf.RoundToInt(currentGoal.transform.position.z / (2f * wallSeparation));
                    if (i == gi && j == gj) continue;
                }

                var decoOpt = PickWeightedDecoration();
                if (decoOpt == null) continue;

                var basePos = CellToWorld_Floor(i, j);

                //candidates along walls
                List<Vector3> candidates = new();

                if (!maze[i, j].connected[1]) //north wall
                    candidates.Add(basePos + new Vector3(Random.Range(-wallLength / 2, wallLength / 2), 0, edgeOffset));
                if (!maze[i, j].connected[3]) //south wall
                    candidates.Add(basePos + new Vector3(Random.Range(-wallLength / 2, wallLength / 2), 0, -edgeOffset));
                if (!maze[i, j].connected[2]) //east wall
                    candidates.Add(basePos + new Vector3(edgeOffset, 0, Random.Range(-wallLength / 2, wallLength / 2)));
                if (!maze[i, j].connected[0]) //west wall
                    candidates.Add(basePos + new Vector3(-edgeOffset, 0, Random.Range(-wallLength / 2, wallLength / 2)));

                if (candidates.Count == 0) continue;

                Vector3 pos = candidates[Random.Range(0, candidates.Count)];
                Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                var obj = Instantiate(decoOpt.prefab, pos, rot, decorationsParent);

                float s = Random.Range(decoOpt.scaleRange.x, decoOpt.scaleRange.y);
                obj.transform.localScale *= s;

                spawned++;
            }
        }
    }

    DecorationOption PickWeightedDecoration()
    {
        float total = 0f;
        foreach (var d in decorations) total += d.weight;
        float r = Random.value * total;

        foreach (var d in decorations)
        {
            if (r < d.weight) return d;
            r -= d.weight;
        }
        return null;
    }
}
