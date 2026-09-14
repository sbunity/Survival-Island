using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public interface IUpgradePanel
    {
        GameObject UpgradeUIPrefab { get; }
        Transform ContentTransform { get; }

        VerticalLayoutGroup ContentLayoutGroup { get; }

        bool ShowAllAfterUpgrade { get; set;  }
        Color DefaultColor { get; }
        Color HighlightedColor { get; }
    }
}
