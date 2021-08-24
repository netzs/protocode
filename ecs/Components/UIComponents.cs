using System;
using TMPro;

namespace ecs.Components
{
    [Serializable]
    public struct CoinPanelComponent
    {
        public TextMeshProUGUI text;
    }

    // public sealed class CoinPanelComponentProvider : MonoProvider<CoinPanelComponent> {}
}