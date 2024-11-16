using BepInEx;

namespace Blasphemous.CombatImprovements;

[BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
[BepInDependency("Blasphemous.ModdingAPI", "0.1.0")]
public class Main : BaseUnityPlugin
{
    public static CombatImprovements CombatImprovements { get; private set; }

    private void Start()
    {
        CombatImprovements = new CombatImprovements();
    }
}
