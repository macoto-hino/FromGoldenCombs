using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using FromGoldenCombs.BlockEntities;

namespace FromGoldenCombs.Blocks
{
    class ClayBroodPot : Block
    {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            BEClayBroodPot bebeehive = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEClayBroodPot;
            ItemStack stack = new(world.GetBlock(new AssetLocation("fromgoldencombs", "ceramicbroodpot-notop")));
            ItemStack hivetopStack = new(world.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-" + (this.Variant["harvestable"] == "harvestable" ? "harvestable" : "empty"))));
            stack.Attributes.SetBool("populated", this.Variant["populated"] == "populated");

            if (bebeehive is BEClayBroodPot)
            {
                bebeehive.BlockInteract(world);
            }
            else if (this.Variant["top"] == "notop")
            {
                api.World.BlockAccessor.SetBlock(stack.Block.BlockId, blockSel.Position);
            }
            else
            {
                api.World.BlockAccessor.SetBlock(stack.Block.BlockId, blockSel.Position);
                BECeramicBroodPot beCBP = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(blockSel.Position);
                beCBP.TryPutDirect(hivetopStack);
            }
            this.OnNeighbourBlockChange(world, blockSel.Position, blockSel.Position);
            return true;
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
