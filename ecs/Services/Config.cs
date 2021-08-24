using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;

namespace ecs
{
    internal class Config
    {
        public float DeltaTime = 0f;
        public float Time = 0;

        public const string EventsWorldName = "events";

        public int Ids = 10;

        public GameConfig GameConfig = null;

        public const float TimeMove = 1f;
        public const float TimeReAction = 0.1f;

        public const float SqrDeltaLen = 2f;

        public readonly FilterConfig FilterConfig = new FilterConfig();
        public EcsWorld WorldBullet;
        public EcsWorld WorldEffect;
        public EcsWorld WorldEvents;
        public EcsWorld WorldGroup;
        public EcsWorld WorldDefault;
        public EcsWorld WorldUI;

        public GameObject bulletPool;


        // public EcsWorld DefaultWorld;
        public const int StoneId = -3;
        public EcsPackedEntity EmptyEcsPackEntity;
        public GameObject Camera;
        public EcsWorld WorldPhysics;
        public const int TeamId = 4;
        public int Coin = 0;

        public int DamageFactor = 0;
        public int ArmorFactor = 0;
        public int Spell = 0;

        public SpellsData Spells;
        public int SummonsCount = 0;
        public int MainSummonsCount;

        public int GetDamageFactor(int teamId)
        {
            if (teamId == TeamId) return DamageFactor;
            return 0;
        }

        public int GetArmorFactor(int teamId)
        {
            if (teamId == TeamId) return ArmorFactor;
            return 0;
        }
    }

    internal class FilterConfig
    {
        public EcsWorld World = null;
        public EcsPool<BaseUnitComponent> BaseUnitFilter;
        public EcsPool<DeadComponent> DeadPool;
        public EcsPool<HpComponent> HpPool;
        public EcsPool<StoneComponent> StonePool;
        public EcsPool<UnitComponent> UnitPool;

        public void Init(EcsWorld getWorld)
        {
            World = getWorld;
            BaseUnitFilter = World.GetPool<BaseUnitComponent>();
            DeadPool = World.GetPool<DeadComponent>();
            HpPool = World.GetPool<HpComponent>();
            StonePool = World.GetPool<StoneComponent>();
            UnitPool = World.GetPool<UnitComponent>();
        }
    }
}