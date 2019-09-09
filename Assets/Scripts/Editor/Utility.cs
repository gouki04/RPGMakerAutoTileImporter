using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPGMakerAssetImporter
{
    internal struct Size
    {
        public int width;
        public int height;

        public Size(int w, int h)
        {
            width = w;
            height = h;
        }

        public Size(int s)
        {
            width = height = s;
        }

        public Size Half()
        {
            return new Size(width / 2, height / 2);
        }

        public Size Scale(int w_scale, int h_scale)
        {
            return new Size(width * w_scale, height * h_scale);
        }
    }

    internal static class Utility
    {
        public static Rect SubRectFromIdx(this Rect rect, int idx, Size cell_size)
        {
            var rows = rect.height / cell_size.height;
            var cols = rect.width / cell_size.width;
            return new Rect(
                cell_size.width * (idx % cols) + rect.x,
                cell_size.height * (rows - 1 - Mathf.FloorToInt(idx / cols)) + rect.y,
                cell_size.width, cell_size.height);
        }

        public static Sprite GetSpriteByName(this List<Sprite> sprs, string name)
        {
            foreach (var spr in sprs) {
                if (spr != null && spr.name == name) {
                    return spr;
                }
            }
            return null;
        }

        public static Sprite GetSpriteByIdx(this List<Sprite> sprs, int frame, int idx)
        {
            return sprs.GetSpriteByName(string.Format("tile_{0}_{1}", frame, idx));
        }

        public static Sprite[] GetSpritesByIdx(this List<Sprite> sprs, int frame_count, int idx)
        {
            var ret = new Sprite[frame_count];
            for (var i = 0; i < frame_count; ++i) {
                ret[i] = sprs.GetSpriteByIdx(i, idx);
            }
            return ret;
        }

        public static Tilemap CreateNewPalette(string folder_path, string name)
        {
            var go = CreateNewPalette(folder_path, name, 
                GridLayout.CellLayout.Rectangle, 
                GridPalette.CellSizing.Manual, new Vector3(1, 1, 0), 
                GridLayout.CellSwizzle.XYZ);
            return go.GetComponentInChildren<Tilemap>();
        }

        public static GameObject CreateNewPalette(string folder_path, string name, Grid.CellLayout layout, GridPalette.CellSizing cell_sizing, Vector3 cell_size, Grid.CellSwizzle swizzle)
        {
            var tmp_go = new GameObject(name);
            var grid = tmp_go.AddComponent<Grid>();

            // We set size to kEpsilon to mark this as new uninitialized palette
            // Nice default size can be decided when first asset is dragged in
            grid.cellSize = cell_size;
            grid.cellLayout = layout;
            grid.cellSwizzle = swizzle;
            CreateNewLayer(tmp_go, "Layer1", layout);

            var path = AssetDatabase.GenerateUniqueAssetPath(folder_path + "/" + name + ".prefab");

            Object prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(tmp_go, path, InteractionMode.AutomatedAction);
            var palette = GridPalette.CreateInstance<GridPalette>();
            palette.name = "Palette Settings";
            palette.cellSizing = cell_sizing;
            AssetDatabase.AddObjectToAsset(palette, prefab);
            PrefabUtility.ApplyPrefabInstance(tmp_go, InteractionMode.AutomatedAction);
            AssetDatabase.Refresh();

            GameObject.DestroyImmediate(tmp_go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        public static GameObject CreateNewLayer(GameObject paletteGO, string name, Grid.CellLayout layout)
        {
            var new_layer_go = new GameObject(name);
            var tilemap = new_layer_go.AddComponent<Tilemap>();
            var renderer = new_layer_go.AddComponent<TilemapRenderer>();
            new_layer_go.transform.parent = paletteGO.transform;
            new_layer_go.layer = paletteGO.layer;

            // Set defaults for certain layouts
            switch (layout) {
                case Grid.CellLayout.Hexagon: {
                        tilemap.tileAnchor = Vector3.zero;
                        break;
                    }
                case Grid.CellLayout.Isometric:
                case Grid.CellLayout.IsometricZAsY: {
                        renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
                        break;
                    }
            }

            return new_layer_go;
        }

        public static Vector3 CalculateAutoCellSize(Grid grid, Vector3 default_value)
        {
            var tilemaps = grid.GetComponentsInChildren<Tilemap>();
            foreach (var tilemap in tilemaps) {
                foreach (var position in tilemap.cellBounds.allPositionsWithin) {
                    var sprite = tilemap.GetSprite(position);
                    if (sprite != null) {
                        return new Vector3(sprite.rect.width, sprite.rect.height, 0f) / sprite.pixelsPerUnit;
                    }
                }
            }
            return default_value;
        }
    }
}
