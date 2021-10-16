using FromGoldenCombs.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

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

        //public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        //{
        //    var facing = SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
        //    bool placed;
        //    placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        //    if (placed)
        //    {
        //        var block = this.api.World.BlockAccessor.GetBlock(blockSel.Position);
        //        var newPath = block.Code.Path;
        //        newPath = newPath.Replace("north", facing);
        //        block = this.api.World.GetBlock(block.CodeWithPath(newPath));
        //        this.api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
        //    }
        //    return placed;
        //}
    }
}
