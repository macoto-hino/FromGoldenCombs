using FromGoldenCombs.BlockEntities;
using Vintagestory.API.Common;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothStack : LangstrothCore 
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            // Todo: Add interaction help
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            BELangstrothStack belangstrothstack = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELangstrothStack;
            if (belangstrothstack != null) return belangstrothstack.OnInteract(byPlayer, blockSel);
            
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
