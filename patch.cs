using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;

namespace GunProjectileSizePatch
{    // Declares our Mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our Mod Is associated with
    [BepInProcess("Rounds.exe")]
    public class Main : BaseUnityPlugin
    {
        private const string ModId = "com.root.projectile.size.patch";
        private const string ModName = "ProjectileSizePatch";
        public const string Version = "1.1.0"; // What version are we On (major.minor.patch)?
        void Awake()
        {
            // Use this to call any harmony patch files your Mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

        }
    }

    [Serializable]
    [HarmonyPatch(typeof(Gun), "ApplyPlayerStuff")]
    public class PatchGun
    {
        private static void Prefix(Gun __instance, GameObject obj)
        {
            foreach (var t in obj.GetComponentsInChildren<TrailRenderer>())
            {
                float o = t.startWidth;
                t.startWidth *= (1 + __instance.projectileSize);
                obj.GetComponent<ProjectileHit>().GetAdditionalData().projectileSize = t.startWidth - o;
                t.endWidth = t.startWidth;
                t.time = 0;
            }
        }
    }

    [Serializable]
    [HarmonyPatch(typeof(RayCastTrail), "Start")]
    public class PatchRayCastTrail
    {
        private static void Postfix(RayCastTrail __instance)
        {
            UnityEngine.Debug.Log(__instance.size);
            __instance.size += (__instance.GetComponent<ProjectileHit>().GetAdditionalData().projectileSize/2);
            UnityEngine.Debug.Log(__instance.size);
        }
    }


    [Serializable]
    public class ProjectileHitModifiers
    {
        public float projectileSize;
        public ProjectileHitModifiers()
        {
            projectileSize = 0;
        }
    }

    public static class ProjectileHitExtension
    {
        public static readonly ConditionalWeakTable<ProjectileHit, ProjectileHitModifiers> data =
          new ConditionalWeakTable<ProjectileHit, ProjectileHitModifiers>();
        public static ProjectileHitModifiers GetAdditionalData(this ProjectileHit projectile)
        {
            return data.GetOrCreateValue(projectile);
        }
        public static void AddData(this ProjectileHit projectile, ProjectileHitModifiers value)
        {
            try
            {
                data.Add(projectile, value);
            }
            catch (Exception) { }
        }
    }
}


