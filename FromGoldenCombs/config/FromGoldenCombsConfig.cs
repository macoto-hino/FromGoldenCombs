using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace FromGoldenCombs.config
{
    class FromGoldenCombsConfig
    {
        public int hiveHoursToHarvest = 168;
        public int clayPotHiveHoursToHarvest = 168;
        public int langstrothHiveHoursToHarvest = 168;
        public int MaxStackSize = 6;
        public int baseframedurability = 32;
        public int minFrameYield = 2;
        public int maxFrameYield = 4;
        public bool showcombpoptime = false;


        //private ArrayList HiveSeasons = new();

        public FromGoldenCombsConfig()
        {}

        public static FromGoldenCombsConfig Current { get; set; }

        public static FromGoldenCombsConfig GetDefault()
        {
            FromGoldenCombsConfig defaultConfig = new();

            //Set to 120 before launch
            defaultConfig.hiveHoursToHarvest = 168;
            //Set to 120 before launch
            defaultConfig.clayPotHiveHoursToHarvest = 168;
            //Set to 120 before launch
            defaultConfig.langstrothHiveHoursToHarvest = 168;
            defaultConfig.MaxStackSize = 6;
            defaultConfig.baseframedurability = 32;
            defaultConfig.minFrameYield = 2;
            defaultConfig.maxFrameYield = 4;
            defaultConfig.showcombpoptime = false;
            //defaultConfig.HiveSeasons =
            return defaultConfig;
        }

    }
}
