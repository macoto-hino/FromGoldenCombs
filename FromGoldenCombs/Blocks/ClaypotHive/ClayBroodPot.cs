using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using FromGoldenCombs.BlockEntities;

namespace FromGoldenCombs.Blocks
{
    class ClayBroodPot : Block
    {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            BEClayBroodPot bebeehive = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEClayBroodPot;
            Block livePot = world.GetBlock(new AssetLocation("fromgoldencombs", "claypothive-populated-none-withtop"));
            Block liveNoPot = world.GetBlock(new AssetLocation("fromgoldencombs", "claypothive-populated-none-notop"));
            Block emptyNoPot = world.GetBlock(new AssetLocation("fromgoldencombs", "claypothive-empty-none-notop"));
            Block emptyPot = world.GetBlock(new AssetLocation("fromgoldencombs", "claypothive-empty-none-withtop"));
            Block emptyTop = world.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-empty"));

            if (bebeehive != null && bebeehive is BEClayBroodPot)
            {
                //If there's a block entity at this pos, and its a BEClayBroodPot
                bebeehive.BlockInteract(world, byPlayer, blockSel, this);
                
            } else if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack == null 
                && Variant["top"] == "notop" 
                && (int)byPlayer.InventoryManager.ActiveHotbarSlot.StorageType == 2 
                && byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(this)))
            {
                //If the players hand is empty, and there's no top on the hive, and the open slot is a bagslot... Try to take the pot.

                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.PlaySoundAt(new AssetLocation("sounds/block/planks"), blockSel.Position.X + 0.5, blockSel.Position.Y, blockSel.Position.Z + 0.5, byPlayer, false);
            }
            else if (this.Variant["top"] == "withtop"
                       && this.Variant["populated"] == "empty"
                       && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack == null)
            {
                //If the variant has a top, and no colony in the pot, and the players hand is empty. Remove the top and give to the player.

                world.BlockAccessor.ExchangeBlock(emptyNoPot.BlockId, blockSel.Position);
                byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack = new ItemStack(emptyTop, 1);

            } else if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack != null)
            {
                //Otherwise, if the players hand is NOT empty
                if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.WildCardMatch(new AssetLocation("game", "skep-populated-*"))
                    && Variant["populated"] == "empty"
                    && Variant["top"] == "notop")
                {
                    //TODO: Combine these else If statements and find a cleaner way to change its state. 
                    //And they're holding a skep, and the pot has no colony in it, and it has no top.  Populate hive.
                    world.BlockAccessor.SetBlock(liveNoPot.BlockId, blockSel.Position);
                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOutWhole();

                }
                else if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.WildCardMatch(new AssetLocation("game", "skep-populated-*"))
                  && Variant["populated"] == "empty"
                  && Variant["top"] == "withtop")
                {
                    //TODO: Combine these else If statements and find a cleaner way to change its state. 
                    //And they're holding a skep, and the pot has no colony in it, and it has a top.  Populate hive.
                    world.BlockAccessor.SetBlock(livePot.BlockId, blockSel.Position);
                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOutWhole();

                }
                else if (this.Variant["top"] == "notop"
                    && this.Variant["populated"] == "empty"
                    && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code.ToString() == "fromgoldencombs:hivetop-empty")
                {
                    //TODO: Combine these else If statements and find a cleaner way to change its state. 
                    //add pot, retain empty status
                    world.BlockAccessor.ExchangeBlock(emptyPot.BlockId, blockSel.Position);
                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOutWhole();
                }
                else
                {
                    return base.OnBlockInteractStart(world, byPlayer, blockSel);
                }
            }
            return true;
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            //If the hive is broken, and is populated, potentially spawn bee mob.
            if (this.Variant["populated"] == "populated" 
                && world.Rand.NextDouble() < 0.4)
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
