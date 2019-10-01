using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{

    [Header("- Setup -")]
    [Range(2, 10)]
    public int tilesCount = 3;

    [Header("- Input -")]
    public GameObject inputObject;

    [Header("- Tiles info -")]
    public GameObject tilePrefab;
    public TileInfo[] tilesInfo =
    {
        new TileInfo(2, new Color(240 / 255f, 229 / 255f, 221 / 255f), new Color(122 / 255f, 111 / 255f, 103 / 255f)),
        new TileInfo(4, new Color(238 / 255f, 225 / 255f, 203 / 255f), new Color(124 / 255f, 109 / 255f, 102 / 255f)),
        new TileInfo(8, new Color(243 / 255f, 176 / 255f, 125 / 255f)),
        new TileInfo(16, new Color(236 / 255f, 141 / 255f, 90 / 255f)),
        new TileInfo(32, new Color(246 / 255f, 124 / 255f, 100 / 255f)),
        new TileInfo(64, new Color(233 / 255f, 89 / 255f, 62 / 255f)),
        new TileInfo(128, new Color(245 / 255f, 215 / 255f, 155 / 255f)),
        new TileInfo(256, new Color(242 / 255f, 207 / 255f, 87 / 255f)),
        new TileInfo(512, new Color(229 / 255f, 192 / 255f, 61 / 255f)),
        new TileInfo(1024, new Color(226 / 255f, 186 / 255f, 47 / 255f)),
        new TileInfo(2048, new Color(237 / 255f, 196 / 255f, 44 / 255f))
    };

    private IGameInput _input;

    private Tile[,] _tiles;
    private List<Position> _vacantPositions = new List<Position>();

    // If tile world space size 1f, and _distanceBetweenTiles 5
    // World space distance between tiles will be 0.2f (1f / 5)
    private float _distanceBetweenTiles = 5;

    // How much space between tiles and camera's borders
    // For example, if _borderOffset = 4
    //                 tile worldSpace size 1f
    //                 and _distanceBetweenTiles 5
    // Then distance will be (1f / 5) * 4 / 2
    private float _borderOffset = 4;

    private float _movementSpeed = 26;
    private float _mergeTileSpeed = 1.5f;
    private float _tileAppearSpeed = 7;

    public float TileSize { get; private set; }
    public Vector2[,] TilesWorldSpacePositions { get; private set; }
    public float DistanceBetweenTiles { get { return _distanceBetweenTiles; } }

    public delegate void IncreaseScore(int number);

    public event System.Action CannotMoveEvent;
    public event IncreaseScore IncreaseScoreEvent;

    private void OnValidate()
    {
        if (inputObject && inputObject.GetComponent<IGameInput>() == null)
        {
            Debug.LogError("Game: Input Object has to have IGameInput implementation.");
            inputObject = null;
        }
    }

    #region Initialization

    private void Awake()
    {
        TileSize = GetTileSize();
        TilesWorldSpacePositions = GetTilesPositions(TileSize);
    }

    private void Start()
    {
        _input = inputObject.GetComponent<IGameInput>();

        _tiles = new Tile[tilesCount, tilesCount];
        InitializeVacantPositions();

        GenerateTile();
        GenerateTile();
    }

    private void InitializeVacantPositions()
    {
        for (int i = 0; i < tilesCount; i++)
            for (int j = 0; j < tilesCount; j++)
                _vacantPositions.Add(new Position(i, j));
    }

    private float GetTileSize()
    {
        float worldSpaceHeight = Camera.main.orthographicSize * 2f;
        float worldSpaceWidth = worldSpaceHeight / Screen.height * Screen.width;

        float minSideSize = worldSpaceHeight < worldSpaceWidth ? worldSpaceHeight : worldSpaceWidth;

        return minSideSize / ((tilesCount + _borderOffset - 1) / _distanceBetweenTiles + tilesCount);
    }

    private Vector2[,] GetTilesPositions(float tileSize)
    {
        Vector2[,] positions = new Vector2[tilesCount, tilesCount];

        float startPositionY = (tileSize * tilesCount
                                + tileSize * (tilesCount - 1f) / _distanceBetweenTiles
                                - tileSize) / 2f;
        float startPositionX = -startPositionY;
        float deltaPosition = tileSize + tileSize / _distanceBetweenTiles;

        float currentPositionX = startPositionX;
        for (int i = 0; i < tilesCount; i++)
        {
            currentPositionX = startPositionX;

            for (int j = 0; j < tilesCount; j++)
            {
                positions[i, j] = new Vector3(currentPositionX, startPositionY, 0);
                currentPositionX += deltaPosition;
            }

            startPositionY -= deltaPosition;
        }

        return positions;
    }

    #endregion

    private int movingTilesCount;

    private void Update()
    {
        if (movingTilesCount != 0)
            return;

        if (_input.IsSlideToLeft())
            MoveTilesLeft();
        else if (_input.IsSlideToRight())
            MoveTilesRight();
        else if (_input.IsSlideToDown())
            MoveTilesDown();
        else if (_input.IsSlideToUp())
            MoveTilesUp();
    }

    private void MoveTilesLeft()
    {
        for (int i = 0; i < tilesCount; i++)
        {
            int vacantPositionJ = 0;

            for (int j = 0; j < tilesCount; j++)
            {
                if (_tiles[i, j] == null || vacantPositionJ == j)
                    continue;

                // move
                if (_tiles[i, vacantPositionJ] == null)
                {
                    MoveTile(i, j, i, vacantPositionJ);
                }
                else
                {
                    // merge
                    if (_tiles[i, vacantPositionJ].Number == _tiles[i, j].Number)
                    {
                        MergeTiles(i, j, i, vacantPositionJ);
                        vacantPositionJ++;
                    }
                    // just move
                    else
                    {
                        vacantPositionJ++;
                        if (vacantPositionJ == j)
                            continue;

                        MoveTile(i, j, i, vacantPositionJ);
                    }
                }
            }
        }

        if (movingTilesCount != 0)
            StartCoroutine(WaitForTileGeneration());
    }

    private void MoveTilesRight()
    {
        for (int i = 0; i < tilesCount; i++)
        {
            int vacantJ = tilesCount - 1;

            for (int j = tilesCount - 1; j >= 0; j--)
            {
                if (_tiles[i, j] == null || vacantJ == j)
                    continue;

                // move
                if (_tiles[i, vacantJ] == null)
                {
                    MoveTile(i, j, i, vacantJ);
                }
                else
                {
                    // merge
                    if (_tiles[i, vacantJ].Number == _tiles[i, j].Number)
                    {
                        MergeTiles(i, j, i, vacantJ);
                        vacantJ--;
                    }
                    //  move
                    else
                    {
                        vacantJ--;
                        if (vacantJ == j)
                            continue;

                        MoveTile(i, j, i, vacantJ);
                    }
                }
            }
        }

        if (movingTilesCount != 0)
            StartCoroutine(WaitForTileGeneration());
    }

    private void MoveTilesUp()
    {
        for (int j = 0; j < tilesCount; j++)
        {
            int vacantI = 0;

            for (int i = 0; i < tilesCount; i++)
            {
                if (_tiles[i, j] == null || vacantI == i)
                    continue;

                // move
                if (_tiles[vacantI, j] == null)
                {
                    MoveTile(i, j, vacantI, j);
                }
                else
                {
                    // merge
                    if (_tiles[vacantI, j].Number == _tiles[i, j].Number)
                    {
                        MergeTiles(i, j, vacantI, j);
                        vacantI++;
                    }
                    // just move
                    else
                    {
                        vacantI++;
                        if (vacantI == i)
                            continue;

                        MoveTile(i, j, vacantI, j);
                    }
                }
            }
        }

        if (movingTilesCount != 0)
            StartCoroutine(WaitForTileGeneration());
    }

    private void MoveTilesDown()
    {
        for (int j = 0; j < tilesCount; j++)
        {
            int vacantI = tilesCount - 1;

            for (int i = vacantI; i >= 0; i--)
            {
                if (_tiles[i, j] == null || vacantI == i)
                    continue;

                // move
                if (_tiles[vacantI, j] == null)
                {
                    MoveTile(i, j, vacantI, j);
                }
                else
                {
                    // merge
                    if (_tiles[vacantI, j].Number == _tiles[i, j].Number)
                    {
                        MergeTiles(i, j, vacantI, j);
                        vacantI--;
                    }
                    // just move
                    else
                    {
                        vacantI--;
                        if (vacantI == i)
                            continue;

                        MoveTile(i, j, vacantI, j);
                    }
                }
            }
        }

        if (movingTilesCount != 0)
            StartCoroutine(WaitForTileGeneration());
    }

    private IEnumerator WaitForTileGeneration()
    {
        while (movingTilesCount != 0)
            yield return null;

        GenerateTile();

        if (!CanMove() && CannotMoveEvent != null)
            CannotMoveEvent();
    }

    private void GenerateTile()
    {
        GameObject tileObj = Instantiate(tilePrefab);

        // Tile settings
        Tile tile = tileObj.GetComponent<Tile>();

        tile.Number = tilesInfo[0].number;
        tile.TileColor = tilesInfo[0].tileColor;
        tile.NumberColor = tilesInfo[0].numberColor;
        StartCoroutine(ShowTile(tile));

        // Random position
        int index = Random.Range(0, _vacantPositions.Count);
        Position position = _vacantPositions[index];

        // Remove from Vacant positions for Tile generation
        _vacantPositions.RemoveAt(index);

        int i = position.i;
        int j = position.j;

        tile.WorldSpacePosition = TilesWorldSpacePositions[i, j];

        // Adjency matrix of tiles
        _tiles[i, j] = tile;
    }

    private bool CanMove()
    {
        if (_vacantPositions.Count > 0)
            return true;

        for (int i = 0; i < tilesCount; i++)
            for (int j = 0; j < tilesCount - 1; j++)
                if (_tiles[i, j].Number == _tiles[i, j + 1].Number)
                    return true;

        for (int j = 0; j < tilesCount; j++)
            for (int i = 0; i < tilesCount - 1; i++)
                if (_tiles[i, j].Number == _tiles[i + 1, j].Number)
                    return true;

        return false;
    }

    private void MoveTile(int fromI, int fromJ, int toI, int toJ)
    {
        // Visual movement
        StartCoroutine(MoveTileUtility(_tiles[fromI, fromJ], TilesWorldSpacePositions[toI, toJ]));

        // Adjency matrix of tiles
        _tiles[toI, toJ] = _tiles[fromI, fromJ];
        _tiles[fromI, fromJ] = null;

        // Vacant positions for Tile generation
        _vacantPositions.Remove(new Position(toI, toJ));
        _vacantPositions.Add(new Position(fromI, fromJ));

        movingTilesCount++;
    }

    private void MergeTiles(int fromI, int fromJ, int toI, int toJ)
    {
        // Visual movement
        StartCoroutine(MergeTilesUtility(_tiles[toI, toJ], _tiles[fromI, fromJ]));

        // Adjency matrix of tiles
        _tiles[fromI, fromJ] = null;

        // Vacant position for Tile generation
        _vacantPositions.Add(new Position(fromI, fromJ));

        movingTilesCount++;
    }

    private IEnumerator MoveTileUtility(Tile tile, Vector3 position)
    {
        while (Vector3.Distance(tile.WorldSpacePosition, position) > 0.005f)
        {
            tile.WorldSpacePosition = Vector3.MoveTowards(tile.WorldSpacePosition, position, _movementSpeed * Time.deltaTime);
            yield return null;
        }

        tile.WorldSpacePosition = position;
        movingTilesCount--;
    }

    private IEnumerator MergeTilesUtility(Tile firstTile, Tile secondTile)
    {
        // Moves to position
        while (Vector3.Distance(firstTile.WorldSpacePosition, secondTile.WorldSpacePosition) > 0.005f)
        {
            secondTile.WorldSpacePosition = Vector3.MoveTowards(secondTile.WorldSpacePosition, firstTile.WorldSpacePosition, _movementSpeed * Time.deltaTime);
            yield return null;
        }

        // Destroy tile
        Destroy(secondTile.gameObject);

        // Udpate tile
        TileInfo nextTileInfo = GetNextTileInfo(firstTile);

        firstTile.Number = nextTileInfo.number;
        firstTile.TileColor = nextTileInfo.tileColor;
        firstTile.NumberColor = nextTileInfo.numberColor;

        StartCoroutine(IncreaseTileSize(firstTile));

        movingTilesCount--;

        // Score
        if (IncreaseScoreEvent != null)
            IncreaseScoreEvent(nextTileInfo.number);
    }

    private TileInfo GetNextTileInfo(Tile tile)
    {
        for (int i = 0; i < tilesInfo.Length; i++)
            if (tilesInfo[i].number == tile.Number)
                return tilesInfo[i + 1];

        return null;
    }

    private IEnumerator IncreaseTileSize(Tile tile)
    {
        float currentSize = TileSize;
        float desirableTileSize = TileSize * 1.2f;

        while (tile && currentSize < desirableTileSize)
        {
            tile.WorldSpaceSize = currentSize;
            currentSize += Time.deltaTime * _mergeTileSpeed;
            yield return null;
        }

        if (tile)
            StartCoroutine(DecreaseTileSize(tile));
    }

    private IEnumerator DecreaseTileSize(Tile tile)
    {
        float currentSize = tile.WorldSpaceSize;
        float desirableTileSize = TileSize;

        while (tile && currentSize > desirableTileSize)
        {
            tile.WorldSpaceSize = currentSize;
            currentSize -= Time.deltaTime * _mergeTileSpeed;
            yield return null;
        }

        if (tile)
            tile.WorldSpaceSize = TileSize;
    }

    private IEnumerator ShowTile(Tile tile)
    {
        float currentSize = 0;

        while (tile && currentSize < TileSize)
        {
            tile.WorldSpaceSize = currentSize;
            currentSize += Time.deltaTime * _tileAppearSpeed;

            yield return null;
        }

        if (tile)
            tile.WorldSpaceSize = TileSize;
    }

    private struct Position
    {
        public int i, j;

        public Position(int i, int j)
        {
            this.i = i;
            this.j = j;
        }
    }

    [System.Serializable]
    public class TileInfo
    {
        public int number;
        public Color tileColor;
        public Color numberColor = Color.white;

        public TileInfo(int number, Color tileColor)
        {
            this.number = number;
            this.tileColor = tileColor;
        }

        public TileInfo(int number, Color tileColor, Color numberColor)
        {
            this.number = number;
            this.tileColor = tileColor;
            this.numberColor = numberColor;
        }
    }
}