using System;
using System.Collections.Generic;
using UnityEngine;

public class GroundMover : MonoBehaviour
{
    [Header("GameObject Tiles")]
    [SerializeField] private List<GameObject> tiles;
    
    [Header("Mover Attributes")]
    public float speed = 1f;

    private float _totalWidth = 0f;
    
    private struct TileAttributes
    {
        public GameObject Tile;
        public float Width;
    }

    private TileAttributes[] _tileAttributesArray;
    
    private void Start()
    {
        // we check to see if the list of tiles has any tile
        if (tiles.Count <= 0) return;
        
        // we initialize the struc array to keep the tile values
        _tileAttributesArray = new TileAttributes[tiles.Count];
        
        // we save the main spawner position
        var thisPos = transform.position;

        for (var index = 0; index < tiles.Count; index++)
        {
            var tile = tiles[index];

            // we align the tile with the main spawner
            tile.transform.forward = transform.forward;

            // we check to see if there is a box collider in the tile
            if (tile.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                var thisWidth = boxCollider.size.z;
                var nextPos = new Vector3(thisPos.x, thisPos.y, thisPos.z + _totalWidth);
                tile.transform.position = nextPos;

                _totalWidth += thisWidth;

                _tileAttributesArray[index].Tile = tile;
                _tileAttributesArray[index].Width = thisWidth;
            }
        }
    }

    private void Update()
    {
        for (var i = 0; i < _tileAttributesArray.Length; i++)
        {
            var tile = _tileAttributesArray[i].Tile;
            var width = _tileAttributesArray[i].Width;
            
            var movement = new Vector3(0, 0, speed);
            tile.transform.Translate(movement * Time.deltaTime);

            if (tile.transform.localPosition.z < -width)
            {
                var thisPos = tile.transform.position;
                var teleportPos = new Vector3(thisPos.x,thisPos.y,thisPos.z + _totalWidth);
                tile.transform.localPosition = teleportPos;
            }
        }
    }
}
