using FromGoldenCombs.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothStack : BlockContainer 
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            // Todo: Add interaction help
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BELangstrothStack belangstrothstack = (BELangstrothStack)world.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (belangstrothstack != null)
            {
                return belangstrothstack.OnInteract(byPlayer, blockSel);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
