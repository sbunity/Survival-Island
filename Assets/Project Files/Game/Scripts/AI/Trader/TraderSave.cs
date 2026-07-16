using System.Collections.Generic;

namespace Watermelon
{
    [System.Serializable]
    public class TraderSave : ISaveObject
    {
        public bool IsRescued;
        public int Phase;
        public float TimeUntilArrival;
        public float VisitTimeLeft;
        public List<int> ActiveOfferIndices = new List<int>();
        public List<int> OfferRemaining = new List<int>();

        public void OnBeforeSave() { }
    }
}
