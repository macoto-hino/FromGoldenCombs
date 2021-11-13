using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using FromGoldenCombs.Blocks;
using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.config;
using FromGoldenCombs.Blocks.Langstroth;
using VFromGoldenCombs.Blocks.Langstroth;

namespace FromGoldenCombs
{
    class FromGoldenCombs : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            //BlockEntities
            api.RegisterBlockEntityClass("fgcbeehive", typeof(FGCBeehive));
            api.RegisterBlockEntityClass("beclaypothive", typeof(BEClayBroodPot));
            api.RegisterBlockEntityClass("belangstrothsuper", typeof(BELangstrothSuper));
            api.RegisterBlockEntityClass("belangstrothstack", typeof(BELangstrothStack));

            //Blocks
            api.RegisterBlockClass("claypothive", typeof(ClayBroodPot));
            api.RegisterBlockClass("hivetop", typeof(ClayHiveTop));
            api.RegisterBlockClass("rawclaypothive", typeof(RawBroodPot));
            api.RegisterBlockClass("langstrothsuper", typeof(LangstrothSuper));
            api.RegisterBlockClass("langstrothbrood", typeof(LangstrothBrood));
            api.RegisterBlockClass("langstrothbase", typeof(LangstrothBase));
            api.RegisterBlockClass("langstrothstack", typeof(LangstrothStack));
            api.RegisterBlockClass("waxblock", typeof(WaxBlock));
            api.RegisterBlockClass("honeyjar", typeof(HoneyJar));

            //Items
            api.RegisterItemClass("langstrothframe", typeof(LangstrothFrame));


            try
            {
                var Config = api.LoadModConfig<FromGoldenCombsConfig>("fromgoldencombs.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    FromGoldenCombsConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    FromGoldenCombsConfig.Current = FromGoldenCombsConfig.GetDefault();
                }
            }
            catch
            {
                FromGoldenCombsConfig.Current = FromGoldenCombsConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                if (FromGoldenCombsConfig.Current.hiveHoursToHarvest <= 0)
                    FromGoldenCombsConfig.Current.hiveHoursToHarvest = 1488;
                if (FromGoldenCombsConfig.Current.clayPotHiveHoursToHarvest <= 0)
                    FromGoldenCombsConfig.Current.clayPotHiveHoursToHarvest = 1488;
                api.StoreModConfig(FromGoldenCombsConfig.Current, "fromgoldencombs.json");
            }
        }

            //TODO: Project List
            //Add Clay Honeypot For Pre-Bucket/Barrel storage
            //Add Wax Blocks For placeable, stackable storage of wax
            //Langstroth Hive:
            //Add Langstroth Base
            //Langstroth base will contain controlling logic
            //Add Langstroth Super
            //Add Langstroth Brood Box
            //Give Raccons the ability to knock the top off a hive, and then eat it.
            //Give hivetops growth mechanics for volume.
        }
}
