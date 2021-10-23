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
            if (bed != null)
            {
                SetContents(stack, bed.GetContentStacks());
            }

            return stack;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            BELangstrothSuper belangstrothsuper = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELangstrothSuper;
            if (belangstrothsuper != null) return belangstrothsuper.OnInteract(byPlayer, blockSel);

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
