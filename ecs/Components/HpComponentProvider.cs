using UnityEngine;
using Voody.UniLeo.Lite;

namespace ecs.Components
{
    [DisallowMultipleComponent]
    public sealed class HpComponentProvider : MonoProvider<HpComponent> { }
}