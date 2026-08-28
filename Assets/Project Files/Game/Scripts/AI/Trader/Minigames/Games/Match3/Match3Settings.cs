using UnityEngine;

namespace Watermelon
{
    public class Match3Settings
    {
        public Sprite FieldSprite;

        public int Columns;
        public int Rows;

        public Rect GridRect;
        public float TileScale;

        public CurrencyType[] TilePool;
        public int TileTypesPerGame;

        public DuoInt MovesRange;
    }
}
