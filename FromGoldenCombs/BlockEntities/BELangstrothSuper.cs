using FromGoldenCombs.Items;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{
    //TODO: Consider adding a lid object, or adding an animation showing the lid being slid off (This sounds neat). 
    //TODO: Find out how to get animation functioning
    //TODO: Fix selection box issue
    
    class BELangstrothSuper : BlockEntityDisplay
    {

        readonly InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "langstrothsuper";

        Block block;

        public BELangstrothSuper()
        {
            inv = new InventoryGeneric(10, "frameslot-0", null, null);
            meshes = new MeshData[10];
        }

        public override void Initialize(ICoreAPI api)
        {
            block = api.World.BlockAccessor.GetBlock(Pos);
            base.Initialize(api);
        }
                
        public override void OnBlockBroken(IPlayer player)
        {
            // Don't drop inventory contents
        }

        //TODO: Add animations to Langstroth Super
        //BlockEntityAnimationUtil AnimUtil
        //{
        //    get { return GetBehavior<BEBehaviorAnimatable>()?.animUtil; }
        //}
        
        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            CollectibleObject colObj = slot.Itemstack?.Collectible;
            bool isBeeframe = colObj is LangstrothFrame;
            BlockContainer block = Api.World.BlockAccessor.GetBlock(blockSel.Position) as BlockContainer;
            block.SetContents(new(block), this.GetContentStacks());

            
            if (!slot.Empty && slot.Itemstack.Collectible.FirstCodePart() == "langstrothbroodtop" && (slot.Itemstack.Collectible.Variant["primary"] == this.Block.Variant["primary"] && slot.Itemstack.Collectible.Variant["accent"] == this.Block.Variant["accent"]))
            {
                if (inv.Empty)
                {
                    Api.World.BlockAccessor.SetBlock(Api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "langstrothbrood-empty-" + this.Block.Variant["primary"] + "-" + this.Block.Variant["accent"] + "-" + this.block.Variant["side"])).BlockId, Pos);
                    slot.TakeOutWhole();
                    MarkDirty(true);
                    return true;
                }
                (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, "nonemptysuper", Lang.Get("fromgoldencombs:nonemptysuper"));
                System.Diagnostics.Debug.WriteLine("Super to Broodbox conversion fail due to non-empty super.");
            }
            else if ((slot.Empty || !isBeeframe) && blockSel.SelectionBoxIndex < 10 && this.Block.Variant["open"] == "open")
            {
                if (TryTake(byPlayer, blockSel))
                {
                    MarkDirty(true);
                    return true;
                }
            }
            else if (isBeeframe && blockSel.SelectionBoxIndex < 10  && this.Block.Variant["open"] == "open")
            {
                //AssetLocation sound = slot.Itemstack?.Item?.Sounds?.Place;
                MarkDirty(true);
                if (TryPut(slot, blockSel))
                {
                    //Api.World.PlaySoundAt(sound != null ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    return true;
                }

            }
            else if (//byPlayer.Entity.Controls.Sneak &&
                     slot.Itemstack == null
                     && (int)slot.StorageType == 2
                     && Api.World.BlockAccessor.GetBlock(blockSel.Position).Variant["open"] == "closed"
                     && byPlayer.InventoryManager.TryGiveItemstack(block.OnPickBlock(Api.World, blockSel.Position))
                     )
            {
                Api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                MarkDirty(true);
                return true;        
            }
            else if (this.Block.Variant["open"] == "open" && !byPlayer.Entity.Controls.Sneak)
            {
                Api.World.BlockAccessor.ExchangeBlock(Api.World.GetBlock(block.CodeWithVariant("open", "closed")).BlockId, blockSel.Position);
                MarkDirty(true);
                return true;
            }
            else if (this.Block.Variant["open"] == "closed")
            { 
                Api.World.BlockAccessor.ExchangeBlock(Api.World.GetBlock(block.CodeWithVariant("open", "open")).BlockId, blockSel.Position);
                MarkDirty(true);
                return true;
            }
            return false;
        }

        private bool TryPut(ItemSlot slot, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;

            for (int i = 0; i < inv.Count; i++)
            {
                int slotnum = (index + i) % inv.Count;
                if (inv[slotnum].Empty)
                {
                    int moved = slot.TryPutInto(Api.World, inv[slotnum]);
                    updateMeshes();
                    return moved > 0;
                }
            }

            return false;
        }

        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;
            
            if (!inv[index].Empty)
            {
                ItemStack stack = inv[index].TakeOut(1);
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0)
                {
                    Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                updateMeshes();
                return true;
            }

            return false;
        }


        readonly Matrixf mat = new();

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            //mat.Identity();
            //if (block.Variant["side"] == "north" || block.Variant["side"] == "south")
            //{
            //    mat.RotateYDeg(block.Shape.rotateY);
            //}

            return base.OnTesselation(mesher, tessThreadTesselator);
        }

        public override void updateMeshes()
        {
            for (int i = 0; i < this.meshes.Length; i++)
            {
                this.updateMesh(i);
            }

            base.updateMeshes();
        }

        protected override void updateMesh(int index)
        {
            if (this.Api == null || this.Api.Side == EnumAppSide.Server)
            {
                return;
            }
            if (this.Inventory[index].Empty)
            {
                this.meshes[index] = null;
                return;
            }
            MeshData meshData = this.genMesh(this.Inventory[index].Itemstack);
            this.TranslateMesh(meshData, index);
            this.meshes[index] = meshData;
        }

        public override void TranslateMesh(MeshData mesh, int index)
        {
            float x = 0f;
            float y = 0.069f;
            float z = 0f;

            if (block.Variant["side"] == "north")
            {
                x = .7253f + .0625f * index - 1;
                Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
                mesh.Translate(offset.XYZ);
            }
            else if (block.Variant["side"] == "south")
            {
                x = 0.2747f - .0625f * index;
                Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
                mesh.Translate(offset.XYZ);
            }
            else if (block.Variant["side"] == "west")
            {
                z = 0.2747f - .0625f * index;
                Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
                mesh.Translate(offset.XYZ);
            }
            else if (block.Variant["side"] == "east")
            {
                z = 0.7253f + .0625f * index - 1;
                Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
                mesh.Translate(offset.XYZ);
            }
        }
        protected override MeshData genMesh(ItemStack stack)
        {

            IContainedMeshSource containedMeshSource = stack.Collectible as IContainedMeshSource;
            MeshData meshData;
            if (containedMeshSource != null)
            {
                meshData = containedMeshSource.GenMesh(stack, this.capi.BlockTextureAtlas, this.Pos);
                meshData.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, base.Block.Shape.rotateY * 0.017453292f, 0f);
            } else
            {
                this.nowTesselatingObj = stack.Collectible;
                this.nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
                capi.Tesselator.TesselateItem(stack.Item, out meshData, this);
            }
            ModelTransform transform = stack.Collectible.Attributes.AsObject<ModelTransform>();
            transform.EnsureDefaultValues();
            transform.Rotation.X = 0;
            transform.Rotation.Y = block.Shape.rotateY;
            transform.Rotation.Z = 0;
            meshData.ModelTransform(transform);

            return meshData;
        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            int index = forPlayer.CurrentBlockSelection.SelectionBoxIndex;
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
            }
            else if (this.Block.Variant["open"] == "closed")
            {

                return;
            } else if (index == 10)
            {
                sb.AppendLine();
                for (int i = 0; i < 10; i++)
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
            }
            else if (index < 10)
            {
                ItemSlot slot = inv[index];
                if (slot.Empty)
                {
                    sb.AppendLine(Lang.Get("Empty"));
                }
                else
                {
                    sb.AppendLine(slot.Itemstack.GetName());
                }
            }
        }
    }
}

