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
        //private ArrayList HiveSeasons = new();

        public FromGoldenCombsConfig()
        {}

        public static FromGoldenCombsConfig Current { get; set; }

        public static FromGoldenCombsConfig getDefault()
        {
            FromGoldenCombsConfig defaultConfig = new FromGoldenCombsConfig();

            defaultConfig.hiveHoursToHarvest = 120;
            defaultConfig.clayPotHiveHoursToHarvest = 120;
            //defaultConfig.HiveSeasons = 
            return defaultConfig;
        }

    }
}
