using System;
using System.Reflection;
using AnimatedMoodles;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SyncedHeartbeat;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("CasualtiesUnknown.exe")]
[BepInDependency("com.sosumi.animatedmoodles")]
public partial class AMSHBPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("Plugin AM-SyncedHeartbeat is loaded!");
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_GUID);
    }

    [HarmonyPatch(typeof(AnimatedMoodle), nameof(AnimatedMoodle.Update))]
    public static class Update_Patch
    {
        static void Postfix(AnimatedMoodle __instance)
        {
            if (!__instance.IsCritical)
                return;

            Body body = PlayerCamera.main.body;

            float fixedProg = body.heartProg - 0.2f;
            if (fixedProg < 0)
                fixedProg++;

            float pulseR = Mathf.Lerp(1 - fixedProg, 0, .8f);

            float severityMult = __instance.Intensity * .6f;

            float weaknessMult = Mathf.Max(
                1 - Mathf.Pow(body.fibrillationProgress, 3) / 10000 / 100,
                0
            );

            if (body.heartRate <= 0)
                pulseR = 0;

            switch (Plugin.SelectedCriticalStyle.Value)
            {
                case CriticalStyle.AlarmHop:
                    __instance.rect.anchoredPosition = new Vector2(
                        __instance.currentPos.x,
                        __instance.currentPos.y + pulseR * 100 * severityMult * weaknessMult
                    );
                    break;
            }
        }
    }
}
