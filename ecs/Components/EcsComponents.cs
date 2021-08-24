using System;
using System.Linq;
using Leopotam.EcsLite;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ecs.Components
{
    [Serializable]
    public struct BaseUnitComponent
    {
        public int teamId;
        public Vector3 Pos => cur.transform.position;
        public Transform cur;
        public AnimatorCallback animatorCallback;
        public Transform weaponPoint;
    }

    [Serializable]
    public struct AnimatorComponent
    {
        [HideInInspector] public bool isNeedUpdate;
        [HideInInspector] public bool attackTrigger;
        [HideInInspector] public bool runTrigger;
        [HideInInspector] public float timeAttack;
        public int weapon;
        [HideInInspector] public bool isAlreadyRun;
        [HideInInspector] public int oldWeapon;
        [HideInInspector] public bool deathTrigger;

        public Animator animator;
        [HideInInspector] public bool idleTrigger;
        [HideInInspector] public bool specialStartTrigger;
        [HideInInspector] public bool specialEndTrigger;
    }


    public struct StoneComponent
    {
    }

    public struct UnitComponent
    {
    }

    [Serializable]
    public struct HpComponent
    {
        public int value;
        public int maxValue;
        public TextMeshPro text;
        public bool isNeedUpdate;
    }


    public struct ResurrectComponent
    {
    }

    [Serializable]
    public struct HpsComponent
    {
        public int count;
        public float delay;

        [HideInInspector] public float time;
    }

    public struct SelfResurrectComponent
    {
        public float Timer;
        public bool IsStart;
        public Vector3 StartPos;
    }

    [Serializable]
    public struct MoveConfigComponent
    {
        public NavMeshAgent agent;
    }

    [Serializable]
    public struct TeamComponent
    {
        public MeshRenderer mesh;
        public ParticleSystemRenderer particle;
    }

    [Serializable]
    public struct SimpleAttackComponent : IUnitAction
    {
        public int damage;
        [SerializeField] private UnitAction unitAction;
        public UnitAction UnitAction => unitAction;
    }

    [Serializable]
    public struct SimpleAttackStoneComponent : IUnitAction
    {
        [SerializeField] private UnitAction unitAction;
        public UnitAction UnitAction => unitAction;
    }

    [Serializable]
    public struct MeleeSplashAttackComponent : IUnitAction
    {
        [SerializeField] public float radius;
        [SerializeField] private UnitAction unitAction;
        public int damage;
        public UnitAction UnitAction => unitAction;
    }

    [Serializable]
    public struct HealerActionComponent : IUnitAction
    {
        [SerializeField] private UnitAction unitAction;
        public UnitAction UnitAction => unitAction;
        public int power;
        public GameObject fx;
    }

    [Serializable]
    public struct Special2HandAttackComponent : IUnitAction
    {
        [SerializeField] public float radius;
        [SerializeField] public float duration;

        [FormerlySerializedAs("periodDamage")] [SerializeField]
        public float periodDamageTime;

        [SerializeField] private UnitAction unitAction;
        public UnitAction UnitAction => unitAction;

        [FormerlySerializedAs("currentTime")] [HideInInspector]
        public float endTime;

        [HideInInspector] public float currentPeriod;
        public GameObject particle;
        public int damage;
    }

    [Serializable]
    public struct SimpleRangeAttackComponent : IUnitAction
    {
        [SerializeField] private UnitAction unitAction;
        public BulletSetting bulletSetting;
        public Transform bulletPoint;
        public UnitAction UnitAction => unitAction;
    }

    [Serializable]
    public struct ResurrectSpellComponent : IUnitAction
    {
        [SerializeField] private UnitAction unitAction;
        public UnitAction UnitAction => unitAction;

        [SerializeField] public GameObject fx;
    }

    [Serializable]
    public struct BulletSettingComponent
    {
        public float speed;
        public int damage;
        [HideInInspector] public Transform transform;
        [HideInInspector] public Vector3 pos;
        [HideInInspector] public EcsPackedEntity target;
        [HideInInspector] public EcsPackedEntity attacker;

        [HideInInspector] public int teamAttacker;
    }

    public struct UnitActionsComponent
    {
        public UnitAction[] Actions;
        public bool IsHasAction => Actions.Length > 0;

        public float GetMinDelay()
        {
            return Actions.Length == 0
                ? 0
                : Actions.Min(item => item.startTime + Math.Max(item.delay, item.actionTime));
        }
    }


    public interface IUnitAction
    {
        public UnitAction UnitAction { get; }
    }

    public enum TypeTarget
    {
        Enemy = 0,
        Self = 1,
        DeadAlly = 2,
        Stone = 3,
        NotFullHp = 4,
    }


    public struct DamageComponent
    {
        public EcsPackedEntity Target;
        public EcsPackedEntity Attacker;
        public int TeamAttacker;
        public int Damage;
    }

    public struct DeadComponent
    {
        public float Time;
        public bool IsGetDrop;
    }

    public struct DestroyComponent
    {
    }

    public struct ImpulseComponent
    {
        public Vector3 Pos;
        public float Factor;
    }

    public struct GiantComponent
    {
    }

    public struct AnimHitComponent
    {
    }

    [Serializable]
    public struct NeedInitComponent
    {
        public int teamId;
    }


    [Serializable]
    public struct SpawnComponent
    {
        public float time;
        [HideInInspector] public float currentTime;
        [SerializeField] public GameObject[] units;
        public EcsPackedEntity Group;
        public GameObject fxSummon;

        public GameObject collider;
        [HideInInspector] public bool isActive;
        [HideInInspector] public bool isNeedUpdate;
    }


    public struct DefaultMove
    {
        public Vector3 Pos;
    }

    public struct MainPlayerComponent
    {
        public bool IsActiveMove;
        public Vector3 Pos;
        public int Tick;
    }

    public struct CameraPlayerComponent
    {
    }

    [Serializable]
    public struct SpawnSlaveComponent : IUnitAction
    {
        [SerializeField] public GameObject[] units;

        [SerializeField] private UnitAction unitAction;
        public UnitAction UnitAction => unitAction;
        public EcsPackedEntity Group;

        [SerializeField] public float distance;
        [SerializeField] public GameObject fxSummon;
    }

    public struct UnitGroupDataComponent
    {
        public EcsPackedEntity Leader;
        public EcsPackedEntity[] Units;
        public int Count;
        public float Distance;
    }

    public struct UnitGroupComponent
    {
        public EcsPackedEntity Group;
    }

    // public sealed class MoveConfigComponentProvider : MonoProvider<MoveConfigComponent>
}