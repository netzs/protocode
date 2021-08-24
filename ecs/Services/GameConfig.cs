using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ecs
{
    [CreateAssetMenu(fileName = "GameData", menuName = "GameData")]
    internal class GameConfig : ScriptableObject
    {
        public int unitCount = 10;
        public int lineCount = 5;
        public GameObject simpleUnit;
        public GameObject unitSword;
        public GameObject unitSwin;
        public GameObject unitBow;
        public GameObject sphere;

        public GameObject[] units;

        public Material[] teams;
        public float waitTime = 1f;

        public bool isSwin;

        private int sizeX = 100;
        private int sizeY = 100;
        private int roomCount = 10;
        private int roomMin = 10;
        private int roomMax = 20;

        private Dictionary<string, Stack<GameObject>> _bulletPool =
            new Dictionary<string, Stack<GameObject>>();

        public GameObject stone;

        private int[] level;
        public GameObject[] unitsSummon;
        [SerializeField] public GameObject mainSummon;

        public GameObject fxLvlUp;

        public void Generate()
        {
            level = new int[sizeX * sizeY];

            // for (var i = 0; i < roomCount; i++)
            // {
            //     var sx = Random.Range(roomMin, roomMax);
            //     var sy = Random.Range(roomMin, roomMax);
            //     var px = Random.Range(1, sizeX - sx);
            //     var py = Random.Range(1, sizeY - sy);
            //     for (var xx = px; xx < sx + px; xx++)
            //     {
            //         for (var yy = py; yy < sy + py; yy++)
            //         {
            //             level[GetIndex(xx, yy)] = i + 1;
            //         }
            //     }
            // }

        }

        private int GetIndex(int x, int y)
        {
            return y * sizeX + x;
        }


        public GameObject CreateBullet(GameObject prefab, GameObject parent)
        {
            if (_bulletPool.TryGetValue(prefab.name, out var lst) && lst.Count > 0)
            {
                return lst.Pop();
            }

            var pr = Instantiate(prefab, parent.transform);
            pr.name = prefab.name;
            return pr;
        }

        public void ReleaseBullet(GameObject bullet)
        {
            if (_bulletPool.TryGetValue(bullet.name, out var lst))
            {
                lst.Push(bullet);
            }
            else
            {
                _bulletPool.Add(bullet.name, new Stack<GameObject>(new[] {bullet}));
            }
        }
    }
}