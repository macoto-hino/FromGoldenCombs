using FromGoldenCombs.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
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
            if (!slot.Empty && slot.Itemstack.Collectible is Block && isValidLangstroth(slot.Itemstack.Block, block))
            {
                createLangstrothStack(slot, byPlayer, block, blockSel.Position);
                return true;
            }

            return false;
        }

        //Is The Langstroth Block of type LangstrothCore
        public bool isValidLangstroth(Block heldBlock, Block targetBlock)
        {
            return (!(targetBlock is LangstrothBrood) && heldBlock is LangstrothCore);
            
        }

        public void createLangstrothStack(ItemSlot slot, IPlayer byPlayer, Block block, BlockPos pos)
        {
            ItemStack super = this.OnPickBlock(api.World, pos);
            api.World.BlockAccessor.SetBlock(api.World.GetBlock(
            new AssetLocation("fromgoldencombs", "langstrothstack-two-" + block.LastCodePart())).BlockId, pos);
            BELangstrothStack lStack = (BELangstrothStack)api.World.BlockAccessor.GetBlockEntity(pos);
            lStack.InitializePut(super, slot);
        }

        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
        {
            if (this is LangstrothStack)
            return base.GetPlacedBlockName(world, pos);
 
            StringBuilder sb = new();
            return base.GetPlacedBlockName(world, pos) + sb.AppendLine() + Lang.Get("fromgoldencombs:getmaterials", this.Variant["primary"].ToString().UcFirst(), this.Variant["accent"].ToString().UcFirst());
        }
    }
 }
