using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPGMakerAssetImporter
{
    /// <summary>
    /// RPG Maker Auto Tile
    /// 
    /// incase the user do not have the 2d-extra package, I duplicate the RuleTile
    /// from 2d-extra and rename it as AutoTile, and cleanup some code.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Auto Tile", menuName = "Tiles/Auto Tile")]
    public class AutoTile : TileBase
    {
        private static readonly int NeighborCount = 8;

        /// <summary>
        /// Returns the number of neighbors a Rule Tile can have.
        /// </summary>
        public virtual int neighborCount
        {
            get { return NeighborCount; }
        }

        /// <summary>
        /// The Default Sprite set when creating a new Rule.
        /// </summary>
        public Sprite m_DefaultSprite;
        /// <summary>
        /// The Default Collider Type set when creating a new Rule.
        /// </summary>
        public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Grid;

        /// <summary>
        /// A cache for the neighboring Tiles when matching Rules.
        /// </summary>
        protected TileBase[] m_CachedNeighboringTiles = new TileBase[NeighborCount];
        private Quaternion m_GameObjectQuaternion;

        /// <summary>
        /// The data structure holding the Rule information for matching Rule Tiles with
        /// its neighbors.
        /// </summary>
        [Serializable]
        public class TilingRule
        {
            /// <summary>
            /// The matching Rule conditions for each of its neighboring Tiles.
            /// </summary>
            public int[] m_Neighbors;
            /// <summary>
            /// The output Sprites for this Rule.
            /// </summary>
            public Sprite[] m_Sprites;
            /// <summary>
            /// The output Animation Speed for this Rule.
            /// </summary>
            public float m_AnimationSpeed;
            /// <summary>
            /// The output type for this Rule.
            /// </summary>
            public OutputSprite m_Output;
            /// <summary>
            /// The output Collider Type for this Rule.
            /// </summary>
            public Tile.ColliderType m_ColliderType;

            /// <summary>
            /// Constructor for Tiling Rule. This defaults to a Single Output.
            /// </summary>
            public TilingRule()
            {
                m_Output = OutputSprite.Single;
                m_Neighbors = new int[NeighborCount];
                m_Sprites = new Sprite[1];
                m_AnimationSpeed = 1f;
                m_ColliderType = Tile.ColliderType.Sprite;

                for (var i = 0; i < m_Neighbors.Length; i++)
                    m_Neighbors[i] = Neighbor.DontCare;
            }

            /// <summary>
            /// The enumeration for matching Neighbors when matching Rule Tiles
            /// </summary>
            public class Neighbor
            {
                /// <summary>
                /// The Rule Tile will not care about the contents of the cell in that direction
                /// </summary>
                public const int DontCare = 0;
                /// <summary>
                /// The Rule Tile will check if the contents of the cell in that direction is an instance of this Rule Tile.
                /// If not, the rule will fail.
                /// </summary>
                public const int This = 1;
                /// <summary>
                /// The Rule Tile will check if the contents of the cell in that direction is not an instance of this Rule Tile.
                /// If it is, the rule will fail.
                /// </summary>
                public const int NotThis = 2;
            }

            /// <summary>
            /// The Output for the Tile which fits this Rule.
            /// </summary>
            public enum OutputSprite
            {
                /// <summary>
                /// A Single Sprite will be output.
                /// </summary>
                Single,
                /// <summary>
                /// A Random Sprite will be output.
                /// </summary>
                Random,
                /// <summary>
                /// A Sprite Animation will be output.
                /// </summary>
                Animation
            }
        }

        /// <summary>
        /// A list of Tiling Rules for the Rule Tile.
        /// </summary>
        [HideInInspector] public List<TilingRule> m_TilingRules;

        /// <summary>
        /// StartUp is called on the first frame of the running Scene.
        /// </summary>
        /// <param name="location">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="instantiatedGameObject">The GameObject instantiated for the Tile.</param>
        /// <returns>Whether StartUp was successful</returns>
        public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject instantiatedGameObject)
        {
            if (instantiatedGameObject != null) {
                var tmpMap = tilemap.GetComponent<Tilemap>();
                instantiatedGameObject.transform.position = tmpMap.LocalToWorld(tmpMap.CellToLocalInterpolated(location + tmpMap.tileAnchor));
                instantiatedGameObject.transform.rotation = m_GameObjectQuaternion;
            }

            return true;
        }

        /// <summary>
        /// Retrieves any tile rendering data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileData">Data to render the tile.</param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            TileBase[] neighboringTiles = null;
            GetMatchingNeighboringTiles(tilemap, position, ref neighboringTiles);
            var iden = Matrix4x4.identity;

            tileData.sprite = m_DefaultSprite;
            tileData.colliderType = m_DefaultColliderType;
            tileData.flags = TileFlags.LockTransform;
            tileData.transform = iden;

            foreach (var rule in m_TilingRules) {
                var transform = iden;
                if (RuleMatches(rule, ref neighboringTiles, ref transform)) {
                    switch (rule.m_Output) {
                        case TilingRule.OutputSprite.Single:
                        case TilingRule.OutputSprite.Animation:
                            tileData.sprite = rule.m_Sprites[0];
                            break;
                    }
                    tileData.transform = transform;
                    tileData.colliderType = rule.m_ColliderType;

                    // Converts the tile's rotation matrix to a quaternion to be used by the instantiated Game Object
                    m_GameObjectQuaternion = Quaternion.LookRotation(new Vector3(transform.m02, transform.m12, transform.m22), new Vector3(transform.m01, transform.m11, transform.m21));
                    break;
                }
            }
        }

        /// Retrieves any tile animation data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileAnimationData">Data to run an animation on the tile.</param>
        /// <returns>Whether the call was successful.</returns>
        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            TileBase[] neighboringTiles = null;
            var iden = Matrix4x4.identity;
            foreach (var rule in m_TilingRules) {
                if (rule.m_Output == TilingRule.OutputSprite.Animation) {
                    var transform = iden;
                    GetMatchingNeighboringTiles(tilemap, position, ref neighboringTiles);
                    if (RuleMatches(rule, ref neighboringTiles, ref transform)) {
                        tileAnimationData.animatedSprites = rule.m_Sprites;
                        tileAnimationData.animationSpeed = rule.m_AnimationSpeed;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method is called when the tile is refreshed.
        /// </summary>
        /// <param name="location">Position of the Tile on the Tilemap.</param>
        /// <param name="tileMap">The Tilemap the tile is present on.</param>
        public override void RefreshTile(Vector3Int location, ITilemap tileMap)
        {
            if (m_TilingRules != null && m_TilingRules.Count > 0) {
                for (var y = -1; y <= 1; y++) {
                    for (var x = -1; x <= 1; x++) {
                        base.RefreshTile(location + new Vector3Int(x, y, 0), tileMap);
                    }
                }
            }
            else {
                base.RefreshTile(location, tileMap);
            }
        }

        /// <summary>
        /// Does a Rule Match given a Tiling Rule and neighboring Tiles.
        /// </summary>
        /// <param name="rule">The Tiling Rule to match with.</param>
        /// <param name="neighboringTiles">The neighboring Tiles to match with.</param>
        /// <param name="transform">A transform matrix which will match the Rule.</param>
        /// <returns>True if there is a match, False if not.</returns>
        protected virtual bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles, ref Matrix4x4 transform)
        {
            if (RuleMatches(rule, ref neighboringTiles)) {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if there is a match given the neighbor matching rule and a Tile.
        /// </summary>
        /// <param name="neighbor">Neighbor matching rule.</param>
        /// <param name="tile">Tile to match.</param>
        /// <returns>True if there is a match, False if not.</returns>
        public virtual bool RuleMatch(int neighbor, TileBase tile)
        {
            switch (neighbor) {
                case TilingRule.Neighbor.This: return tile == this;
                case TilingRule.Neighbor.NotThis: return tile != this;
            }
            return true;
        }

        /// <summary>
        /// Checks if there is a match given the neighbor matching rule and a Tile with a rotation angle.
        /// </summary>
        /// <param name="rule">Neighbor matching rule.</param>
        /// <param name="neighboringTiles">Tile to match.</param>
        /// <param name="angle">Rotation angle for matching.</param>
        /// <returns>True if there is a match, False if not.</returns>
        protected bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles)
        {
            for (var i = 0; i < neighborCount; ++i) {
                var tile = neighboringTiles[i];
                if (!RuleMatch(rule.m_Neighbors[i], tile)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets and caches the neighboring Tiles around the given Tile on the Tilemap.
        /// </summary>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="neighboringTiles">An array storing the neighboring Tiles</param>
        protected virtual void GetMatchingNeighboringTiles(ITilemap tilemap, Vector3Int position, ref TileBase[] neighboringTiles)
        {
            if (neighboringTiles != null)
                return;

            if (m_CachedNeighboringTiles == null || m_CachedNeighboringTiles.Length < neighborCount)
                m_CachedNeighboringTiles = new TileBase[neighborCount];

            var index = 0;
            for (var y = 1; y >= -1; y--) {
                for (var x = -1; x <= 1; x++) {
                    if (x != 0 || y != 0) {
                        var tilePosition = new Vector3Int(position.x + x, position.y + y, position.z);
                        m_CachedNeighboringTiles[index++] = tilemap.GetTile(tilePosition);
                    }
                }
            }
            neighboringTiles = m_CachedNeighboringTiles;
        }
    }
}
