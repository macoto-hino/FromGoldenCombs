using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.Blocks.Langstroth;
using System;
using System.Collections.Generic;
using System.Text;
using VFromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FromGoldenCombs.Blocks
{
    class LangstrothSuper : LangstrothCore
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            // Todo: Add interaction help
        }

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
