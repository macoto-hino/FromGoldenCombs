using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace FromGoldenCombs.Blocks
{
    class LangstrothSuper : LangstrothCore
    {
        
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            // Todo: Add interaction help
        }
        
        //Picks up block while retaining its contents
        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            ItemStack stack = base.OnPickBlock(world, pos);

            BELangstrothSuper bed = world.BlockAccessor.GetBlockEntity(pos) as BELangstrothSuper;
            if (bed is BELangstrothSuper)
            {
                SetContents(stack, bed.GetContentStacks());
            }

            return stack;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!(byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Block is LangstrothCore))
            {
                BELangstrothSuper belangstrothsuper = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELangstrothSuper;
                if (belangstrothsuper is BELangstrothSuper) return belangstrothsuper.OnInteract(byPlayer, blockSel);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
