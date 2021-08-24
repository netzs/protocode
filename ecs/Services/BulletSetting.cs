using ecs;
using Leopotam.EcsLite;
using UnityEngine;
using Voody.UniLeo.Lite;

public class BulletSetting : MonoBehaviour
{
    [SerializeField] public GameObject bulletPrefab;
    private IConvertToEntity[] _entities;

    public void Awake()
    {
        var convert = GetComponent<ConvertToEntity>();
        if (convert != null) convert.enabled = false;

        _entities = GetComponents<IConvertToEntity>();
    }

    public void SetEntityData(int entities, EcsWorld world)
    {
        foreach (var item in _entities)
        {
            item.Convert(entities, world);
        }
    }
}