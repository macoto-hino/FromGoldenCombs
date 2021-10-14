using System;
using Vintagestory.API.Common;

namespace FromGoldenCombs.Blocks
{
    class ClayHiveTop : Block
    {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            Block emptyTop = world.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-empty"));
            if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack == null && byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(this)))
            {
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.PlaySoundAt(new AssetLocation("sounds/block/planks"), blockSel.Position.X + 0.5, blockSel.Position.Y, blockSel.Position.Z + 0.5, byPlayer, false);
                return true;
            }
            else if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.WildCardMatch(new AssetLocation("knife-*"))
              && this.Variant["type"] == "harvestable")
            {
                Random rand = new();
                byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(world.GetItem(new AssetLocation("game", "honeycomb")), rand.Next(1, 5)));
                world.BlockAccessor.SetBlock(emptyTop.BlockId, blockSel.Position);
            }
            return true;

        }
    }
}
