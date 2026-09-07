using UnityEngine;

namespace Watermelon
{
    public class TreasureHuntSettings
    {
        public Sprite FieldSprite;

        public Sprite BuriedSprite;
        public Sprite DugSprite;

        public Rect GridRect;
        public float CellScale;
        public float PrizeScale;

        public TreasureDistanceMode DistanceMode;

        public TreasureHintBand[] HintBands;

        public TreasureHuntDifficulty[] Difficulties;
    }
}
