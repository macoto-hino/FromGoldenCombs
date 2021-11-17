using FromGoldenCombs.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothStack : LangstrothCore
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            // Todo: Add interaction help
        }
        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if(world.BlockAccessor.GetBlock(pos.DownCopy()).BlockMaterial == EnumBlockMaterial.Air)
            {
                this.OnBlockBroken(world, pos, null);
                if (world.BlockAccessor.GetBlock(pos.UpCopy()) is LangstrothCore)
                {
                    world.BlockAccessor.GetBlock(pos.UpCopy()).OnNeighbourBlockChange(world,pos.UpCopy(),neibpos);
                }

            }
            base.OnNeighbourBlockChange(world, pos, neibpos); 
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                ItemStack[] array = new ItemStack[]{};
                for (int i = 0; i < array.Length; i++)
                {
                    world.SpawnItemEntity(array[i], new Vec3d((double)pos.X + 0.5, (double)pos.Y + 0.5, (double)pos.Z + 0.5), null);
                }
                world.PlaySoundAt(this.Sounds.GetBreakSound(byPlayer), (double)pos.X, (double)pos.Y, (double)pos.Z, byPlayer, true, 32f, 1f);
            }
            if (this.EntityClass != null)
            {
                BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(pos);
                if (blockEntity != null)
                {
                    blockEntity.OnBlockBroken();
                }
            }
            world.BlockAccessor.SetBlock(0, pos);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            BELangstrothStack belangstrothstack = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELangstrothStack;
            if (belangstrothstack is BELangstrothStack) return belangstrothstack.OnInteract(byPlayer);
            
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
