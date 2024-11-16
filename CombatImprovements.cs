#nullable enable

using Blasphemous.ModdingAPI;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore.Attributes.Logic;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Attack;
using HarmonyLib;
using System.Reflection;

namespace Blasphemous.CombatImprovements;

public class CombatImprovements : BlasMod
{
    public CombatImprovements() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION) { }

    public static BlasMod? BlasMod;

    protected override void OnInitialize()
    {
        BlasMod = this;
        LogError($"{ModInfo.MOD_NAME} has been initialized");
    }
}

[HarmonyPatch(typeof(Penitent), nameof(Penitent.Damage))]
class Penitent_Damage_Patch()
{
    public static bool Prefix(ref Penitent __instance, Hit hit)
    {
        CombatImprovements.BlasMod?.LogError($"Element: {hit.DamageElement}");
        if (__instance.IsDashing || hit.DamageElement == DamageArea.DamageElement.Contact) return false;
        return true;
    }
}

[HarmonyPatch(typeof(PenitentSword), nameof(PenitentSword.SuccessParryChance))]
class PenitentSword_SuccessParryChance_Patch()
{
    private static readonly FieldInfo OnParryField = typeof(PenitentSword).GetField(nameof(PenitentSword.OnParry), BindingFlags.Instance | BindingFlags.NonPublic);

    public static bool Prefix(ref PenitentSword __instance, ref bool __result, Hit hit)
    {
        var enemy = hit.AttackingEntity.GetComponent<Enemy?>();
        var projectile = hit.AttackingEntity.GetComponent<ParriableProjectile?>();
        var onParry = (Core.SimpleEventParam?) OnParryField.GetValue(__instance);

        var sameDirection = __instance.IsEnemySameDirection(enemy) || enemy == null;
        if (sameDirection)
        {
            enemy?.Parry();
            projectile?.OnParry();
            if (enemy != null) onParry?.Invoke(enemy);
        }

        __result = sameDirection;
        return false;
    }
}

[HarmonyPatch(typeof(Parry), "IsHitParryable")]
class Parry_IsHitParryable_Patch()
{
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(PlatformCharacterController), "DoClimbing")]
class PlatformCharacterController_DoClimbing_Patch()
{
    private static readonly FieldInfo InAirLadderGrabSensitivityField = typeof(PlatformCharacterController).GetField("m_inAirLadderGrabSensitivity", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo LadderWidthFactorField = typeof(PlatformCharacterController).GetField("m_ladderWidthFactor", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void Prefix(ref PlatformCharacterController __instance)
    {
        InAirLadderGrabSensitivityField.SetValue(__instance, 0f);
        LadderWidthFactorField.SetValue(__instance, 4f);
    }
}
