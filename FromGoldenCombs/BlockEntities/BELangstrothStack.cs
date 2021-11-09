using FromGoldenCombs.Blocks;
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

        internal bool OnInteract(IPlayer byPlayer)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            CollectibleObject colObj = slot.Itemstack?.Collectible;
            bool isSuper = colObj?.Class == "langstrothsuper" && colObj.Variant["open"] == "closed";
            if ((int)slot.StorageType != 2) return false;

            if (slot.Empty)   
            {
                if (TryTake(byPlayer)) //Attempt to take a super from the topmost stack
                                       //if there are multiple stacks on top of each other.
                                       //Or from the topmost occupied slot of this stack.
                {
                    UpdateStackSize();
                    MarkDirty(true);
                    return true;
                }
            } else if (isSuper)
            {
                if (TryPut(slot)) //Attempt to place super either in the current stack,
                                            //any stacks above this, or as a new stack above the
                                            //topmost stack if the block at that position is an air block.
                {
                    UpdateStackSize();
                    MarkDirty(true);
                }
                return true; //This prevents TryPlaceBlock from passing if TryPut fails.
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
            if (stacksize == "zero")
            {

                Api.World.BlockAccessor.SetBlock(0, Pos);

            } 
            ////else if (stacksize == "one")
            //{
            //    ItemStack stack = inv[0].TakeOutWhole();
                
            //    Api.World.BlockAccessor.SetBlock(0, Pos);
            //    Api.World.BlockAccessor.SetBlock(stack.Block.BlockId, Pos);

            //} 
            else
            {
                Api.World.BlockAccessor.ExchangeBlock(Api.World.BlockAccessor
                    .GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-" + stacksize + "-" + this.block.Variant["side"])).BlockId, Pos);
                MarkDirty(true);
            }
            MarkDirty(true);
        }

         private bool TryTake(IPlayer byPlayer)
        {

            //TODO: Restructure code to take top super in any stack, or top index from top stack in a stack of stacks.
            bool isSuccess = false;
            int index = 0;

            while (index < inv.Count - 1 && !inv[index+1].Empty ) //Cycle through indices until reach the top index,
                                                                  //or topmost index with an empty slot over it
            {
                index++;
            }

            bool isTopSlot = index==inv.Count-1; // Check to see if we're accessing the top slot
            bool superAbove = IsSuperAbove(Pos.UpCopy());
            bool airAbove = Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Air;
            if (inv[index].Empty) return isSuccess;

            if (isTopSlot && (!airAbove && !superAbove) || inv[index].Empty) //If the block above isn't air, or another super, of if the target index is empty, fail.
            {
                return isSuccess;
            }
            
            if (superAbove)
            {
                string blockName = Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).FirstCodePart();
                if (blockName == "langstrothsuper")
                {
                   ItemStack super = Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).OnPickBlock(Api.World, Pos.UpCopy());
                   return byPlayer.InventoryManager.TryGiveItemstack(super);
                } else if (blockName == "langstrothstack")
                {
                    BELangstrothStack BELangStack = Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy()) as BELangstrothStack;
                    return BELangStack.RetrieveSuper(byPlayer);
                }
                
            }
            else 
            {
                System.Diagnostics.Debug.WriteLine("Active index is:" + index);
                if (byPlayer.InventoryManager.TryGiveItemstack(inv[index].TakeOutWhole()))
                {
                    isSuccess = true; //isSuccess only equals true ONLY if the above if passes. All other cases its false.
                    //AssetLocation sound = stack.Block?.Sounds?.Place;
                    //Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty(true);
                }
            }
            UpdateStackSize();
            return isSuccess;
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

        private bool TryPut(ItemSlot slot)
        {
            int index = 0;

            while (index < inv.Count - 1 && !inv[index].Empty) //Cycle through indices until reach an empty index, or the top index
            {
                index++;
            }
            //TODO: Add check to determine if the index under the current one is a brood box, and fail TryPut() if it is.

            if (inv[index].Empty) //If the new target index is empty, place a super
            {
                inv[index].Itemstack = slot.TakeOutWhole();
                updateMeshes();
                MarkDirty(true);
                return true;
                System.Diagnostics.Debug.WriteLine(Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).FirstCodePart());
            }
            else if (IsSuperAbove(Pos.UpCopy())) //Otherwise, check to see if the next block up is a Super or SuperStack
            {
                
                if (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).FirstCodePart() == "langstrothstack") //If It's a SuperStack, Send Super To Next Stack
                {
                    (Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy()) as BELangstrothStack).ReceiveSuper(slot); 
                }
                else if (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).FirstCodePart() == "langstrothsuper") //If It's a Super, create a new SuperStack
                {
                                 ItemStack super = block.OnPickBlock(Api.World, Pos.UpCopy());
                    Api.World.BlockAccessor.SetBlock(Api.World.GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-two-" + GetSide(block))).BlockId, Pos.UpCopy());
                    BELangstrothStack lStack = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy());
                    lStack.InitializePut(super, slot);
                    MarkDirty(true);
                }
            }
            else if (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Air)
            {
                Api.World.BlockAccessor.SetBlock(Api.World.GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-one-" + GetSide(block))).BlockId, Pos.UpCopy());
                TryPut(slot);
            }
            UpdateStackSize();
            return true;
        }

        public void ReceiveSuper(ItemSlot slot)
        {
            TryPut(slot);

        }

        public bool RetrieveSuper(IPlayer byPlayer)
        {
            return TryTake(byPlayer);
        }

        private string GetSide(Block block)
        {
            return Api.World.BlockAccessor.GetBlock(block.BlockId).Variant["side"].ToString();
        }
        private bool IsSuperAbove(BlockPos pos)
        {
            string aboveBlockName = Api.World.BlockAccessor.GetBlock(pos).FirstCodePart();

            return (aboveBlockName == "langstrothsuper"
                 || aboveBlockName == "langstrothstack");
        }










        //Rendering Processes
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
