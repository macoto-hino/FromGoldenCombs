using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

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
            } else if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.WildCardMatch(new AssetLocation("game", "skep-populated-*")) && Variant["populated"] == "empty")
            {
                api.World.BlockAccessor.SetBlock(api.World.BlockAccessor.GetBlock(this.CodeWithVariant("populated","populated")).BlockId, blockSel.Position);
                byPlayer.InventoryManager.ActiveHotbarSlot.TakeOutWhole();
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos) {
            StringBuilder sb = new();
                        
            return Variant["populated"].UcFirst() + " " + sb.ToString() + base.GetPlacedBlockName(world, pos);
        }
    }

    
}
