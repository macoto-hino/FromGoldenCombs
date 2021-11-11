using FromGoldenCombs.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothCore : BlockContainer
    {
        //Enable selectionbox interaction
        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            Block block = api.World.BlockAccessor.GetBlock(blockSel.Position);
            if (!slot.Empty &&
                IsValidLangstroth(block))
            {
                ItemStack super = this.OnPickBlock(api.World, blockSel.Position);
                api.World.BlockAccessor.SetBlock(api.World.GetBlock(
                new AssetLocation("fromgoldencombs", "langstrothstack-two-" + block.LastCodePart())).BlockId, blockSel.Position);
                BELangstrothStack lStack = (BELangstrothStack)api.World.BlockAccessor.GetBlockEntity(blockSel.Position);
                lStack.InitializePut(super, slot);
                return true;
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public bool IsValidLangstroth(Block block)
        {
            if (block is LangstrothCore)
            {
                return true;
            }
            return false;
        }
    }
}
