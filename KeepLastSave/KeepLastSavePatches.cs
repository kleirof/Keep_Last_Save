using System;
using System.Reflection;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace KeepLastSave
{
	public static class KeepLastSavePatches
	{
		public static void EmitCall<T>(this ILCursor iLCursor, string methodName, Type[] parameters = null, Type[] generics = null)
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(T), methodName, parameters, generics);
			iLCursor.Emit(OpCodes.Call, methodInfo);
		}

		[HarmonyPatch(typeof(GameManager), nameof(GameManager.VerifyAndLoadMidgameSave))]
		public class VerifyAndLoadMidgameSavePatchClass
		{
			[HarmonyILManipulator]
			public static void VerifyAndLoadMidgameSavePatch(ILContext ctx)
			{
				ILCursor crs = new ILCursor(ctx);

				if (crs.TryGotoNext(MoveType.After,
					x => x.MatchCallvirt("System.Collections.Generic.List`1<System.String>", "Contains")
					))
				{
					crs.EmitCall<VerifyAndLoadMidgameSavePatchClass>(nameof(VerifyAndLoadMidgameSavePatchClass.VerifyAndLoadMidgameSavePatchCall));
				}
			}

			private static bool VerifyAndLoadMidgameSavePatchCall(bool orig)
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(MainMenuFoyerController), nameof(MainMenuFoyerController.InitializeMainMenu))]
		public class InitializeMainMenuPatchClass
		{
			[HarmonyILManipulator]
			public static void InitializeMainMenuPatch(ILContext ctx)
			{
				ILCursor crs = new ILCursor(ctx);

				if (crs.TryGotoNext(MoveType.Before,
					x => x.MatchCall<GameManager>("HasValidMidgameSave")
					))
				{
					crs.Emit(OpCodes.Ldarg_0);
					crs.EmitCall<InitializeMainMenuPatchClass>(nameof(InitializeMainMenuPatchClass.InitializeMainMenuPatchCall));
				}
			}

			private static void InitializeMainMenuPatchCall(MainMenuFoyerController self)
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
			}
		}
	}
}
