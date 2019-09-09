using UnityEngine;
using UnityEngine.Assertions;

namespace RPGMakerAssetImporter
{
    internal class Texture2DRegion
    {
        public Texture2D texture;
        public Rect rect;

        public Texture2DRegion(Texture2D tex, Rect r)
        {
            texture = tex;
            rect = r;
        }

        public Texture2DRegion(Texture2D tex)
        {
            texture = tex;
            rect = new Rect(0f, 0f, tex.width, tex.height);
        }

        public Texture2DRegion(Size size)
        {
            texture = new Texture2D(size.width, size.height);
            rect = new Rect(0f, 0f, size.width, size.height);
        }

        public Texture2DRegion SubRegionFromIdx(int idx, Size cell_size)
        {
            return new Texture2DRegion(texture, rect.SubRectFromIdx(idx, cell_size));
        }

        public void CopyTo(Texture2DRegion to)
        {
            Assert.AreEqual(rect.size, to.rect.size);
            for (var x = 0; x < rect.width; ++x) {
                for (var y = 0; y < rect.height; ++y) {
                    to.texture.SetPixel((int)to.rect.x + x, (int)to.rect.y + y,
                        texture.GetPixel((int)rect.x + x, (int)rect.y + y));
                }
            }
        }
    }
}
