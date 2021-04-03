using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe
{
    public struct GraphicBundlePack
    {
        public string BundleName;
        public Sprite CrossSymbol;
        public Sprite CircleSymbol;
        public Sprite BackgroundGraphic;

        public GraphicBundlePack(string bundleName, Sprite crossSymbol, Sprite circleSymbol, Sprite backgroundGraphic)
        {
            this.BundleName = bundleName;
            this.CrossSymbol = crossSymbol;
            this.CircleSymbol = circleSymbol;
            this.BackgroundGraphic = backgroundGraphic;
        }
    }
}