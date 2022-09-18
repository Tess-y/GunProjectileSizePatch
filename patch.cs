using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using UnboundLib.Utils.UI;
using TMPro;
using UnboundLib;

namespace GunProjectileSizePatch
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our Mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our Mod Is associated with
    [BepInProcess("Rounds.exe")]
    public class Main : BaseUnityPlugin
    {
        private const string ModId = "com.root.projectile.size.patch";
        private const string ModName = "ProjectileSizePatch";
        public const string Version = "1.1.1"; // What version are we On (major.minor.patch)?
        public static ConfigEntry<bool> RenderFix;
        public static ConfigEntry<bool> ExtendTrail;
        void Awake()
        {
            // Use this to call any harmony patch files your Mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            RenderFix = base.Config.Bind<bool>("ProjectileSizePatch", "RenderFix", true, "Enable bullet render fix?");
            ExtendTrail = base.Config.Bind<bool>("ProjectileSizePatch", "ExtendTrail", false, "Extend bullet trail based on size?");

            Unbound.RegisterMenu("Projectile Size Patch", delegate () { }, new Action<GameObject>(this.NewGUI), null, true); 
        }
        private void NewGUI(GameObject menu)
        {
            TextMeshProUGUI textMeshProUGUI;
            MenuHandler.CreateText("ProjectileSizePatch", menu, out textMeshProUGUI, 60, false, null, null, null, null);
            MenuHandler.CreateToggle(ExtendTrail.Value, "Extend bullet trail", menu, delegate (bool value)
            {
                ExtendTrail.Value = value;
            }, 50);
            MenuHandler.CreateText("Enable to cause the bullet trail to scale with projectile size stat \n\n", menu, out textMeshProUGUI, 30, false, null, null, null, TextAlignmentOptions.Center);

            MenuHandler.CreateToggle(!RenderFix.Value, "Disable Render Fix", menu, delegate (bool value)
            {
                RenderFix.Value = !value;
            }, 50);
            MenuHandler.CreateText("Warning: disabling this setting will cause the buttlets to no longer match their hit box. \nThis will be more apparent at large bullet sizes.", menu, out textMeshProUGUI, 30, false, Color.red, null, null, TextAlignmentOptions.Center);
        }
    }

    [Serializable]
    [HarmonyPatch(typeof(Gun), "ApplyPlayerStuff")]
    public class PatchGun 
    {
        private static void Prefix(Gun __instance, GameObject obj)
        {
            TrailRenderer trail = null;
            foreach (var t in obj.GetComponentsInChildren<TrailRenderer>())
            {
                float o = t.startWidth;
                t.startWidth *= (1 + __instance.projectileSize);
                obj.GetComponent<ProjectileHit>().GetAdditionalData().projectileSize = t.startWidth - o;
                trail = t;
            }
            if (trail != null && Main.RenderFix.Value)
            {
                var t = UnityEngine.Object.Instantiate(trail, trail.transform.parent);
                trail.endWidth = trail.startWidth;
                trail.time = 0;
                if (Main.ExtendTrail.Value) t.time *= (1 + __instance.projectileSize);
            }else if(trail != null && Main.ExtendTrail.Value) trail.time *= (1 + __instance.projectileSize);
        }
    }

    [Serializable]
    [HarmonyPatch(typeof(RayCastTrail), "Start")]
    public class PatchRayCastTrail
    {
        private static void Postfix(RayCastTrail __instance)
        {
            __instance.size += (__instance.GetComponent<ProjectileHit>().GetAdditionalData().projectileSize/2);
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


