using UnityEngine;

public class BackgroundCreator : MonoBehaviour
{
    public Game game;

    [Header("- Background -")]
    public Sprite backgroundSprite;
    public Color backgroundColor = new Color(187 / 255f, 172 / 255f, 160 / 255f);

    [Header("- Background Tile -")]
    public Sprite backgroundTileSprite;
    public Color backgroundTileColor = new Color(203 / 255f, 191 / 255f, 180 / 255f);

    public void Start()
    {
        InstantiateBackground();
        InstantiateBackgroundTiles();
    }

    private void InstantiateBackground()
    {
        float backgroundSize = game.TilesWorldSpacePositions[0, 0].x * -2
                                   + game.TileSize
                                   + game.TileSize / game.DistanceBetweenTiles * 2;

        InstantiatePrefab("Background",
                          backgroundSize,
                          Vector3.zero,
                          backgroundSprite,
                          0,
                          backgroundColor);
    }

    private void InstantiateBackgroundTiles()
    {
        int tilesCount = game.tilesCount;

        for (int i = 0; i < tilesCount * tilesCount; i++)
        {
            InstantiatePrefab("BackgroundTile",
                              game.TileSize,
                              game.TilesWorldSpacePositions[i / tilesCount, i % tilesCount],
                              backgroundTileSprite,
                              1,
                              backgroundTileColor);
        }
    }

    private void InstantiatePrefab(string name,
                                         float worldSpaceWidth,
                                         Vector3 worldSpacePosition,
                                         Sprite sprite,
                                         int sortingOrder,
                                         Color color)
    {
        GameObject obj = new GameObject();
        obj.name = name;
        obj.transform.position = worldSpacePosition;

        SpriteRenderer spriteRenderer = obj.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;

        double width = spriteRenderer.sprite.bounds.size.x;
        obj.transform.localScale = Vector2.one * (float)(worldSpaceWidth / width);
    }

}