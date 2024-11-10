using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace RPGMakerAssetImporter
{
    public class AutoTileEditor : EditorWindow
    {
        private enum EAutoTileType
        {
            RPG_MAKER_XP,
            RPG_MAKER_MV
        }

        private Texture2D m_OrgTexture;
        private EAutoTileType m_Type;
        private int m_AnimationFrames = 1;
        private float m_AnimationSpeed = 1f;

        [MenuItem("Window/AutoTile Editor")]
        public static AutoTileEditor Create()
        {
            var window = GetWindow<AutoTileEditor>();
            window.titleContent = new("AutoTile Editor");

            return window;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="from_idx_list">[lt, rt, lb, rb]</param>
        /// <param name="to"></param>
        /// <param name="to_idx"></param>
        /// <param name="cell_size"></param>
        private static void CopyRegion(Texture2DRegion from, IReadOnlyList<int> from_idx_list, Texture2DRegion to, int to_idx, Size cell_size)
        {
            Assert.IsTrue(from_idx_list.Count == 4);
            var half_cell_size = cell_size.Half();
            to = to.SubRegionFromIdx(to_idx, cell_size);

            for (var i = 0; i < from_idx_list.Count; ++i) {
                from.SubRegionFromIdx(from_idx_list[i], half_cell_size).CopyTo(to.SubRegionFromIdx(i, half_cell_size));
            }
        }

        #region RPG_MAKER_AUTOTILE_CFG
        private static readonly List<int[]> ms_XpAutoTileMap = new() {
            new[] {26,27,32,33},
            new[] {4,27,32,33},
            new[] {26,5,32,33},
            new[] {4,5,32,33},
            new[] {26,27,32,11},
            new[] {4,27,32,11},
            new[] {26,5,32,11},
            new[] {4,5,32,11},
            new[] {26,27,10,33},
            new[] {4,27,10,33},
            new[] {26,5,10,33},
            new[] {4,5,10,33},
            new[] {26,27,10,11},
            new[] {4,27,10,11},
            new[] {26,5,10,11},
            new[] {4,5,10,11},
            new[] {24,25,30,31},
            new[] {24,5,30,31},
            new[] {24,25,30,11},
            new[] {24,5,30,11},
            new[] {14,15,20,21},
            new[] {14,15,20,11},
            new[] {14,15,10,21},
            new[] {14,15,10,11},
            new[] {28,29,34,35},
            new[] {28,29,10,35},
            new[] {4,29,34,35},
            new[] {4,29,10,35},
            new[] {26,27,44,45},
            new[] {4,39,44,45},
            new[] {38,5,44,45},
            new[] {4,5,44,45},
            new[] {24,29,30,35},
            new[] {14,15,44,45},
            new[] {12,13,18,19},
            new[] {12,13,18,11},
            new[] {16,17,22,23},
            new[] {16,17,10,23},
            new[] {40,41,46,47},
            new[] {4,41,46,47},
            new[] {36,37,42,43},
            new[] {36,5,42,43},
            new[] {12,17,18,23},
            new[] {12,13,42,43},
            new[] {36,41,42,47},
            new[] {16,17,46,47},
            new[] {12,17,42,47},
            new[] {12,17,42,47}
        };

        private static readonly List<int> ms_MvXpRemap = new()
        {
             0, 1, 0, 1, 2, 3,
             4, 5, 4, 5, 6, 7,
             8, 9, 9,10,10,11,
            12,13,13,14,14,15,
            12,13,13,14,14,15,
            16,17,17,18,18,19,
            16,17,17,18,18,19,
            20,21,21,22,22,23
        };

        private const int R0 = AutoTile.TilingRule.Neighbor.DontCare;
        private const int R1 = AutoTile.TilingRule.Neighbor.This;
        private const int R2 = AutoTile.TilingRule.Neighbor.NotThis;

        private static readonly List<int[]> ms_XpRuleMap = new()
        {
            // 0
            new[]
            {
                R1, R1, R1,
                R1,     R1,
                R1, R1, R1
            },
            // 1
            new[]
            {
                R2, R1, R1,
                R1,     R1,
                R1, R1, R1
            },
            // 2
            new[]
            {
                R1, R1, R2,
                R1,     R1,
                R1, R1, R1
            },
            // 3
            new[]
            {
                R2, R1, R2,
                R1,     R1,
                R1, R1, R1
            },
            // 4
            new[]
            {
                R1, R1, R1,
                R1,     R1,
                R1, R1, R2
            },
            // 5
            new[]
            {
                R2, R1, R1,
                R1,     R1,
                R1, R1, R2
            },
            // 6
            new[]
            {
                R1, R1, R2,
                R1,     R1,
                R1, R1, R2
            },
            // 7
            new[]
            {
                R2, R1, R2,
                R1,     R1,
                R1, R1, R2
            },
            // 8
            new[]
            {
                R1, R1, R1,
                R1,     R1,
                R2, R1, R1
            },
            // 9
            new[]
            {
                R2, R1, R1,
                R1,     R1,
                R2, R1, R1
            },
            // 10
            new[]
            {
                R1, R1, R2,
                R1,     R1,
                R2, R1, R1
            },
            // 11
            new[]
            {
                R2, R1, R2,
                R1,     R1,
                R2, R1, R1
            },
            // 12
            new[]
            {
                R1, R1, R1,
                R1,     R1,
                R2, R1, R2
            },
            // 13
            new[]
            {
                R2, R1, R1,
                R1,     R1,
                R2, R1, R2
            },
            // 14
            new[]
            {
                R1, R1, R2,
                R1,     R1,
                R2, R1, R2
            },
            // 15
            new[]
            {
                R2, R1, R2,
                R1,     R1,
                R2, R1, R2
            },

            // 16
            new[]
            {
                R0, R1, R1,
                R2,     R1,
                R0, R1, R1
            },
            // 17
            new[]
            {
                R0, R1, R2,
                R2,     R1,
                R0, R1, R1
            },
            // 18
            new[]
            {
                R0, R1, R1,
                R2,     R1,
                R0, R1, R2
            },
            // 19
            new[]
            {
                R0, R1, R2,
                R2,     R1,
                R0, R1, R2
            },
            // 20
            new[]
            {
                R0, R2, R0,
                R1,     R1,
                R1, R1, R1
            },
            // 21
            new[]
            {
                R0, R2, R0,
                R1,     R1,
                R1, R1, R2
            },
            // 22
            new[]
            {
                R0, R2, R0,
                R1,     R1,
                R2, R1, R1
            },
            // 23
            new[]
            {
                R0, R2, R0,
                R1,     R1,
                R2, R1, R2
            },
            // 24
            new[]
            {
                R1, R1, R0,
                R1,     R2,
                R1, R1, R0
            },
            // 25
            new[]
            {
                R1, R1, R0,
                R1,     R2,
                R2, R1, R0
            },
            // 26
            new[]
            {
                R2, R1, R0,
                R1,     R2,
                R1, R1, R0
            },
            // 27
            new[]
            {
                R2, R1, R0,
                R1,     R2,
                R2, R1, R0
            },
            // 28
            new[]
            {
                R1, R1, R1,
                R1,     R1,
                R0, R2, R0
            },
            // 29
            new[]
            {
                R2, R1, R1,
                R1,     R1,
                R0, R2, R0
            },
            // 30
            new[]
            {
                R1, R1, R2,
                R1,     R1,
                R0, R2, R0
            },
            // 31
            new[]
            {
                R2, R1, R2,
                R1,     R1,
                R0, R2, R0
            },

            // 32
            new[]
            {
                R0, R1, R0,
                R2,     R2,
                R0, R1, R0
            },
            // 33
            new[]
            {
                R0, R2, R0,
                R1,     R1,
                R0, R2, R0
            },

            // 34
            new[]
            {
                R0, R2, R0,
                R2,     R1,
                R0, R1, R1
            },
            // 35
            new[]
            {
                R0, R2, R0,
                R2,     R1,
                R0, R1, R2
            },
            // 36
            new[]
            {
                R0, R2, R0,
                R1,     R2,
                R1, R1, R0
            },
            // 37
            new[]
            {
                R0, R2, R0,
                R1,     R2,
                R2, R1, R0
            },
            // 38
            new[]
            {
                R1, R1, R0,
                R1,     R2,
                R0, R2, R0
            },
            // 39
            new[]
            {
                R2, R1, R0,
                R1,     R2,
                R0, R2, R0
            },
            // 40
            new[]
            {
                R0, R1, R1,
                R2,     R1,
                R0, R2, R0
            },
            // 41
            new[]
            {
                R0, R1, R2,
                R2,     R1,
                R0, R2, R0
            },

            // 42
            new[]
            {
                R0, R2, R0,
                R2,     R2,
                R0, R1, R0
            },
            // 43
            new[]
            {
                R0, R2, R0,
                R2,     R1,
                R0, R2, R0
            },
            // 44
            new[]
            {
                R0, R1, R0,
                R2,     R2,
                R0, R2, R0
            },
            // 45
            new[]
            {
                R0, R2, R0,
                R1,     R2,
                R0, R2, R0
            },
            // 46
            new[]
            {
                R0, R2, R0,
                R2,     R2,
                R0, R2, R0
            }
        };
        #endregion

        private void FillAutoTileTexture(Texture2DRegion org, Texture2DRegion to, Size cell_size, EAutoTileType autotile_type)
        {
            for (var i = 0; i < ms_XpAutoTileMap.Count; ++i) {
                var map = ms_XpAutoTileMap[i];
                if (autotile_type == EAutoTileType.RPG_MAKER_MV) {
                    map = map.Select(idx => ms_MvXpRemap[idx]).ToArray();
                }

                CopyRegion(org, map, to, i, cell_size);
            }
        }

        private void FillAutoTileAnimations(Texture2DRegion org, Texture2DRegion to, Size cell_size, int frame_count, EAutoTileType autotile_type)
        {
            // animation frames are horizontal layout
            var org_frame_cell_size = new Size((int)org.rect.width / frame_count, (int)org.rect.height);
            var to_frame_cell_size = new Size((int)to.rect.width / frame_count, (int)to.rect.height);
            for (var i = 0; i < frame_count; ++i) {
                FillAutoTileTexture(
                    org.SubRegionFromIdx(i, org_frame_cell_size), 
                    to.SubRegionFromIdx(i, to_frame_cell_size), 
                    cell_size, autotile_type);
            }
        }

        private bool IsValid()
        {
            if (m_Type == EAutoTileType.RPG_MAKER_XP) {
                var width = m_OrgTexture.width / m_AnimationFrames;
                var height = m_OrgTexture.height;
                if (width / 3 != height / 4) {
                    return false;
                }
            }
            else if (m_Type == EAutoTileType.RPG_MAKER_MV) {
                var width = m_OrgTexture.width / m_AnimationFrames;
                var height = m_OrgTexture.height;
                if (width / 2 != height / 3) {
                    return false;
                }
            }

            return true;
        }

        private void MakeAutoTile_A2(string img_path)
        {
            var txt_path = Path.Combine(Path.GetDirectoryName(img_path), Path.GetFileNameWithoutExtension(img_path) + ".txt");
            var names = File.ReadAllLines(txt_path).Select(s => s.Split('|')[0]).ToArray();

            var rows = 4;
            var cols = 8;
            var cell_size = new Size(48);

            var org_tex = new Texture2D(2, 2);
            org_tex.LoadImage(File.ReadAllBytes(img_path));

            var org_tex_region = new Texture2DRegion(org_tex);
            var new_tex_region = new Texture2DRegion(cell_size.Scale(8 * cols, 6 * rows));

            for (var i = 0; i < rows * cols; ++i) {
                FillAutoTileTexture(org_tex_region.SubRegionFromIdx(i, cell_size.Scale(2, 3)),
                    new_tex_region.SubRegionFromIdx(i, cell_size.Scale(8, 6)), cell_size, EAutoTileType.RPG_MAKER_MV);
            }

            // save the reordered texture and reload it
            var base_path = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder("Assets", Path.GetFileNameWithoutExtension(img_path)));

            var path = Path.Combine(base_path, Path.GetFileNameWithoutExtension(img_path) + "_Split.png");
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            File.WriteAllBytes(path, new_tex_region.texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            var sheet = new SpriteMetaData[rows * cols * ms_XpAutoTileMap.Count];

            for (var i = 0; i < rows * cols; ++i) {
                var sub_rect = new_tex_region.rect.SubRectFromIdx(i, cell_size.Scale(8, 6));
                for (var j = 0; j < ms_XpAutoTileMap.Count; ++j) {
                    var meta = new SpriteMetaData
                    {
                        name = $"{names[i]}_{j}",
                        rect = sub_rect.SubRectFromIdx(j, cell_size),
                        border = Vector4.zero,
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new(0.5f, 0.5f)
                    };
                    sheet[i * ms_XpAutoTileMap.Count + j] = meta;
                }
            }

            // reimport the texture to generate all sprites
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            imp!.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = cell_size.width;
            imp.spriteImportMode = SpriteImportMode.Multiple;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Point;
            imp.spritesheet = sheet;
            imp.SaveAndReimport();

            // get all the sprites
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprs = new List<Sprite>();
            foreach (var obj in objs) {
                if (obj is Sprite) {
                    sprs.Add(obj as Sprite);
                }
            }

            // generate the auto tile assets
            var tiles = new List<AutoTile>();
            for (var i = 0; i < rows * cols; ++i) {
                var auto_tile = CreateInstance<AutoTile>();

                // use the last sprite as the default, to show in the palette.
                auto_tile.m_DefaultSprite = sprs.GetSpriteByName($"{names[i]}_{47}");

                auto_tile.m_TilingRules = new(ms_XpRuleMap.Count);
                for (var j = 0; j < ms_XpRuleMap.Count; ++j) {
                    var rule = new AutoTile.TilingRule {
                        m_Neighbors = ms_XpRuleMap[j],
                        m_Sprites = new[] { sprs.GetSpriteByName($"{names[i]}_{j}") },
                        m_Output = AutoTile.TilingRule.OutputSprite.Single
                    };
                    auto_tile.m_TilingRules.Add(rule);
                }

                var auto_tile_path = Path.Combine(base_path, names[i] + "_RuleTile.asset");
                auto_tile_path = AssetDatabase.GenerateUniqueAssetPath(auto_tile_path);

                AssetDatabase.CreateAsset(auto_tile, auto_tile_path);
                tiles.Add(auto_tile);
            }

            var palette = Utility.CreateNewPalette(base_path, Path.GetFileNameWithoutExtension(img_path));
            for (var i = 0; i < rows * cols; ++i) {
                var x = i % cols;
                var y = Mathf.FloorToInt(i / (float)cols);
                palette.SetTile(new(x, y, 0), tiles[i]);
                EditorUtility.SetDirty(tiles[i]);
            }
        }

        private void OnGUI()
        {
            m_OrgTexture = EditorGUILayout.ObjectField(m_OrgTexture, typeof(Texture2D), false) as Texture2D;
            m_AnimationFrames = EditorGUILayout.IntField("Animation Frames", m_AnimationFrames);
            m_AnimationSpeed = EditorGUILayout.FloatField("Animation Speed", m_AnimationSpeed);
            m_Type = (EAutoTileType)EditorGUILayout.EnumPopup("AutoTile Type", m_Type);

            if (m_OrgTexture != null) {
                // pre-check texture size is valid!
                if (!IsValid()) {
                    EditorGUILayout.HelpBox("Texture size is not match!", MessageType.Error);
                    GUI.enabled = false;
                }

                if (GUILayout.Button("MakeAutoTile")) {
                    TextureImporter imp;
                    var org_path = AssetDatabase.GetAssetPath(m_OrgTexture);

                    // make sure the texture is readable!
                    if (!m_OrgTexture.isReadable) {
                        imp = AssetImporter.GetAtPath(org_path) as TextureImporter;
                        imp!.isReadable = true;
                        imp.SaveAndReimport();
                    }

                    Size cell_size;
                    if (m_Type == EAutoTileType.RPG_MAKER_XP) {
                        cell_size = new(32);
                    }
                    else {
                        cell_size = new(48);
                    }

                    var org_tex_region = new Texture2DRegion(m_OrgTexture);
                    var new_tex_region = new Texture2DRegion(cell_size.Scale(8 * m_AnimationFrames, 6));
                    FillAutoTileAnimations(org_tex_region, new_tex_region, cell_size, m_AnimationFrames, m_Type);

                    // save the reordered texture and reload it
                    var path = Path.Combine(Path.GetDirectoryName(org_path), Path.GetFileNameWithoutExtension(org_path) + "_Split.png");
                    path = AssetDatabase.GenerateUniqueAssetPath(path);
                    File.WriteAllBytes(path, new_tex_region.texture.EncodeToPNG());
                    AssetDatabase.ImportAsset(path);

                    // calc all sprite meta data
                    var sheet = new SpriteMetaData[ms_XpAutoTileMap.Count * m_AnimationFrames];
                    var frame_cell_size = new Size((int)new_tex_region.rect.width / m_AnimationFrames, (int)new_tex_region.rect.height);

                    for (var i = 0; i < m_AnimationFrames; ++i) {
                        var sub_rect = new_tex_region.rect.SubRectFromIdx(i, frame_cell_size);
                        for (var j = 0; j < ms_XpAutoTileMap.Count; ++j) {
                            var meta = new SpriteMetaData
                            {
                                name = $"tile_{i}_{j}",
                                rect = sub_rect.SubRectFromIdx(j, cell_size),
                                border = Vector4.zero,
                                alignment = (int)SpriteAlignment.Center,
                                pivot = new(0.5f, 0.5f)
                            };
                            sheet[i * ms_XpAutoTileMap.Count + j] = meta;
                        }
                    }

                    // reimport the texture to generate all sprites
                    imp = AssetImporter.GetAtPath(path) as TextureImporter;
                    imp!.textureType = TextureImporterType.Sprite;
                    imp.spritePixelsPerUnit = cell_size.width;
                    imp.spriteImportMode = SpriteImportMode.Multiple;
                    imp.mipmapEnabled = false;
                    imp.filterMode = FilterMode.Point;
                    imp.spritesheet = sheet;
                    imp.SaveAndReimport();

                    // get all the sprites
                    var objs = AssetDatabase.LoadAllAssetsAtPath(path);
                    var spr_list = new List<Sprite>();
                    foreach (var obj in objs) {
                        if (obj is Sprite sprite) {
                            spr_list.Add(sprite);
                        }
                    }

                    // generate the auto tile asset
                    var auto_tile = CreateInstance<AutoTile>();

                    // use the last sprite as the default, to show in the palette.
                    auto_tile.m_DefaultSprite = spr_list.GetSpriteByIdx(0, 47);

                    auto_tile.m_TilingRules = new(ms_XpRuleMap.Count);
                    for (var i = 0; i < ms_XpRuleMap.Count; ++i) {
                        var rule = new AutoTile.TilingRule {
                            m_Neighbors = ms_XpRuleMap[i],
                            m_Sprites = spr_list.GetSpritesByIdx(m_AnimationFrames, i)
                        };

                        if (m_AnimationFrames > 1) {
                            rule.m_AnimationSpeed = m_AnimationSpeed;
                            rule.m_Output = AutoTile.TilingRule.OutputSprite.Animation;
                        }
                        else {
                            rule.m_Output = AutoTile.TilingRule.OutputSprite.Single;
                        }
                        auto_tile.m_TilingRules.Add(rule);
                    }

                    var auto_tile_path = Path.Combine(Path.GetDirectoryName(org_path), Path.GetFileNameWithoutExtension(org_path) + "_RuleTile.asset");
                    auto_tile_path = AssetDatabase.GenerateUniqueAssetPath(auto_tile_path);

                    AssetDatabase.CreateAsset(auto_tile, auto_tile_path);
                }

                GUI.enabled = true;

                //if (GUILayout.Button("Test A2")) {
                //    MakeAutoTile_A2(@"E:\RPGMakerMV\Project1\img\tilesets\Inside_A2.png");
                //}
            }
        }
    }
}