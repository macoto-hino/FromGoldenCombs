using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace FromGoldenCombs.config
{
    class FromGoldenCombsConfig
    {
        public int hiveHoursToHarvest = 120;
        public int clayPotHiveHoursToHarvest = 120;
        public int MaxStackSize = 6;
        public int baseframedurability = 32;
        //private ArrayList HiveSeasons = new();

        public FromGoldenCombsConfig()
        {}

        public static FromGoldenCombsConfig Current { get; set; }

        public static FromGoldenCombsConfig GetDefault()
        {
            FromGoldenCombsConfig defaultConfig = new();

            //Set to 120 before launch
            defaultConfig.hiveHoursToHarvest = 120;
            //Set to 120 before launch
            defaultConfig.clayPotHiveHoursToHarvest = 120;
            //defaultConfig.HiveSeasons =
            defaultConfig.MaxStackSize = 6;
            defaultConfig.baseframedurability = 32;
            return defaultConfig;
        }

    }
}
