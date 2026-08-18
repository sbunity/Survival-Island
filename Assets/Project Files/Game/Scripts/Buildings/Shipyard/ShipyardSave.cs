using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ShipyardSave : ISaveObject
    {
        [SerializeField] List<string> completedStageIds = new List<string>();
        public List<string> CompletedStageIds => completedStageIds ??= new List<string>();

        public void OnBeforeSave() { }
    }
}
