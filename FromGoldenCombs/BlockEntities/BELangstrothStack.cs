using FromGoldenCombs.Blocks;
using FromGoldenCombs.Blocks.Langstroth;
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
            bool isSuper = colObj is LangstrothCore;
            if ((int)slot.StorageType != 2) return false;

            if (slot.Empty)
            {
                if (TryTake(byPlayer)) //Attempt to take a super from the topmost stack
                                       //if there are multiple stacks on top of each other.
                                       //Or from the topmost occupied slot of this stack.
                {
                    //UpdateStackSize();
                    MarkDirty(true);
                    return true;
                }
            } else if (isSuper)
            {
                if (TryPut(slot)) //Attempt to place super either in the current stack,
                                  //any stacks above this, or as a new stack above the
                                  //topmost stack if the block at that position is an air block.
                {
                    //UpdateStackSize();
                    MarkDirty(true);
                }
                return true; //This prevents TryPlaceBlock from passing if TryPut fails.
            }
            return false;
        }

        public bool InitializePut(ItemStack first, ItemSlot slot)
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

                if (index - 1 >= 0 && inv[index - 1].Itemstack.Block is LangstrothBrood)
                {
                    return false;
                }
                else if (index - 1 < 0 && IsLangstrothStackAt(Pos.DownCopy()))
                {
                    BELangstrothStack stack = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(Pos.DownCopy());
                    if (stack.GetStackIndex(2).Block is LangstrothBrood)
                    {
                        Api.World.BlockAccessor.SetBlock(0, Pos);
                        return false;
                    }
                    inv[index].Itemstack = slot.TakeOutWhole();
                    return true;
                }
                inv[index].Itemstack = slot.TakeOutWhole();
                updateMeshes();
                return true;
            }
            else if (IsLangstrothAt(Pos.UpCopy())) //Otherwise, check to see if the next block up is a Super or SuperStack
            {
                if (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()) is LangstrothStack) //If It's a SuperStack, Send To Next Stack
                {
                    System.Diagnostics.Debug.WriteLine("BELangStack Beta-Alpha Reached at POS " + Pos);
                    (Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy()) as BELangstrothStack).ReceiveSuper(slot);
                }
                else if (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()) is LangstrothCore) //If It's a LangstrothCore, create a new LangstrothStack
                {
                    ItemStack langstrothBlock = block.OnPickBlock(Api.World, Pos.UpCopy());
                    Api.World.BlockAccessor.SetBlock(Api.World.GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-two-" + GetSide(block))).BlockId, Pos.UpCopy());
                    BELangstrothStack lStack = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy());
                    lStack.InitializePut(langstrothBlock, slot);
                    MarkDirty(true);
                }
            }
            else if (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Air)
            {
                Api.World.BlockAccessor.SetBlock(Api.World.GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-two-" + GetSide(block))).BlockId, Pos.UpCopy());
                TryPut(slot);
            }
            UpdateStackSize();
            return true;
        }

        //TryTake attemps to retrieve the contents of an Inventory Slot in the stack
        private bool TryTake(IPlayer byPlayer)
        {
            //TODO: Restructure code to take top super in any stack, or top index from top stack in a stack of stacks.
            bool isSuccess = false;
            int index = 0;

            //Cycle through indices until the topmost occupied index that has an empty index over it is reached, or the top index is reached.
            while (index < inv.Count - 1 && !inv[index + 1].Empty) 
            {
                index++;
            }

            // Confirm if this is the top inventory slot of the stack
            bool isTopSlot = index == inv.Count - 1; 
            bool langstrothAbove = IsLangstrothAt(Pos.UpCopy());
            bool airAbove = Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Air;

            // If the index is empty, return isSuccess (False at this point)
            if (inv[index].Empty) return isSuccess; 

            //If the block above isn't air, or another super, of if the target index is empty, return iSSuccess, Still False
            if (isTopSlot && (!airAbove && !langstrothAbove) || inv[index].Empty) 
            {
                return isSuccess;
            }

            //If the above block is of type LangstrothCore
            if (langstrothAbove)
            {
                Block block = Api.World.BlockAccessor.GetBlock(Pos.UpCopy());
                //If it's not a LangstrothStack, take the block
                if (!(block is LangstrothStack) && block is LangstrothCore)
                {
                    ItemStack stack = Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).OnPickBlock(Api.World, Pos.UpCopy());
                    return byPlayer.InventoryManager.TryGiveItemstack(stack);
                }
                //If it is a stack, retrieve the block from the stack
                else if (block is LangstrothStack)
                {
                    BELangstrothStack BELangStack = Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy()) as BELangstrothStack;
                    return BELangStack.RetrieveSuper(byPlayer);
                }

            }
            else
            //Otherwise return the context of the targeted index
            {
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

            //Summary: TryTake grabs the topmost index out of a stack of stacks. In a single stack, it takes the topmost index, or the targeted index if empty.
        }

        private void UpdateStackSize()
        {
            //Update the Stack to match the number of blocks in it.
            int filledstacks = 0;
            string stacksize;
            for (int i = 0; i < inv.Count; i++)
            {
                if (!inv[i].Empty)
                {
                    filledstacks++;
                }
            }

            stacksize = filledstacks == 0 ? "zero" : filledstacks == 1 ? "one" : filledstacks == 2 ? "two" : "three";
            if (stacksize == "zero")
            {
                Api.World.BlockAccessor.SetBlock(0, this.Pos);
            }
            else if (stacksize == "one" && !IsLangstrothAt(Pos.DownCopy())) //If there's only one block left in the stack, and the below stack is a langstroth block
            {
                ItemStack stack = inv[0].TakeOutWhole();
                Api.World.BlockAccessor.SetBlock(Api.World.BlockAccessor.GetBlock(stack.Block.CodeWithVariant("side", GetSide(block))).BlockId, Pos, stack);
            }
            else
            {
                Api.World.BlockAccessor.ExchangeBlock(Api.World.BlockAccessor
                    .GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-" + stacksize + "-" + this.block.Variant["side"])).BlockId, Pos);
            }

            //Summary: UpdateStackSize changes the size of the stack as blocks are added or removed from it.
            //If a single block is left in a stack, the stack is removed and that block placed, provided that the block under the stack is not another stack.
        }

        public void ReceiveSuper(ItemSlot slot)
        {
            //Receive a super from another source for placement.
            //Intended to function as a way for other stacks to send blocks to this stack.
            TryPut(slot);
        }

        public bool RetrieveSuper(IPlayer byPlayer)
        {
            //Receive a call to TryTake from another source for this super.
            //Intended to function as a way for other stacks to take blocks from this stack and give them to the player.
            return TryTake(byPlayer);
        }

        private string GetSide(Block block)
        {
            return Api.World.BlockAccessor.GetBlock(block.BlockId).Variant["side"].ToString();
        }

        public ItemStack GetStackIndex(int i)
        {
            //return the contents of a specific index.
            //Current use is checking to see if top index of a stack is a LangstrothBrood. Intended to prevent adding a LangstrothBlock on top of a broodbox.
            return inv[i].Itemstack;
        }

        //Identify if the block at the given BlockPos is of type LangstrothCore
        private bool IsLangstrothAt(BlockPos pos)
        {
            Block aboveBlockName = Api.World.BlockAccessor.GetBlock(pos);

            return aboveBlockName is LangstrothCore;
        }

        //Identify if the block at the given BlockPos is a LangstrothStack
        private bool IsLangstrothStackAt(BlockPos pos)
        {
        if (IsLangstrothAt(pos) && Api.World.BlockAccessor.GetBlock(pos) is LangstrothStack)
        return true;
        
        return false; 
        }

        private bool ValidHive()
        {
            //Will check size and configuration of stack to determine if it's a valid hive
            return false;
        }

        public bool CanAdd() {
            //If there is a stack below this stack
                //True -> GetStackSize from Stack.Down();
                    
            
            return false;
        }


        //ReturnStackSize
        private int StackSize()
        {
            int filledstacks = 0;
            for (int i = 0; i < inv.Count; i++)
            {
                if (!inv[i].Empty)
                {
                    filledstacks++;
                }
            }
            return filledstacks;
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
