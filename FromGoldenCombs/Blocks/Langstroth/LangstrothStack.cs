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
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            BELangstrothStack belangstrothsuper = (BELangstrothStack)world.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (belangstrothsuper != null) 
                return belangstrothsuper.OnInteract(byPlayer, blockSel);

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
