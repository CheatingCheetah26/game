using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

[System.Serializable]
public class TilemapData
{
    public Vector3 size;
    public List<string> tiles = new List<string>();

    public static TilemapData Convert(Tilemap tilemap)
    {
        TilemapData newTMD = new TilemapData();
        newTMD.size = tilemap.size;
        Debug.Log(tilemap.size);
        foreach(var position in tilemap.cellBounds.allPositionsWithin)
        {
            newTMD.tiles.Add(tilemap.GetTile(position).name);
        }
        return newTMD;
    }

    public Tilemap Create(Tilemap templateTilemap)
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "tiles"));
        templateTilemap.size = new Vector3Int((int)size.x, (int)size.y,1);
        TileBase[] tileBases = assetBundle.LoadAllAssets<TileBase>();
        Debug.Log("loaded " + tileBases.Length + " assets");
        Dictionary<string, TileBase> library = new Dictionary<string, TileBase>();
        foreach(TileBase tileBase in tileBases)
        {
            library.Add(tileBase.name, tileBase);
        }

        Debug.Log("The boundaries are: ");
        Debug.Log(templateTilemap.cellBounds.xMin+" "+ templateTilemap.cellBounds.xMax + " " + templateTilemap.cellBounds.yMin + " " + templateTilemap.cellBounds.yMax + " ");


        int h = 0;
        foreach (var position in templateTilemap.cellBounds.allPositionsWithin)
        {
            Debug.Log(position.ToString()+"/"+h);
            
            Debug.Log("There is the tile: " + templateTilemap.GetTile(position).name);
            Debug.Log("I will replace it with: " + library[tiles[h]].name);
            templateTilemap.SetTile(position, library[tiles[h]]);
            h++;
        }
        Debug.Log("Done");
        assetBundle.Unload(false);
        return templateTilemap;
    }
}
