using BepInEx;
using HarmonyLib;

namespace KeepLastSave
{
	[BepInDependency("etgmodding.etg.mtgapi")]
	[BepInPlugin(GUID, NAME, VERSION)]
	public class KeepLastSaveModule : BaseUnityPlugin
	{
		public const string GUID = "kleirof.etg.keeplastsave";
		public const string NAME = "Keep Last Save";
		public const string VERSION = "1.0.1";
		public const string TEXT_COLOR = "#FFCCCC";

		public void Start()
		{
			ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
		}

		public void GMStart(GameManager g)
		{
			Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);

			Harmony harmony = new Harmony(GUID);
			harmony.PatchAll();
		}

		public static void Log(string text, string color = "FFFFFF")
		{
			ETGModConsole.Log($"<color={color}>{text}</color>");
		}
	}
}
