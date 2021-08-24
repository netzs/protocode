using System.Collections.Generic;
using UnityEngine;
using Voody.UniLeo.Lite;

public class SpellsData : MonoBehaviour
{
    public IConvertToEntity[] Convert;
    public Component[] components;

    private void Awake()
    {
        var a = new List<IConvertToEntity>();
        foreach (var item in components)
        {
            if (item is IConvertToEntity ci)
            {
                a.Add(ci);
            }
        }

        Convert = a.ToArray();
    }
}