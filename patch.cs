using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;

namespace GunProjectileSizePatch
{    // Declares our Mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our Mod Is associated with
    [BepInProcess("Rounds.exe")]
    public class Main : BaseUnityPlugin
    {
        private const string ModId = "com.root.projectile.size.patch";
        private const string ModName = "ProjectileSizePatch";
        public const string Version = "1.0.0"; // What version are we On (major.minor.patch)?
        void Awake()
        {
            // Use this to call any harmony patch files your Mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

        }
    }

    [Serializable]
    [HarmonyPatch(typeof(Gun), "ApplyPlayerStuff")]
    public class Patch
    {
        private static void Prefix(Gun __instance, GameObject obj)
        {
            foreach (var t in obj.GetComponentsInChildren<TrailRenderer>())
            {
                t.startWidth *= (1 + __instance.projectileSize);
            }
        }
    }
}
