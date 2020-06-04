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
        for(int i=(int)(-tilemap.size.x/2+0.5); i < tilemap.size.x/2; i++)
        {
            for(int j= (int)(-tilemap.size.y / 2+0.5);  j< tilemap.size.y / 2; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);
                newTMD.tiles.Add(tilemap.GetTile(pos).name);
            }
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


        int h = 0;
        for (int n = templateTilemap.cellBounds.xMin; n < templateTilemap.cellBounds.xMax; n++)
        {
            for (int p = templateTilemap.cellBounds.yMin; p < templateTilemap.cellBounds.yMax; p++)
            {
                Debug.Log("adding tile");
                Vector3Int pos = new Vector3Int(n, p, 0);
                Debug.Log(pos);
                Debug.Log(templateTilemap.GetTile(pos));
                templateTilemap.SetTile(pos, library[tiles[h]]);
                templateTilemap.SetColor(pos, new Color(h, h, h));
                templateTilemap.RefreshTile(pos);
                Debug.Log(templateTilemap.GetTile(pos));
                
                h++;
            }
        }
        Debug.Log("Done");
        assetBundle.Unload(true);
        return templateTilemap;
    }
}
