using System.Collections.Generic;
using System.IO;
using System.Text;
using AdvancedWorldGen.BetterVanillaWorldGen;
using AdvancedWorldGen.SpecialOptions;
using AdvancedWorldGen.SpecialOptions.Halloween;
using AdvancedWorldGen.SpecialOptions.Snow;
using AdvancedWorldGen.UI;
using Newtonsoft.Json;
using Terraria.ModLoader;
using OnWorldGen = On.Terraria.WorldGen;
using OnUIWorldCreation = On.Terraria.GameContent.UI.States.UIWorldCreation;
// using OnUIWorldListItem = On.Terraria.GameContent.UI.Elements.UIWorldListItem;
using OnUserInterface = On.Terraria.UI.UserInterface;
using OnUIWorldLoad = On.Terraria.GameContent.UI.States.UIWorldLoad;
using OnWorldFile = On.Terraria.IO.WorldFile;
using OnMain = On.Terraria.Main;
using ILProjectile = IL.Terraria.Projectile;
using ILWorldGen = IL.Terraria.WorldGen;

namespace AdvancedWorldGen.Base
{
	public class AdvancedWorldGen : Mod
	{
		public Crimruption Crimruption = null!;
		public UiChanger UiChanger = null!;
		public static AdvancedWorldGen Instance => ModContent.GetInstance<AdvancedWorldGen>();

		public override void Load()
		{
			OptionsSelector.OptionDict = JsonConvert.DeserializeObject<Dictionary<string, Option>>(
				Encoding.UTF8.GetString(GetFileBytes("Options.json")));
			TileReplacer.Initialize();

			UiChanger = new UiChanger(this);

			Crimruption = new Crimruption();

			OnUIWorldCreation.AddDescriptionPanel += UiChanger.TweakWorldGenUi;
			//OnUIWorldListItem.ctor += UiChanger.CopySettingsButton; Removed until twld can be loaded in a reasonable time

			ILWorldGen.GenerateWorld += ModifiedWorld.OverrideWorldOptions;
			OnWorldFile.CreateMetadata += DedServUi.DedServOptions;

			OnUIWorldLoad.ctor += UiChanger.AddCancel;
			OnWorldGen.worldGenCallback += UiChanger.ThreadifyWorldGen;

			OnUserInterface.SetState += ModifiedWorld.Instance.ResetSettings;

			OnWorldGen.NotTheBees += ClassicOptions.SmallNotTheBees;
			ILWorldGen.makeTemple += ClassicOptions.ReduceTemple;

			OnMain.UpdateTime_StartDay += ModifiedWorld.Instance.OnDawn;
			OnMain.UpdateTime_StartNight += ModifiedWorld.Instance.OnDusk;
			OnMain.checkXMas += SnowWorld.MainOnCheckXMas;
			ILProjectile.Kill += SnowWorld.RemoveSnowDropDuringChristmas;

			ILWorldGen.MakeDungeon += Crimruption.CrimruptionChest;

			Replacer.Replace();

			HalloweenCommon.Setup();
		}

		public override void Unload()
		{
			OnUIWorldCreation.AddDescriptionPanel -= UiChanger.TweakWorldGenUi;
			OnUIWorldLoad.ctor -= UiChanger.AddCancel;
			OnWorldGen.worldGenCallback -= UiChanger.ThreadifyWorldGen;
			ILWorldGen.GenerateWorld -= ModifiedWorld.OverrideWorldOptions;
			OnWorldFile.CreateMetadata -= DedServUi.DedServOptions;

			OnWorldGen.NotTheBees -= ClassicOptions.SmallNotTheBees;
			ILWorldGen.makeTemple -= ClassicOptions.ReduceTemple;

			OnMain.UpdateTime_StartDay -= ModifiedWorld.Instance.OnDawn;
			OnMain.UpdateTime_StartNight -= ModifiedWorld.Instance.OnDusk;
			ILProjectile.Kill -= SnowWorld.RemoveSnowDropDuringChristmas;

			ILWorldGen.MakeDungeon -= Crimruption.CrimruptionChest;

			Replacer.UnReplace();

			HalloweenCommon.UnSetup();

			OptionsSelector.OptionDict = null!;
			TileReplacer.Unload();
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			OptionHelper.HandlePacket(reader);
		}
	}
}