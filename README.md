# RPGMakerAutoTileImporter
Import asset like auto tile in RPG Maker to unity, and paint with the tilemap system. This tool is really simple, but you can extend it as you like.

If I got time, I may try to extend this tool to be able to import some built in auto tile file in RPG Maker MV, like A1-A5, or character animations.

## support

1. RPG Maker XP Auto tile (single or animation)
2. RPG Maker MV Auto tile (single or animation)
3. Custom Auto tile like above two (eg. difference resolution)

## usage

1. Place the auto tile image under unity's assets folder.

2. Open the AutoTileEditor through menu `Windows/AutoTile Editor`.

3. Drag the image to the first slot, and set other parameters like below:

   | Animation Frames | 1 if it is a single auto tile, or the frame count of the auto tile animation. |
   | ---------------- | ------------------------------------------------------------ |
   | Animation Speed  | only useful when it is a auto tile animation, pass to [TileAnimationData.animationSpeed](https://docs.unity3d.com/ScriptReference/Tilemaps.TileAnimationData-animationSpeed.html), it means how many frames should play in one second. |
   | AutoTile Type    | RPG_MAKER_XP or RPG_MAKER_MV.                                |

4. If the parameters did not match the image you choose, it will show up a error box says "Texture size is not match!", and you should double check the parameters.

5. If everything ok, the `MakeAutoTile` button will show up, click it, it will generate the tile asset and the new texture next to the image file.

6. Drag the generated tile asset to the palette, and have fun.

## FAQ

1. If the auto tile image is inside other big image like atlas, should I extract it as one single image file?

   Yes!