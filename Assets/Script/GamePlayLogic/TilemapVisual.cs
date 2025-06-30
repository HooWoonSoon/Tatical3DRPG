
using System;
using System.Collections.Generic;
using UnityEngine;

public class TilemapVisual : Entity
{
    [System.Serializable]
    public struct TilemapSpriteUV
    {
        public GameNode.TilemapSprite tilemapSprite;
        public Vector2Int uv00Pixels;
        public Vector2Int uv11Pixels;
    }

    private struct UVsCoords
    {
        public Vector2 uv00;
        public Vector2 uv11;
    }

    [SerializeField] private TilemapSpriteUV[] tileSpriteUVArray;
    private Dictionary<GameNode.TilemapSprite, UVsCoords> uvCoordsDictionary;
    private bool updateMesh;
    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Texture texture = GetComponent<MeshRenderer>().material.mainTexture;
        float textureWidth = texture.width;
        float textureHeight = texture.height;

        uvCoordsDictionary = new Dictionary<GameNode.TilemapSprite, UVsCoords>();

        foreach (TilemapSpriteUV tilemapSpriteUV in tileSpriteUVArray)
        {
            uvCoordsDictionary[tilemapSpriteUV.tilemapSprite] = new UVsCoords
            {
                uv00 = new Vector2(tilemapSpriteUV.uv00Pixels.x / textureWidth, tilemapSpriteUV.uv00Pixels.y / textureHeight),
                uv11 = new Vector2(tilemapSpriteUV.uv11Pixels.x / textureWidth, tilemapSpriteUV.uv11Pixels.y / textureHeight)
            };
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    private void LateUpdate()
    {
        if (!updateMesh)
        {
            updateMesh = true;
            UpdateTilemapVisual();
        }
    }

    public void UpdateTilemapVisual()
    {
        Utils.CreateEmptyMeshArrays(world.worldMaxX * world.worldMaxY * world.worldMaxZ, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles);
        for (int x = 0; x < world.worldMaxX; x++)
        {
            for (int y = 0; y < world.worldMaxY; y++)
            {
                for (int z = 0; z < world.worldMaxZ; z++)
                {
                    int index = x * (world.worldMaxY * world.worldMaxZ) + y * world.worldMaxZ + z;
                    Vector3 cubeSize = Vector3.one;

                    GameNode node = world.GetNodeAtWorldPosition(x, y, z);
                    GameNode.TilemapSprite tilemapSprite = node.GetTilemapSprite();
                    Vector2 gridValueUV00, gridValueUV11;
                    if (tilemapSprite == GameNode.TilemapSprite.None)
                    {
                        gridValueUV00 = Vector2.zero;
                        gridValueUV11 = Vector2.zero;
                        cubeSize = Vector2.zero;
                    }
                    else
                    {
                        UVsCoords uvCoords = uvCoordsDictionary[tilemapSprite];
                        gridValueUV00 = uvCoords.uv00;
                        gridValueUV11 = uvCoords.uv11;
                    }
                    Utils.AddToMeshArrays(vertices, uvs, triangles, index, new Vector3Int(x, y, z), 0, cubeSize, gridValueUV00, gridValueUV11);
                }
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }
}

