using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using FromGoldenCombs.BlockEntities;

namespace FromGoldenCombs.Blocks
{
    class CeramicBroodPot : Block
    {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BECeramicBroodPot beCeramicBroodPot = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (beCeramicBroodPot is BECeramicBroodPot) return beCeramicBroodPot.OnInteract(byPlayer);
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            
            return base.OnPickBlock(world, pos); 
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            BECeramicBroodPot beCeramicBroodPot = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(pos);
            //If the hive is broken, and is populated, potentially spawn bee mob.
            if (beCeramicBroodPot.GetPopulated()  && world.Rand.NextDouble() < 0.4)
            {
                EntityProperties type = world.GetEntityType(new AssetLocation("beemob"));
                Entity entity = world.ClassRegistry.CreateEntity(type);

                if (entity != null)
                {
                    entity.ServerPos.X = pos.X + 0.5f;
                    entity.ServerPos.Y = pos.Y + 0.5f;
                    entity.ServerPos.Z = pos.Z + 0.5f;
                    entity.ServerPos.Yaw = (float)world.Rand.NextDouble() * 2 * GameMath.PI;
                    entity.Pos.SetFrom(entity.ServerPos);

                    entity.Attributes.SetString("origin", "brokenbeehive");
                    world.SpawnEntity(entity);
                }
            }
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            //Information about world interaction
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = Variant["top"] == "notop" && Variant["populated"] == "empty" ? "fromgoldencombs:blockhelp-claypothive-empty-notop" : Variant["top"] == "notop" && Variant["populated"] == "populated" ? "fromgoldencombs:blockhelp-claypothive-populated-notop" : Variant["top"] == "withtop" && Variant["populated"] == "empty" ? "fromgoldencombs:blockhelp-claypothive-empty-withtop" : "fromgoldencombs:blockhelp-claypothive-populated-withtop",
                }
            };

        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return GetHandbookDropsFromBreakDrops(handbookStack, forPlayer);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return null;
        }
    }
}
