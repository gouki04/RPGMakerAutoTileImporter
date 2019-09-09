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
        protected enum EAutoTileType
        {
            RPG_MAKER_XP,
            RPG_MAKER_MV,
        }

        protected Texture2D m_OrgTexture;
        protected EAutoTileType m_Type;
        protected int m_AnimationFrames = 1;
        protected float m_AnimationSpeed = 1f;

        [MenuItem("Window/AutoTile Editor")]
        public static AutoTileEditor Create()
        {
            var window = GetWindow<AutoTileEditor>();
            window.titleContent = new GUIContent("AutoTile Editor");

            return window;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="from_idxs">[lt, rt, lb, rb]</param>
        /// <param name="to"></param>
        /// <param name="to_idx"></param>
        /// <param name="cell_size"></param>
        private void CopyRegion(Texture2DRegion from, int[] from_idxs, Texture2DRegion to, int to_idx, Size cell_size)
        {
            Assert.IsTrue(from_idxs.Length == 4);
            var half_cell_size = cell_size.Half();
            to = to.SubRegionFromIdx(to_idx, cell_size);

            for (var i = 0; i < from_idxs.Length; ++i) {
                from.SubRegionFromIdx(from_idxs[i], half_cell_size).CopyTo(to.SubRegionFromIdx(i, half_cell_size));
            }
        }

        #region RPG_MAKER_AUTOTILE_CFG
        private static readonly List<int[]> XP_AUTOTILE_MAP = new List<int[]>
        {
            new int[] {26,27,32,33},
            new int[] {4,27,32,33},
            new int[] {26,5,32,33},
            new int[] {4,5,32,33},
            new int[] {26,27,32,11},
            new int[] {4,27,32,11},
            new int[] {26,5,32,11},
            new int[] {4,5,32,11},
            new int[] {26,27,10,33},
            new int[] {4,27,10,33},
            new int[] {26,5,10,33},
            new int[] {4,5,10,33},
            new int[] {26,27,10,11},
            new int[] {4,27,10,11},
            new int[] {26,5,10,11},
            new int[] {4,5,10,11},
            new int[] {24,25,30,31},
            new int[] {24,5,30,31},
            new int[] {24,25,30,11},
            new int[] {24,5,30,11},
            new int[] {14,15,20,21},
            new int[] {14,15,20,11},
            new int[] {14,15,10,21},
            new int[] {14,15,10,11},
            new int[] {28,29,34,35},
            new int[] {28,29,10,35},
            new int[] {4,29,34,35},
            new int[] {4,29,10,35},
            new int[] {26,27,44,45},
            new int[] {4,39,44,45},
            new int[] {38,5,44,45},
            new int[] {4,5,44,45},
            new int[] {24,29,30,35},
            new int[] {14,15,44,45},
            new int[] {12,13,18,19},
            new int[] {12,13,18,11},
            new int[] {16,17,22,23},
            new int[] {16,17,10,23},
            new int[] {40,41,46,47},
            new int[] {4,41,46,47},
            new int[] {36,37,42,43},
            new int[] {36,5,42,43},
            new int[] {12,17,18,23},
            new int[] {12,13,42,43},
            new int[] {36,41,42,47},
            new int[] {16,17,46,47},
            new int[] {12,17,42,47},
            new int[] {12,17,42,47},
        };

        private static readonly List<int> MV_XP_REMAP = new List<int>()
        {
             0, 1, 0, 1, 2, 3,
             4, 5, 4, 5, 6, 7,
             8, 9, 9,10,10,11,
            12,13,13,14,14,15,
            12,13,13,14,14,15,
            16,17,17,18,18,19,
            16,17,17,18,18,19,
            20,21,21,22,22,23,
        };

        private static readonly int R0 = AutoTile.TilingRule.Neighbor.DontCare;
        private static readonly int R1 = AutoTile.TilingRule.Neighbor.This;
        private static readonly int R2 = AutoTile.TilingRule.Neighbor.NotThis;

        private static readonly List<int[]> XP_RULE_MAP = new List<int[]>()
        {
            // 0
            new int[]
            {
                R1, R1, R1,
                R1,     R1,
                R1, R1, R1,
            },
            // 1
            new int[]
            {
                R2, R1, R1,
                R1,     R1,
                R1, R1, R1,
            },
            // 2
            new int[]
            {
                R1, R1, R2,
                R1,     R1,
                R1, R1, R1,
            },
            // 3
            new int[]
            {
                R2, R1, R2,
                R1,     R1,
                R1, R1, R1,
            },
            // 4
            new int[]
            {
                R1, R1, R1,
                R1,     R1,
                R1, R1, R2,
            },
            // 5
            new int[]
            {
                R2, R1, R1,
                R1,     R1,
                R1, R1, R2,
            },
            // 6
            new int[]
            {
                R1, R1, R2,
                R1,     R1,
                R1, R1, R2,
            },
            // 7
            new int[]
            {
                R2, R1, R2,
                R1,     R1,
                R1, R1, R2,
            },
            // 8
            new int[]
            {
                R1, R1, R1,
                R1,     R1,
                R2, R1, R1,
            },
            // 9
            new int[]
            {
                R2, R1, R1,
                R1,     R1,
                R2, R1, R1,
            },
            // 10
            new int[]
            {
                R1, R1, R2,
                R1,     R1,
                R2, R1, R1,
            },
            // 11
            new int[]
            {
                R2, R1, R2,
                R1,     R1,
                R2, R1, R1,
            },
            // 12
            new int[]
            {
                R1, R1, R1,
                R1,     R1,
                R2, R1, R2,
            },
            // 13
            new int[]
            {
                R2, R1, R1,
                R1,     R1,
                R2, R1, R2,
            },
            // 14
            new int[]
            {
                R1, R1, R2,
                R1,     R1,
                R2, R1, R2,
            },
            // 15
            new int[]
            {
                R2, R1, R2,
                R1,     R1,
                R2, R1, R2,
            },

            // 16
            new int[]
            {
                R0, R1, R1,
                R2,     R1,
                R0, R1, R1,
            },
            // 17
            new int[]
            {
                R0, R1, R2,
                R2,     R1,
                R0, R1, R1,
            },
            // 18
            new int[]
            {
                R0, R1, R1,
                R2,     R1,
                R0, R1, R2,
            },
            // 19
            new int[]
            {
                R0, R1, R2,
                R2,     R1,
                R0, R1, R2,
            },
            // 20
            new int[]
            {
                R0, R2, R0,
                R1,     R1,
                R1, R1, R1,
            },
            // 21
            new int[]
            {
                R0, R2, R0,
                R1,     R1,
                R1, R1, R2,
            },
            // 22
            new int[]
            {
                R0, R2, R0,
                R1,     R1,
                R2, R1, R1,
            },
            // 23
            new int[]
            {
                R0, R2, R0,
                R1,     R1,
                R2, R1, R2,
            },
            // 24
            new int[]
            {
                R1, R1, R0,
                R1,     R2,
                R1, R1, R0,
            },
            // 25
            new int[]
            {
                R1, R1, R0,
                R1,     R2,
                R2, R1, R0,
            },
            // 26
            new int[]
            {
                R2, R1, R0,
                R1,     R2,
                R1, R1, R0,
            },
            // 27
            new int[]
            {
                R2, R1, R0,
                R1,     R2,
                R2, R1, R0,
            },
            // 28
            new int[]
            {
                R1, R1, R1,
                R1,     R1,
                R0, R2, R0,
            },
            // 29
            new int[]
            {
                R2, R1, R1,
                R1,     R1,
                R0, R2, R0,
            },
            // 30
            new int[]
            {
                R1, R1, R2,
                R1,     R1,
                R0, R2, R0,
            },
            // 31
            new int[]
            {
                R2, R1, R2,
                R1,     R1,
                R0, R2, R0,
            },

            // 32
            new int[]
            {
                R0, R1, R0,
                R2,     R2,
                R0, R1, R0,
            },
            // 33
            new int[]
            {
                R0, R2, R0,
                R1,     R1,
                R0, R2, R0,
            },

            // 34
            new int[]
            {
                R0, R2, R0,
                R2,     R1,
                R0, R1, R1,
            },
            // 35
            new int[]
            {
                R0, R2, R0,
                R2,     R1,
                R0, R1, R2,
            },
            // 36
            new int[]
            {
                R0, R2, R0,
                R1,     R2,
                R1, R1, R0,
            },
            // 37
            new int[]
            {
                R0, R2, R0,
                R1,     R2,
                R2, R1, R0,
            },
            // 38
            new int[]
            {
                R1, R1, R0,
                R1,     R2,
                R0, R2, R0,
            },
            // 39
            new int[]
            {
                R2, R1, R0,
                R1,     R2,
                R0, R2, R0,
            },
            // 40
            new int[]
            {
                R0, R1, R1,
                R2,     R1,
                R0, R2, R0,
            },
            // 41
            new int[]
            {
                R0, R1, R2,
                R2,     R1,
                R0, R2, R0,
            },

            // 42
            new int[]
            {
                R0, R2, R0,
                R2,     R2,
                R0, R1, R0,
            },
            // 43
            new int[]
            {
                R0, R2, R0,
                R2,     R1,
                R0, R2, R0,
            },
            // 44
            new int[]
            {
                R0, R1, R0,
                R2,     R2,
                R0, R2, R0,
            },
            // 45
            new int[]
            {
                R0, R2, R0,
                R1,     R2,
                R0, R2, R0,
            },
            // 46
            new int[]
            {
                R0, R2, R0,
                R2,     R2,
                R0, R2, R0,
            },
        };
        #endregion

        private void FillAutoTileTexture(Texture2DRegion org, Texture2DRegion to, Size cell_size, EAutoTileType autotile_type)
        {
            for (var i = 0; i < XP_AUTOTILE_MAP.Count; ++i) {
                var map = XP_AUTOTILE_MAP[i];
                if (autotile_type == EAutoTileType.RPG_MAKER_MV) {
                    map = map.Select(idx => MV_XP_REMAP[idx]).ToArray();
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

            var sheet = new SpriteMetaData[rows * cols * XP_AUTOTILE_MAP.Count];

            for (var i = 0; i < rows * cols; ++i) {
                var sub_rect = new_tex_region.rect.SubRectFromIdx(i, cell_size.Scale(8, 6));
                for (var j = 0; j < XP_AUTOTILE_MAP.Count; ++j) {
                    var meta = new SpriteMetaData
                    {
                        name = string.Format("{0}_{1}", names[i], j),
                        rect = sub_rect.SubRectFromIdx(j, cell_size),
                        border = Vector4.zero,
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    };
                    sheet[i * XP_AUTOTILE_MAP.Count + j] = meta;
                }
            }

            // reimport the texture to generate all sprites
            var imp = TextureImporter.GetAtPath(path) as TextureImporter;
            imp.textureType = TextureImporterType.Sprite;
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
                var auto_tile = ScriptableObject.CreateInstance<AutoTile>();

                // use the last sprite as the default, to show in the palette.
                auto_tile.m_DefaultSprite = sprs.GetSpriteByName(string.Format("{0}_{1}", names[i], 47));

                auto_tile.m_TilingRules = new List<AutoTile.TilingRule>(XP_RULE_MAP.Count);
                for (var j = 0; j < XP_RULE_MAP.Count; ++j) {
                    var rule = new AutoTile.TilingRule();
                    rule.m_Neighbors = XP_RULE_MAP[j];
                    rule.m_Sprites = new Sprite[] { sprs.GetSpriteByName(string.Format("{0}_{1}", names[i], j)) };
                    rule.m_Output = AutoTile.TilingRule.OutputSprite.Single;
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
                var y = Mathf.FloorToInt(i / cols);
                palette.SetTile(new Vector3Int(x, y, 0), tiles[i]);
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
                // precheck texture size is valid!
                if (!IsValid()) {
                    EditorGUILayout.HelpBox("Texture size is not match!", MessageType.Error);
                    GUI.enabled = false;
                }

                if (GUILayout.Button("MakeAutoTile")) {
                    TextureImporter imp;
                    var org_path = AssetDatabase.GetAssetPath(m_OrgTexture);

                    // make sure the texture is readable!
                    if (!m_OrgTexture.isReadable) {
                        imp = TextureImporter.GetAtPath(org_path) as TextureImporter;
                        imp.isReadable = true;
                        imp.SaveAndReimport();
                    }

                    Size cell_size;
                    if (m_Type == EAutoTileType.RPG_MAKER_XP) {
                        cell_size = new Size(32);
                    }
                    else {
                        cell_size = new Size(48);
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
                    var sheet = new SpriteMetaData[XP_AUTOTILE_MAP.Count * m_AnimationFrames];
                    var frame_cell_size = new Size((int)new_tex_region.rect.width / m_AnimationFrames, (int)new_tex_region.rect.height);

                    for (var i = 0; i < m_AnimationFrames; ++i) {
                        var sub_rect = new_tex_region.rect.SubRectFromIdx(i, frame_cell_size);
                        for (var j = 0; j < XP_AUTOTILE_MAP.Count; ++j) {
                            var meta = new SpriteMetaData
                            {
                                name = string.Format("tile_{0}_{1}", i, j),
                                rect = sub_rect.SubRectFromIdx(j, cell_size),
                                border = Vector4.zero,
                                alignment = (int)SpriteAlignment.Center,
                                pivot = new Vector2(0.5f, 0.5f)
                            };
                            sheet[i * XP_AUTOTILE_MAP.Count + j] = meta;
                        }
                    }

                    // reimport the texture to generate all sprites
                    imp = TextureImporter.GetAtPath(path) as TextureImporter;
                    imp.textureType = TextureImporterType.Sprite;
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

                    // generate the auto tile asset
                    var auto_tile = ScriptableObject.CreateInstance<AutoTile>();

                    // use the last sprite as the default, to show in the palette.
                    auto_tile.m_DefaultSprite = sprs.GetSpriteByIdx(0, 47);

                    auto_tile.m_TilingRules = new List<AutoTile.TilingRule>(XP_RULE_MAP.Count);
                    for (var i = 0; i < XP_RULE_MAP.Count; ++i) {
                        var rule = new AutoTile.TilingRule();
                        rule.m_Neighbors = XP_RULE_MAP[i];
                        rule.m_Sprites = sprs.GetSpritesByIdx(m_AnimationFrames, i);
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