using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{
    class BELangstrothStack : BlockEntityDisplay
    {
        Block block;

        readonly InventoryGeneric inv;

        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "langstrothstack";

        public BELangstrothStack()
        {
            inv = new InventoryGeneric(3, "superslot-0", null, null);
            meshes = new MeshData[3];
        }

        public override void Initialize(ICoreAPI api)
        {
            block = api.World.BlockAccessor.GetBlock(Pos);
            base.Initialize(api);
        }

        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            CollectibleObject colObj = slot.Itemstack?.Collectible;
            bool isSuper = colObj?.Class == "langstrothsuper" && colObj.Variant["open"] == "closed";
            
            if (slot.Empty && (int)slot.StorageType == 2)  
            {
                
                if (TryTake(byPlayer, blockSel))
                {
                    UpdateStackSize();
                    MarkDirty(true);
                    return true;
                }
            } else if (isSuper)
            {
                if (TryPut(slot, blockSel))
                {
                    UpdateStackSize();
                    MarkDirty(true);
                    return true;
                }
            }
            return false;
        }

        private void UpdateStackSize()
        {
            int filledstacks = 0;
            string stacksize;
            for (int i = 0; i < inv.Count; i++)
            {
                if (!inv[i].Empty)
                {
                    filledstacks++;
                }
                System.Diagnostics.Debug.WriteLine(filledstacks);
            }
           
            stacksize = filledstacks == 0 ? "zero" : filledstacks == 1 ? "one" : filledstacks == 2 ? "two" : "three";
            System.Diagnostics.Debug.WriteLine(stacksize);
            if (stacksize != "zero")
            {
                Api.World.BlockAccessor.ExchangeBlock(Api.World.BlockAccessor
                    .GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-"+stacksize+"-"+this.block.Variant["side"])).BlockId,Pos);
                MarkDirty(true);
            } else
            {
                Api.World.BlockAccessor.SetBlock(0, Pos);
            }
            MarkDirty(true);
        }

         private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;
            System.Diagnostics.Debug.WriteLine(IsSuperAbove(blockSel.Position.Up()));
            if (!inv[index].Empty && ((index == 2 && !IsSuperAbove(Pos.Up())) || (index<2 && inv[index+1].Empty)))
            if (!inv[index].Empty && (index == 2 || inv[index + 1].Empty))
            {
                ItemStack stack = inv[index].TakeOutWhole();
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                return true;
            }

            return false;
        }

        private bool IsSuperAbove(BlockPos pos)
        {
            
            return Api.World.BlockAccessor.GetBlock(pos).BlockId != 0;
        
        }

        public bool InitializePut(ItemStack first,ItemSlot slot)
        {
            inv[0].Itemstack = first;
            inv[1].Itemstack = slot.TakeOutWhole();
            UpdateStackSize();
            updateMeshes();
            MarkDirty(true);
            return true;
            
        }

        private bool TryPut(ItemSlot slot, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;

                if (inv[index].Empty && (index==0 || !inv[index-1].Empty))
                {
                    inv[index].Itemstack = slot.TakeOutWhole();
                    updateMeshes();
                    MarkDirty(true);
                    return true;
                }

            return false;
        }

        readonly Matrixf mat = new();

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mat.Identity();
            mat.RotateYDeg(block.Shape.rotateY);

            return base.OnTesselation(mesher, tessThreadTesselator);
        }

        protected override void updateMeshes()
        {
            mat.Identity();
            mat.RotateYDeg(block.Shape.rotateY);

            base.updateMeshes();
        }

        protected override MeshData genMesh(ItemStack stack, int index)
        {
            MeshData mesh;

            ICoreClientAPI capi = Api as ICoreClientAPI;
            mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
            nowTesselatingItem = stack.Item;
            nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Block.Shape.Base);
            mesh.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);

            float x = 0;
            float y = .3333f * index;
            float z = 0;
            Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
            //This seems to work for rotating the actual appearance of the blocks in the itemslots.
            mesh.Rotate(new Vec3f(0.5f, 0f, 0.5f), 0f, block.Shape.rotateY * GameMath.DEG2RAD, 0f);
            mesh.Translate(offset.XYZ);           

            return mesh;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
            } else { 
                for (int i = inv.Count-1; i >= 0; i--)
                {
                    ItemSlot slot = inv[i];
                    if (slot.Empty)
                    {
                        sb.AppendLine(Lang.Get("Empty"));
                    }
                    else
                    {
                        sb.AppendLine(slot.Itemstack.GetName());
                    }
                }
                return;
            }
        }
    }
}
