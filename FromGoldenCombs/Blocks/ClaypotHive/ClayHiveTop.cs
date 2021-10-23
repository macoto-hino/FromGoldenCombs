using System;
using Vintagestory.API.Common;

namespace FromGoldenCombs.Blocks
{
    class ClayHiveTop : Block
    {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
           
            Block emptyTop = world.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-empty"));
            
            
            //TODO: Potentially remove the need for the active hotbar slot to be empty, ]
            //provided TryGiveItemStack just drops it into a convenient empty slot.

            if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack == null && byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(this)))
            {
                //If the active hot bar slot is empty, and can the player can accept the item, pick it up, play sound.
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.PlaySoundAt(new AssetLocation("sounds/block/planks"), blockSel.Position.X + 0.5, blockSel.Position.Y, blockSel.Position.Z + 0.5, byPlayer, false);
                return true;
            }
            else if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.WildCardMatch(new AssetLocation("knife-*"))
              && this.Variant["type"] == "harvestable")
            {
                //If the top is harvestable, and the player uses a knife on it, drop between 1-5 honeycomb.
                //TODO: Switch this to default drop method (using JSON)

                Random rand = new();
                byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(world.GetItem(new AssetLocation("game", "honeycomb")), rand.Next(1, 5)));
                world.BlockAccessor.SetBlock(emptyTop.BlockId, blockSel.Position);
            }
            return true;

        }
    }
}
