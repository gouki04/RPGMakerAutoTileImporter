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

        private void CopyRegion(Texture2D from, Rect from_region, Texture2D to, Rect to_region)
        {
            Assert.AreEqual(from_region.size, to_region.size);
            for (var x = 0; x < from_region.width; ++x) {
                for (var y = 0; y < from_region.height; ++y) {
                    to.SetPixel((int)to_region.x + x, (int)to_region.y + y,
                        from.GetPixel((int)from_region.x + x, (int)from_region.y + y));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="idx">2d to 1d array index, from left top to bottom right.</param>
        /// <param name="cell_size"></param>
        /// <param name="frame">frame idx of animation</param>
        /// <returns></returns>
        private Rect IdxToRegion(Texture2D texture, int idx, int cell_size, int frame)
        {
            var rows = texture.height / cell_size;
            var cols = texture.width / m_AnimationFrames / cell_size;
            return new Rect(
                cell_size * (idx % cols) + (texture.width / m_AnimationFrames) * frame,
                cell_size * (rows - 1 - (idx / cols)),
                cell_size, cell_size);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="from_idxs">[lt, rt, lb, rb]</param>
        /// <param name="to"></param>
        /// <param name="to_idx"></param>
        /// <param name="cell_size"></param>
        /// <param name="frame">frame idx of animation</param>
        private void CopyRegion(Texture2D from, int[] from_idxs, Texture2D to, int to_idx, int cell_size, int frame)
        {
            Assert.IsTrue(from_idxs.Length == 4);
            var half_cell_size = cell_size / 2;
            var to_region = IdxToRegion(to, to_idx, cell_size, frame);

            Rect region;

            // left top
            region = new Rect(to_region.position + new Vector2(0, half_cell_size), to_region.size / 2);
            CopyRegion(from, IdxToRegion(from, from_idxs[0], half_cell_size, frame), to, region);

            // right top
            region = new Rect(to_region.position + new Vector2(half_cell_size, half_cell_size), to_region.size / 2);
            CopyRegion(from, IdxToRegion(from, from_idxs[1], half_cell_size, frame), to, region);

            // left bottom
            region = new Rect(to_region.position, to_region.size / 2);
            CopyRegion(from, IdxToRegion(from, from_idxs[2], half_cell_size, frame), to, region);

            // right bottom
            region = new Rect(to_region.position + new Vector2(half_cell_size, 0), to_region.size / 2);
            CopyRegion(from, IdxToRegion(from, from_idxs[3], half_cell_size, frame), to, region);
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

        private Sprite GetSpriteByName(List<Sprite> sprs, string name)
        {
            foreach (var spr in sprs) {
                if (spr != null && spr.name == name) {
                    return spr;
                }
            }
            return null;
        }

        private Sprite GetSpriteByIdx(List<Sprite> sprs, int frame, int idx)
        {
            return GetSpriteByName(sprs, string.Format("tile_{0}_{1}", frame, idx));
        }

        private Sprite[] GetSpritesByIdx(List<Sprite> sprs, int idx)
        {
            var ret = new Sprite[m_AnimationFrames];
            for (var i = 0; i < m_AnimationFrames; ++i) {
                ret[i] = GetSpriteByIdx(sprs, i, idx);
            }
            return ret;
        }

        private Texture2D GenerateReorderedTexture(int cell_size, int animation_frames)
        {
            var new_texture = new Texture2D(8 * cell_size * animation_frames, 6 * cell_size);
            for (var i = 0; i < animation_frames; ++i) {
                for (var j = 0; j < XP_AUTOTILE_MAP.Count; ++j) {
                    var map = XP_AUTOTILE_MAP[j];
                    if (m_Type == EAutoTileType.RPG_MAKER_MV) {
                        map = map.Select(idx => MV_XP_REMAP[idx]).ToArray();
                    }

                    CopyRegion(m_OrgTexture, map, new_texture, j, cell_size, i);
                }
            }
            return new_texture;
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

                    int cell_size;
                    if (m_Type == EAutoTileType.RPG_MAKER_XP) {
                        cell_size = m_OrgTexture.width / m_AnimationFrames / 3;
                    }
                    else {
                        cell_size = m_OrgTexture.width / m_AnimationFrames / 2;
                    }

                    var new_texture = GenerateReorderedTexture(cell_size, m_AnimationFrames);

                    // save the reordered texture and reload it
                    var path = Path.Combine(Path.GetDirectoryName(org_path), Path.GetFileNameWithoutExtension(org_path) + "_Split.png");
                    path = AssetDatabase.GenerateUniqueAssetPath(path);
                    File.WriteAllBytes(path, new_texture.EncodeToPNG());
                    AssetDatabase.ImportAsset(path);

                    // calc all sprite meta data
                    var sheet = new SpriteMetaData[XP_AUTOTILE_MAP.Count * m_AnimationFrames];
                    for (var i = 0; i < m_AnimationFrames; ++i) {
                        for (var j = 0; j < XP_AUTOTILE_MAP.Count; ++j) {
                            var meta = new SpriteMetaData
                            {
                                name = string.Format("tile_{0}_{1}", i, j),
                                rect = IdxToRegion(new_texture, j, cell_size, i),
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
                    imp.spritePixelsPerUnit = cell_size;
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

                    // generate the rule tile asset
                    var rule_tile = ScriptableObject.CreateInstance<AutoTile>();

                    // use the last sprite as the default, to show in the palette.
                    rule_tile.m_DefaultSprite = GetSpriteByIdx(sprs, 0, 47);

                    rule_tile.m_TilingRules = new List<AutoTile.TilingRule>(XP_RULE_MAP.Count);
                    for (var i = 0; i < XP_RULE_MAP.Count; ++i) {
                        var rule = new AutoTile.TilingRule();
                        rule.m_Neighbors = XP_RULE_MAP[i];
                        rule.m_Sprites = GetSpritesByIdx(sprs, i);
                        if (m_AnimationFrames > 1) {
                            rule.m_AnimationSpeed = m_AnimationSpeed;
                            rule.m_Output = AutoTile.TilingRule.OutputSprite.Animation;
                        }
                        else {
                            rule.m_Output = AutoTile.TilingRule.OutputSprite.Single;
                        }
                        rule_tile.m_TilingRules.Add(rule);
                    }

                    var rule_tile_path = Path.Combine(Path.GetDirectoryName(org_path), Path.GetFileNameWithoutExtension(org_path) + "_RuleTile.asset");
                    rule_tile_path = AssetDatabase.GenerateUniqueAssetPath(rule_tile_path);

                    AssetDatabase.CreateAsset(rule_tile, rule_tile_path);
                }

                GUI.enabled = true;
            }
        }
    }
}