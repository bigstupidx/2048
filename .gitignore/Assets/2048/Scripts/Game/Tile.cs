using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{

    [SerializeField] private Text _numberText;

    private SpriteRenderer _spriteRenderer;

    private int _number;
    public int Number { get { return _number; } set { _number = value; _numberText.text = value.ToString(); } }

    public Color TileColor { set { _spriteRenderer.color = value; } }
    public Color NumberColor { set { _numberText.color = value; } }

    public float WorldSpaceSize 
    { 
        get { return transform.localScale.x; }
        set { transform.localScale = Vector3.one * (value / _spriteRenderer.sprite.bounds.size.x); } 
    }

    public Vector2 WorldSpacePosition { get { return transform.position; } set { transform.position = value; } }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

}