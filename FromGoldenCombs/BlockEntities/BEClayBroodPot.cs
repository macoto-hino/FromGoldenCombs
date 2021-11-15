using FromGoldenCombs.Blocks;
using FromGoldenCombs.config;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{
    public enum EnumHivePopSize
    {
        Poor = 0,
        Decent = 1,
        Large = 2
    }

    class BEClayBroodPot : BlockEntity
    {
        MeshData plane;
        TextureAtlasPosition texPosition;
        double harvestableAtTotalHours;
        double cooldownUntilTotalHours;
        public bool Harvestable;
        int quantityNearbyFlowers;
        int quantityNearbyHives;
        float actvitiyLevel;
        RoomRegistry roomreg;
        float roomness;
        public static SimpleParticleProperties Bees;
        int scanQuantityNearbyFlowers;
        int scanQuantityNearbyHives;
        int scanIteration;
        EnumHivePopSize hivePopSize;

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mesher.AddMeshData(plane);
            return false;
        }

        static BEClayBroodPot()
        {
            Bees = new SimpleParticleProperties(
                1, 1,
                ColorUtil.ToRgba(255, 215, 156, 65),
                new Vec3d(), new Vec3d(),
                new Vec3f(0, 0, 0),
                new Vec3f(0, 0, 0),
                1f,
                0f,
                0.5f, 0.5f,
                EnumParticleModel.Cube
            );
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(TestHarvestable, 6000);
            RegisterGameTickListener(OnScanForFlowers, api.World.Rand.Next(5000) + 30000);

            roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();

            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            if (api.Side == EnumAppSide.Client)
            {
               ICoreClientAPI capi = api as ICoreClientAPI;
                Block ownBlock = block;
                Shape shape = capi.Assets.TryGet(new AssetLocation("fromgoldencombs", "shapes/block/claypothive-empty-none-notop.json")).ToObject<Shape>();
                texPosition = capi.BlockTextureAtlas.GetPosition(ownBlock, "north");
                capi.Tesselator.TesselateShape(ownBlock, shape, out plane);

                if (api.Side == EnumAppSide.Client)
                {
                    RegisterGameTickListener(SpawnBeeParticles, 300);
                }
            }
        }

        public void BlockInteract(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ClayBroodPot block)
        {
            Block hive = Api.World.BlockAccessor.GetBlock(Pos);
            Block livePot = world.GetBlock(new AssetLocation("fromgoldencombs", "claypothive-populated-none-withtop"));
            Block liveNoPot = world.GetBlock(new AssetLocation("fromgoldencombs", "claypothive-populated-none-notop"));
            Block emptyTop = world.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-empty"));
            Block fullTop = world.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-harvestable"));

            if (hive.Variant["top"] == "notop"
                && hive.Variant["populated"] == "populated"
                && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack == null
                && (int)byPlayer.InventoryManager.ActiveHotbarSlot.StorageType == 2
                && byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(block)))
            {
                //Pick up populated hive in backpack slot
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.PlaySoundAt(new AssetLocation("sounds/block/planks"), blockSel.Position.X + 0.5, blockSel.Position.Y, blockSel.Position.Z + 0.5, byPlayer, false);
        
            }
            else if (hive.Variant["top"] == "notop"
                && (hive.Variant["populated"] == "populated"
                && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack != null && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code.ToString() == "fromgoldencombs:hivetop-empty"))
            {
                //add pot, retain populated status
                world.BlockAccessor.ExchangeBlock(livePot.BlockId, blockSel.Position);
                byPlayer.InventoryManager.ActiveHotbarSlot.TakeOutWhole();

            }
            else if (hive.Variant["top"] == "withtop"
              && hive.Variant["populated"] == "populated"
              && hive.Variant["harvestable"] == "none"
              && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack == null)
            {
                //Take back an empty pot from a populated hive
                byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(emptyTop, 1));
                world.BlockAccessor.ExchangeBlock(liveNoPot.BlockId, blockSel.Position);
            }
            else if (hive.Variant["top"] == "withtop"
              && hive.Variant["populated"] == "populated"
              && hive.Variant["harvestable"] == "harvestable"
              && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack == null)
            {
                //Take a harvestable pot from a populated hive
                Harvestable = false;
                byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(fullTop, 1));
                world.BlockAccessor.ExchangeBlock(liveNoPot.BlockId, blockSel.Position);
                harvestableAtTotalHours = 0;
            }
        }

        private void TestHarvestable(float dt)
        {
            int harvestBase = FromGoldenCombsConfig.Current.clayPotHiveHoursToHarvest;
            double worldTime = Api.World.Calendar.TotalHours;
            Block hive = Api.World.BlockAccessor.GetBlock(Pos);
            ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
            if (conds == null) return;

            float temp = conds.Temperature + (roomness > 0 ? 5 : 0);
            actvitiyLevel = GameMath.Clamp(temp / 5f, 0f, 1f);

            // Reset timers during winter
            if (temp <= -10)
            {
                //TODO: Readdress harvestAtTotalHours math to ensure it works for all ranges of growth time.
                harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
                cooldownUntilTotalHours = worldTime + 4 / 2 * 24;
            }

            if(!Harvestable && harvestableAtTotalHours==0 && hivePopSize > EnumHivePopSize.Poor && hive.Variant["top"] == "withtop")
            {
                harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
            }
            else if (!Harvestable && worldTime > harvestableAtTotalHours && hivePopSize > EnumHivePopSize.Poor && hive.Variant["top"] == "withtop")
            {
                Harvestable = true;
                Api.World.BlockAccessor.ExchangeBlock(Api.World.GetBlock(new AssetLocation("fromgoldencombs", "claypothive-populated-harvestable-withtop")).BlockId, Pos);
                MarkDirty(true);
            }
        }

        private double HarvestableTime(int i)
        {
            Random rand = new();
            return (i * .75) + ((i * .5) * rand.NextDouble());
        }

        readonly Vec3d startPos = new();
        readonly Vec3d endPos = new();
        Vec3f minVelo = new();

        private bool isWildHive;

        private void SpawnBeeParticles(float dt)
        {
                float dayLightStrength = Api.World.Calendar.GetDayLightStrength(Pos.X, Pos.Z);
                if (Api.World.Rand.NextDouble() > 2 * dayLightStrength - 0.5) return;

                Random rand = Api.World.Rand;

                Bees.MinQuantity = actvitiyLevel;

                // Leave hive
                if (Api.World.Rand.NextDouble() > 0.5)
                {
                    startPos.Set(Pos.X + 0.5f, Pos.Y + 0.5f, Pos.Z + 0.5f);
                    minVelo.Set((float)rand.NextDouble() * 3 - 1.5f, (float)rand.NextDouble() * 1 - 0.5f, (float)rand.NextDouble() * 3 - 1.5f);

                    Bees.MinPos = startPos;
                    Bees.MinVelocity = minVelo;
                    Bees.LifeLength = 1f;
                    Bees.WithTerrainCollision = false;
                }

                // Go back to hive
                else
                {
                    startPos.Set(Pos.X + rand.NextDouble() * 5 - 2.5, Pos.Y + rand.NextDouble() * 2 - 1f, Pos.Z + rand.NextDouble() * 5 - 2.5f);
                    endPos.Set(Pos.X + 0.5f, Pos.Y + 0.5f, Pos.Z + 0.5f);

                    minVelo.Set((float)(endPos.X - startPos.X), (float)(endPos.Y - startPos.Y), (float)(endPos.Z - startPos.Z));
                    minVelo /= 2;

                    Bees.MinPos = startPos;
                    Bees.MinVelocity = minVelo;
                    Bees.WithTerrainCollision = true;
                Api.World.SpawnParticles(Bees);
            }
        }

        private void OnScanForFlowers(float dt)
        {
            //Scan to get number of nearby flowers and active hives
            Room room = roomreg?.GetRoomForPosition(Pos);
            roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;

            if (actvitiyLevel < 1) return;
            if (Api.Side == EnumAppSide.Client) return;
            if (Api.World.Calendar.TotalHours < cooldownUntilTotalHours) return;

            if (scanIteration == 0)
            {
                scanQuantityNearbyFlowers = 0;
                scanQuantityNearbyHives = 0;
            }

            int minX = -8 + 8 * (scanIteration / 2);
            int minZ = -8 + 8 * (scanIteration % 2);
            int size = 8;

            Block fullSkepN = Api.World.GetBlock(new AssetLocation("skep-populated-north"));
            Block fullSkepE = Api.World.GetBlock(new AssetLocation("skep-populated-east"));
            Block fullSkepS = Api.World.GetBlock(new AssetLocation("skep-populated-south"));
            Block fullSkepW = Api.World.GetBlock(new AssetLocation("skep-populated-west"));

            Block wildhive1 = Api.World.GetBlock(new AssetLocation("wildbeehive-medium"));
            Block wildhive2 = Api.World.GetBlock(new AssetLocation("wildbeehive-large"));

            Block claypothive = Api.World.GetBlock(new AssetLocation("claypothive-populated-empty-withtop"));
            Block claypothive2 = Api.World.GetBlock(new AssetLocation("claypothive-populated-empty-notop"));
            Block claypothive3 = Api.World.GetBlock(new AssetLocation("claypothive-populated-harvestable-notop"));
            Block claypothive4 = Api.World.GetBlock(new AssetLocation("claypothive-populated-harvestable-withtop"));

            Api.World.BlockAccessor.WalkBlocks(Pos.AddCopy(minX, -5, minZ), Pos.AddCopy(minX + size - 1, 5, minZ + size - 1), (block, pos) =>
            {
                if (block.Id == 0) return;

                if (block.Attributes?.IsTrue("beeFeed") == true) scanQuantityNearbyFlowers++;

                if (block == fullSkepN || block == fullSkepE || block == fullSkepS || block == fullSkepW || block == wildhive1 || block == wildhive2 || block == claypothive || block == claypothive2 || block == claypothive3 || block == claypothive4)
                {
                    scanQuantityNearbyHives++;
                }
            });

            scanIteration++;

            if (scanIteration == 4)
            {
                scanIteration = 0;
                OnScanComplete();
            }
        }
        private void OnScanComplete()
        {
            quantityNearbyFlowers = scanQuantityNearbyFlowers;
            quantityNearbyHives = scanQuantityNearbyHives;

            hivePopSize = (EnumHivePopSize)GameMath.Clamp(quantityNearbyFlowers - 3 * quantityNearbyHives, 0, 2);

            MarkDirty();

        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            if (Api.World.EntityDebugMode && forPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                dsc.AppendLine(
                    Lang.Get("Nearby flowers: {0}, Nearby Hives: {1}, Empty Hives: {2}, Pop after hours: {3}. harvest in {4}, repop cooldown: {5}",quantityNearbyFlowers, quantityNearbyHives, (harvestableAtTotalHours - Api.World.Calendar.TotalHours).ToString("#.##"), (cooldownUntilTotalHours - Api.World.Calendar.TotalHours).ToString("#.##"))
                    + "\n" + Lang.Get("Population Size: " + hivePopSize));
            }

            string str = Lang.Get("Nearby flowers: {0}\nPopulation Size: {1}", quantityNearbyFlowers, hivePopSize);
            if (Harvestable) str += "\n" + Lang.Get("Harvestable");

            dsc.AppendLine(str);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);


            tree.SetInt("scanIteration", scanIteration);

            tree.SetInt("quantityNearbyFlowers", quantityNearbyFlowers);
            tree.SetInt("quantityNearbyHives", quantityNearbyHives);


            tree.SetInt("scanQuantityNearbyFlowers", scanQuantityNearbyFlowers);
            tree.SetInt("scanQuantityNearbyHives", scanQuantityNearbyHives);

            tree.SetInt("isWildHive", isWildHive ? 1 : 0);
            tree.SetInt("harvestable", Harvestable ? 1 : 0);
            tree.SetDouble("cooldownUntilTotalHours", cooldownUntilTotalHours);
            tree.SetDouble("harvestableAtTotalHours", harvestableAtTotalHours);
            tree.SetInt("hiveHealth", (int)hivePopSize);
            tree.SetFloat("roomness", roomness);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            bool wasHarvestable = Harvestable;

            scanIteration = tree.GetInt("scanIteration");

            quantityNearbyFlowers = tree.GetInt("quantityNearbyFlowers");
            quantityNearbyHives = tree.GetInt("quantityNearbyHives");

            scanQuantityNearbyFlowers = tree.GetInt("scanQuantityNearbyFlowers");
            scanQuantityNearbyHives = tree.GetInt("scanQuantityNearbyHives");

            isWildHive = tree.GetInt("isWildHive") > 0;
            Harvestable = tree.GetInt("harvestable") > 0;

            cooldownUntilTotalHours = tree.GetDouble("cooldownUntilTotalHours");
            harvestableAtTotalHours = tree.GetDouble("harvestableAtTotalHours");
            hivePopSize = (EnumHivePopSize)tree.GetInt("hiveHealth");
            roomness = tree.GetFloat("roomness");

            if (Harvestable != wasHarvestable && Api != null)
            {
                MarkDirty(true);
            }
        }

    }
}
