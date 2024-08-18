using BepInEx;
using System;
using System.Reflection;
using UnityEngine;
using MonoMod.RuntimeDetour;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;
using System.Collections.Generic;

namespace KeepLastSave
{
	[BepInDependency("etgmodding.etg.mtgapi")]
	[BepInPlugin(GUID, NAME, VERSION)]
	public class KeepLastSaveModule : BaseUnityPlugin
	{
		public const string GUID = "kleirof.etg.keeplastsave";
		public const string NAME = "Keep Last Save";
		public const string VERSION = "1.0.0";
		public const string TEXT_COLOR = "#FFCCCC";

		public class KeepLastSavePatches
		{
			[HarmonyILManipulator, HarmonyPatch(typeof(GameManager), nameof(GameManager.VerifyAndLoadMidgameSave))]
			public static void VerifyAndLoadMidgameSavePatch(ILContext ctx)
			{
				ILCursor crs = new ILCursor(ctx);

				if (crs.TryGotoNext(MoveType.Before,
					x => x.MatchLdstr("Failed to load mid game save (2)")
					))
				{
					crs.Index--;
					crs.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Func<bool, bool>>
						(orig =>
						{
							return false;
						});
				}
			}

			[HarmonyILManipulator, HarmonyPatch(typeof(MainMenuFoyerController), nameof(MainMenuFoyerController.InitializeMainMenu))]
			public static void InitializeMainMenuPatch(ILContext ctx)
			{
				ILCursor crs = new ILCursor(ctx);

				if (crs.TryGotoNext(MoveType.Before,
					x => x.MatchCall<GameManager>("HasValidMidgameSave")
					))
				{
					crs.Emit(OpCodes.Ldarg_0);
					crs.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Action<MainMenuFoyerController>>
						(self =>
						{
							if (GameManager.HasValidMidgameSave())
							{
								self.ContinueGameButton.IsEnabled = true;
								self.ContinueGameButton.IsVisible = true;
							}
							else
							{
								self.ContinueGameButton.IsEnabled = false;
								self.ContinueGameButton.IsVisible = false;
							}
						});
				}
			}
		}

		public void Start()
		{
			ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
		}

		public void GMStart(GameManager g)
		{
			Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);

			Harmony.CreateAndPatchAll(typeof(KeepLastSavePatches));
		}

		public static void Log(string text, string color = "FFFFFF")
		{
			ETGModConsole.Log($"<color={color}>{text}</color>");
		}
	}
}
