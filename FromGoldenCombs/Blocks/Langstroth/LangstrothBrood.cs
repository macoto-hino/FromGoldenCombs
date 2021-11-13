using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothBrood : LangstrothCore
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            System.Diagnostics.Debug.WriteLine("StorageType is " + slot.StorageType.ToString());
            System.Diagnostics.Debug.WriteLine("Slot is " + slot.Empty);
            if (slot.Empty && (int)slot.StorageType == 2)
            {
                ItemStack stack = api.World.BlockAccessor.GetBlock(blockSel.Position).OnPickBlock(api.World, blockSel.Position);
                api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                return byPlayer.InventoryManager.TryGiveItemstack(stack);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
