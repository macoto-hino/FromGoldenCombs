﻿using FromGoldenCombs.Blocks;
using FromGoldenCombs.Blocks.Langstroth;
using FromGoldenCombs.config;
using FromGoldenCombs.Items;
using System;
using System.Collections.Generic;
using System.Text;
using FromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{
    class BELangstrothStack : BlockEntityDisplay
    {

        double harvestableAtTotalHours;
        double cooldownUntilTotalHours;
        public bool Harvestable;
        int quantityNearbyFlowers;
        int quantityNearbyHives;
        float actvitiyLevel;
        readonly RoomRegistry roomreg;
        float roomness;
        public static SimpleParticleProperties Bees;
        int scanQuantityNearbyFlowers;
        int scanQuantityNearbyHives;
        int scanIteration;
        EnumHivePopSize hivePopSize;
        int harvestableFrames = 0;
        int linedFrames = 0;
        //string honeyVarietal;
        //List<String> honeyTypeCount = new();
        public readonly InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "langstrothstack";
        private bool isActiveHive = false;

        

        public BELangstrothStack()
        {
            inv = new InventoryGeneric(3, "superslot-0", null, null);
            meshes = new MeshData[3];

        }

        static BELangstrothStack()
        {
            Bees = new SimpleParticleProperties(
                1, 1,
                ColorUtil.ToRgba(255, 215, 156, 65),
                new Vec3d(), new Vec3d(),
                new Vec3f(0, 0, 0),
                new Vec3f(0, 0, 0),
                1f,
                0f,
                0.5f, 0.5f,
                EnumParticleModel.Cube
            );
        }

        public override void Initialize(ICoreAPI api)
        {
            Block block = api.World.BlockAccessor.GetBlock(Pos);
            base.Initialize(api);
            RegisterGameTickListener(TestHarvestable, 6000);
            RegisterGameTickListener(OnScanForFlowers, api.World.Rand.Next(5000) + 30000);

            block = Api.World.BlockAccessor.GetBlock(Pos);

            if (api.Side == EnumAppSide.Client)
            {
                ICoreClientAPI capi = api as ICoreClientAPI;
                Block ownBlock = block;
                Shape shape = capi.Assets.TryGet(new AssetLocation("fromgoldencombs", "shapes/block/hive/langstroth/langstrothstack.json")).ToObject<Shape>();

                if (api.Side == EnumAppSide.Client)
                {
                    RegisterGameTickListener(SpawnBeeParticles, 300);
                }
            }
        }

        internal bool OnInteract(IPlayer byPlayer)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            CollectibleObject colObj = slot.Itemstack?.Collectible;
            bool isLangstroth = colObj is LangstrothCore;
            if ((int)slot.StorageType != 2) return false;

            if (!(slot.Empty) && slot.Itemstack.Collectible is Item) return false;

            if (slot.Empty)
            {
                if (TryTake(byPlayer)) //Attempt to take a super from the topmost stack
                                       //if there are multiple stacks on top of each other.
                                       //Or from the topmost occupied slot of this stack.
                {
                    
                    if (Api.World.BlockAccessor.GetBlock(Pos) is LangstrothStack)
                    {
                        GetBottomStack().isActiveHive = GetBottomStack().IsValidHive();
                        GetBottomStack().ResetHive();
                    }
                    MarkDirty(true);
                    return true;
                }
            }
            else if (isLangstroth && !IsStackFull())
            {
                if (TryPut(slot)) //Attempt to place super either in the current stack,
                                  //any stacks above this, or as a new stack above the
                                  //topmost stack if the block at that position is an air block.
                {
                    GetBottomStack().isActiveHive = GetBottomStack().IsValidHive();
                    GetBottomStack().ResetHive();
                    MarkDirty(true);
                }

                return true; //This prevents TryPlaceBlock from passing if TryPut fails.
            }
            return false;
        }

        //UpdateFrames cycles through the stack checking for frames that lined to update to Harvestable
        //Will do as many frames as fillframes
        private void UpdateFrames(int fillframes)
        {
            BELangstrothStack curBE = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(GetTopStack().Pos);

            while (curBE is BELangstrothStack)
            {
                for (int index = 2; index >= 0; index--)
                {

                    if (curBE.inv[index].Itemstack != null && curBE.inv[index].Itemstack.Collectible is LangstrothSuper && curBE.inv[index].Itemstack.Attributes.GetTreeAttribute("contents") != null)
                    {
                        ITreeAttribute contents = curBE.inv[index].Itemstack.Attributes.GetTreeAttribute("contents");
                        int contentsSize = contents.Count;

                        for (int j = 0; j <= contentsSize && fillframes > 0; j++)
                        {
                            ItemStack stack = contents.GetItemstack((j - 1).ToString());
                            if (stack?.Collectible is LangstrothFrame)
                            {
                                if (stack.Collectible.Variant["harvestable"] == "lined")
                                {
                                    int durability = stack.Attributes.GetInt("durability")==0?FromGoldenCombsConfig.Current.baseframedurability:stack.Attributes.GetInt("durability");
                                    stack = new ItemStack(Api.World.GetItem(stack.Collectible.CodeWithVariant("harvestable", "harvestable")), 1);
                                    stack.Attributes.SetInt("durability", durability);
                                    fillframes--;
                                }
                                curBE.inv[index].Itemstack.Attributes.GetTreeAttribute("contents").SetItemstack((j - 1).ToString(), stack);
                            }
                            curBE.inv[index].MarkDirty();
                        }
                    }
                }
                curBE = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(curBE.Pos.DownCopy());
            }

        }

        private int CountHarvestable()
        {

            BELangstrothStack topStack = GetTopStack();
            BELangstrothStack bottomStack = GetBottomStack();
            BELangstrothStack curBE = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(topStack.Pos);
                bottomStack.harvestableFrames = 0;

                while (curBE is BELangstrothStack)
                {
                    for (int index = 2; index >= 0; index--)
                    {

                        if (curBE.inv[index].Itemstack != null && curBE.inv[index].Itemstack.Collectible is LangstrothSuper && curBE.inv[index].Itemstack.Attributes.GetTreeAttribute("contents") != null)
                        {
                            ITreeAttribute contents = curBE.inv[index].Itemstack.Attributes.GetTreeAttribute("contents");
                            int contentsSize = contents.Count;

                            for (int j = 0; j <= contentsSize; j++)
                            {
                                ItemStack stack = contents.GetItemstack((j - 1).ToString());
                                if (stack?.Collectible is LangstrothFrame)
                                {
                                    if (stack.Collectible.Variant["harvestable"] == "harvestable")
                                    {
                                        bottomStack.harvestableFrames++;
                                    }

                                }
                                curBE.inv[index].MarkDirty();
                            }
                        }
                    }
                    curBE = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(curBE.Pos.DownCopy());
                }
                return harvestableFrames;
         }

            private int CountLinedFrames()
        {

            BELangstrothStack topStack = GetTopStack();
            BELangstrothStack bottomStack = GetBottomStack();
            BELangstrothStack curBE = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(topStack.Pos);
            bottomStack.linedFrames = 0;

            while (curBE is BELangstrothStack)
            {
                for (int index = 2; index >= 0; index--)
                {

                    if (curBE.inv[index].Itemstack != null && curBE.inv[index].Itemstack.Collectible is LangstrothSuper && curBE.inv[index].Itemstack.Attributes.GetTreeAttribute("contents") != null)
                    {
                        ITreeAttribute contents = curBE.inv[index].Itemstack.Attributes.GetTreeAttribute("contents");
                        int contentsSize = contents.Count;

                        for (int j = 0; j <= contentsSize; j++)
                        {
                            ItemStack stack = contents.GetItemstack((j - 1).ToString());
                            if (stack?.Collectible is LangstrothFrame)
                            {
                                if (stack.Collectible.Variant["harvestable"] == "lined")
                                {
                                    bottomStack.linedFrames++;
                                }

                            }
                            curBE.inv[index].MarkDirty();
                        }
                    }
                }
                curBE = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(curBE.Pos.DownCopy());
            }

            return linedFrames;
        }

        public bool InitializePut(ItemStack first, ItemSlot slot)
        {
            if (slot.Itemstack.Collectible is Item) return false;
            inv[0].Itemstack = first;
            inv[1].Itemstack = slot.TakeOutWhole();
            UpdateStackSize();
            updateMeshes();
            CountHarvestable();
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
            //If the new target index is empty, place a super
            if (inv[index].Empty)
            {
                if (index - 1 >= 0 && inv[index - 1].Itemstack.Collectible is LangstrothBrood) return false;
                return placeLangstrothBlock(index, slot);
            }
            //Otherwise, we're at the top block. Check that it isn't a brood box and see if there is a langstroth stack above this stack.
            else if (!(inv[index].Itemstack.Collectible is LangstrothBrood))
            {

                if (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()) is LangstrothStack)
                {
                    (Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy()) as BELangstrothStack).ReceiveSuper(slot);
                };

                if (Api.World.BlockAccessor.GetBlock(Pos.UpCopy()).BlockMaterial == EnumBlockMaterial.Air)
                {
                    Api.World.BlockAccessor.SetBlock(Api.World.
                    GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-two-" + GetSide(this.Block))).BlockId, Pos.UpCopy());
                    TryPut(slot);
                }
            }
            UpdateStackSize();
            return true;
        }

        private bool placeLangstrothBlock(int index, ItemSlot slot)
        {
                inv[index].Itemstack = slot.TakeOutWhole();
                updateMeshes();
                return true;
        }

        private void createNewStackUp(ItemSlot slot)
        {
            ItemStack langstrothBlock = this.Block.OnPickBlock(Api.World, Pos.UpCopy());
            Api.World.BlockAccessor.SetBlock(Api.World.GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-two-" + GetSide(this.Block))).BlockId, Pos.UpCopy());
            BELangstrothStack lStack = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(Pos.UpCopy());
            lStack.InitializePut(langstrothBlock, slot);
            MarkDirty(true);
        }

        //TryTake attemps to retrieve the contents of an Inventory Slot in the stack
        private bool TryTake(IPlayer byPlayer)
        {
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
                 if (block is LangstrothStack)
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

            //Summar"y": TryTake grabs the topmost index out of a stack of stacks. In a single stack, it takes the topmost index, or the targeted index if empty.
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
            else if (stacksize == "one" && !IsLangstrothAt(Pos.DownCopy())) //If there's only one block left in the stack, and there isn't a Langstroth stack below.
            {
                Api.World.BlockAccessor.SetBlock(Api.World.BlockAccessor.
                    GetBlock(inv[0].Itemstack.Block.CodeWithVariant("side", GetSide(this.Block))).BlockId, Pos, inv[0].Itemstack);
            }
            else
            {
                Api.World.BlockAccessor.ExchangeBlock(Api.World.BlockAccessor
                    .GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-" + stacksize + "-" + 
                    this.Block.Variant["side"])).BlockId, Pos);
            }

            //Summary: UpdateStackSize changes the size of the stack as blocks are added or removed from it.
            //If a single block is left in a stack, and the below block is not a stack, the stack is removed and that block placed in its stead.
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

        public void UpdateOnNeighborBreak()
        {
            GetBottomStack().isActiveHive = IsValidHive();
        }

        private bool IsValidHive()
        {
            BELangstrothStack topStack = GetTopStack();
            BELangstrothStack bottomStack = GetBottomStack();
            CountHarvestable();
           //Check bottomStack's bottom index for a LangstrothBase
            if (!(bottomStack.inv[0].Itemstack.Collectible is LangstrothBase)) {
                ResetHive();
                return false;
            }     

        //Check topStack's top Index for populated brood box
            Block topBlock = topStack?.inv[topStack.StackSize() - 1 >= 0 ? topStack.StackSize() - 1 : 0].Itemstack.Block;
            if (!(topBlock is LangstrothBrood && topBlock.Variant["populated"] == "populated")){
                ResetHive();
                return false;
            }

            //Check the rest of the hive for anything not a super
            return CheckForNonSuper();
        }

        private bool CheckForNonSuper()
        {
            BELangstrothStack topStack = GetTopStack();
            BELangstrothStack bottomStack = GetBottomStack();
            BELangstrothStack curBE = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(GetTopStack().Pos.DownCopy());
            int downCount = 1;

            if (topStack.Pos != bottomStack.Pos)
            {
                for (int i = topStack.StackSize() - 2; i >= 0; i--)
                {
                    if (!(topStack.inv[i].Itemstack.Collectible is LangstrothSuper)) {
                        ResetHive(); ;
                    return false;
                    }
                }

                while (curBE is BELangstrothStack)
                {
                    for (int index = 2; index >= 0 && !(curBE.Pos == bottomStack.Pos && index == 0); index--)
                    {
                        if (!(curBE.inv[index].Itemstack.Collectible is LangstrothSuper)) {
                            ResetHive();
                            return false;
                        }
                    }
                    downCount++;
                    curBE = (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(topStack.Pos.DownCopy(downCount));
                }
            } else if ((topStack.inv[2].Itemstack.Collectible is LangstrothBrood && topStack.inv[2].Itemstack.Collectible.Variant["populated"] == "populated")
                        && topStack.inv[0].Itemstack.Collectible is LangstrothBase)
            {
                return true;
            }
            return true;
        }


        public bool IsStackFull()
        {
            //If there is a stack below this stack
            //True -> GetStackSize from Stack.Down();
            int maxStackSize = FromGoldenCombsConfig.Current.MaxStackSize;
            if (TotalStackSize() >= maxStackSize)
                return true;

            return false;
        }


        //ReturnStackSize
        public int StackSize()
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

        //Return total number of Supers in the Stack
        public int TotalStackSize()
        {

            int totalStackSize = GetTopStack().StackSize();
            BlockPos TopStack = GetTopStack().Pos;
            int downCount = 1;
            while (Api.World.BlockAccessor.GetBlockEntity(TopStack.DownCopy(downCount)) is BELangstrothStack i)
            {
                totalStackSize += i.StackSize();
                downCount++;
            }
            return totalStackSize;
        }

        //Return Top Stack of Stack
        public BELangstrothStack GetTopStack()
        {
            BlockPos topPos = Pos;
            int upCount = 1;
            while (Api.World.BlockAccessor.GetBlock(Pos.UpCopy(upCount)) is LangstrothStack)
            {
                topPos = Pos.UpCopy(upCount);
                upCount++;
            }
            return (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(topPos);
        }

        public BELangstrothStack GetBottomStack()
        {
            BlockPos bottomPos = Pos;
            int downCount = 1;

            while (Api.World.BlockAccessor.GetBlock(Pos.DownCopy(downCount)) is LangstrothStack)
            {
                bottomPos = Pos.DownCopy(downCount);
                downCount++;
            }

            return (BELangstrothStack)Api.World.BlockAccessor.GetBlockEntity(bottomPos);
        }

        //Rendering Processes
        readonly Matrixf mat = new();

        protected override MeshData genMesh(ItemStack stack)
        {
            MeshData mesh;

            ICoreClientAPI capi = Api as ICoreClientAPI;
            mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
            nowTesselatingObj = stack.Collectible;
            nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Block.Shape.Base);
            mesh.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);

            return mesh;
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
            float x = 0;
            float y = .3333f * index;
            float z = 0;
            Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
            //This seems to work for rotating the actual appearance of the blocks in the itemslots.
            mesh.Rotate(new Vec3f(0.5f, 0f, 0.5f), 0f, this.Block != null ? this.Block.Shape.rotateY * GameMath.DEG2RAD : 0f, 0f);
            mesh.Translate(offset.XYZ);
        }

        

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
                mat.Identity();
                mat.RotateYDeg(this.Block.Shape.rotateY);
                return base.OnTesselation(mesher, tessThreadTesselator);
        }

        //Active Hive Methods/Fields
        readonly Vec3d startPos = new();
        readonly Vec3d endPos = new();
        Vec3f minVelo = new();

        private void SpawnBeeParticles(float dt)
        {
            if (isActiveHive && Pos == GetBottomStack().Pos)
            {
                float dayLightStrength = Api.World.Calendar.GetDayLightStrength(Pos.X, Pos.Z);
                if (Api.World.Rand.NextDouble() > 2 * dayLightStrength - 0.5) return;

                Random rand = Api.World.Rand;

                Bees.MinQuantity = actvitiyLevel;

                // Leave hive
                if (Api.World.Rand.NextDouble() > 0.5)
                {
                    startPos.Set(Pos.X + 0.5f, Pos.Y + 0.5f, Pos.Z + 0.5f);
                    minVelo.Set((float)rand.NextDouble() * 3 - 1.5f, (float)rand.NextDouble() * 1 - 0.5f, (float)rand.NextDouble() * 3 - 1.5f);

                    Bees.MinPos = startPos;
                    Bees.MinVelocity = minVelo;
                    Bees.LifeLength = 1f;
                    Bees.WithTerrainCollision = false;
                }

                // Go back to hive
                else
                {
                    startPos.Set(Pos.X + rand.NextDouble() * 5 - 2.5, Pos.Y + rand.NextDouble() * 2 - 1f, Pos.Z + rand.NextDouble() * 5 - 2.5f);
                    endPos.Set(Pos.X + 0.5f, Pos.Y + 0.5f, Pos.Z + 0.5f);

                    minVelo.Set((float)(endPos.X - startPos.X), (float)(endPos.Y - startPos.Y), (float)(endPos.Z - startPos.Z));
                    minVelo /= 2;

                    Bees.MinPos = startPos;
                    Bees.MinVelocity = minVelo;
                    Bees.WithTerrainCollision = true;
                    Api.World.SpawnParticles(Bees);
                }
            }
        }

        public void ResetHive()
        {
            GetBottomStack().harvestableAtTotalHours = 0;
            GetBottomStack().quantityNearbyFlowers = 0;
            GetBottomStack().hivePopSize = 0;
        }


        private void TestHarvestable(float dt)
        {
            if (isActiveHive && (Pos == GetBottomStack().Pos))
            {
                int harvestBase = FromGoldenCombsConfig.Current.langstrothHiveHoursToHarvest;
                double worldTime = Api.World.Calendar.TotalHours;
                ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
                if (conds == null) return;

                float temp = conds.Temperature + (roomness > 0 ? 5 : 0);
                actvitiyLevel = GameMath.Clamp(temp / 5f, 0f, 1f);

                // Reset timers during winter
                if (temp <= -10)
                {
                    //TODO: Readdress harvestAtTotalHours math to ensure it works for all ranges of growth time.
                    harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
                    cooldownUntilTotalHours = worldTime + 4 / 2 * 24;
                }
                if (CountLinedFrames() <= 0)
                {
                    cooldownUntilTotalHours = worldTime + 4 / 2 * 24;
                }
                if (!Harvestable && harvestableAtTotalHours == 0 && hivePopSize > EnumHivePopSize.Poor && CountLinedFrames()>0)
                {
                    harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
                }
                else if (!Harvestable && worldTime > harvestableAtTotalHours && hivePopSize > EnumHivePopSize.Poor && CountLinedFrames() > 0)
                {
                    GetTopStack().UpdateFrames(((int)hivePopSize));
                    harvestableAtTotalHours = worldTime + HarvestableTime(harvestBase);
                    MarkDirty(true);
                    CountHarvestable();
                }
            }
        }

        private double HarvestableTime(int i)
        {
            Random rand = new();
            return (i * .75) + ((i * .5) * rand.NextDouble());
        }


        private void OnScanForFlowers(float dt)
        {
            if (isActiveHive && (Pos == GetBottomStack().Pos))
            {
                //Scan to get number of nearby flowers and active hives
                Room room = roomreg?.GetRoomForPosition(Pos);
                roomness = (room != null && room.SkylightCount > room.NonSkylightCount && room.ExitCount == 0) ? 1 : 0;
                List<String> flowerList = new();

                if (actvitiyLevel < 1) return;
                if (Api.Side == EnumAppSide.Client) return;
                if (Api.World.Calendar.TotalHours < cooldownUntilTotalHours) return;

                if (scanIteration == 0)
                {
                    scanQuantityNearbyFlowers = 0;
                    scanQuantityNearbyHives = 0;
                }

                int minX = -8 + 8 * (scanIteration / 2);
                int minZ = -8 + 8 * (scanIteration % 2);
                int size = 8;

                Block fullSkepN = Api.World.GetBlock(new AssetLocation("skep-populated-north"));
                Block fullSkepE = Api.World.GetBlock(new AssetLocation("skep-populated-east"));
                Block fullSkepS = Api.World.GetBlock(new AssetLocation("skep-populated-south"));
                Block fullSkepW = Api.World.GetBlock(new AssetLocation("skep-populated-west"));

                Block wildhive1 = Api.World.GetBlock(new AssetLocation("wildbeehive-medium"));
                Block wildhive2 = Api.World.GetBlock(new AssetLocation("wildbeehive-large"));

                Block claypothive = Api.World.GetBlock(new AssetLocation("claypothive-populated-empty-withtop"));
                Block claypothive2 = Api.World.GetBlock(new AssetLocation("claypothive-populated-empty-notop"));
                Block claypothive3 = Api.World.GetBlock(new AssetLocation("claypothive-populated-harvestable-notop"));
                Block claypothive4 = Api.World.GetBlock(new AssetLocation("claypothive-populated-harvestable-withtop"));

                Block langstrothstacke = Api.World.GetBlock(new AssetLocation("langstrothstack-one-east"));
                Block langstrothstackn = Api.World.GetBlock(new AssetLocation("langstrothstack-one-north"));
                Block langstrothstacks = Api.World.GetBlock(new AssetLocation("langstrothstack-one-south"));
                Block langstrothstackw = Api.World.GetBlock(new AssetLocation("langstrothstack-one-west"));

                Block langstrothstack2e = Api.World.GetBlock(new AssetLocation("langstrothstack-two-east"));
                Block langstrothstack2n = Api.World.GetBlock(new AssetLocation("langstrothstack-two-north"));
                Block langstrothstack2s = Api.World.GetBlock(new AssetLocation("langstrothstack-two-south"));
                Block langstrothstack2w = Api.World.GetBlock(new AssetLocation("langstrothstack-two-west"));

                Block langstrothstack3e = Api.World.GetBlock(new AssetLocation("langstrothstack-three-east"));
                Block langstrothstack3n = Api.World.GetBlock(new AssetLocation("langstrothstack-three-north"));
                Block langstrothstack3s = Api.World.GetBlock(new AssetLocation("langstrothstack-three-south"));
                Block langstrothstack3w = Api.World.GetBlock(new AssetLocation("langstrothstack-three-west"));


                Api.World.BlockAccessor.WalkBlocks(Pos.AddCopy(minX, -5, minZ), Pos.AddCopy(minX + size - 1, 5, minZ + size - 1), (block, pos) =>
                {
                    if (block.Id == 0) return;

                    if (block.Attributes != null && block.Attributes.IsTrue("beeFeed"))
                    {
                        flowerList.Add(block.Variant["flower"]);
                        scanQuantityNearbyFlowers++;
                    };

                    if (block == fullSkepN || block == fullSkepE || block == fullSkepS || block == fullSkepW
                    || block == wildhive1 || block == wildhive2
                    || block == claypothive || block == claypothive2 || block == claypothive3 || block == claypothive4
                    || block == langstrothstacke || block == langstrothstackn || block == langstrothstacks || block == langstrothstackw
                    || block == langstrothstack2e || block == langstrothstack2n || block == langstrothstack2s || block == langstrothstack2w
                    || block == langstrothstack3e || block == langstrothstack3n || block == langstrothstack3s || block == langstrothstack3w)
                    {
                        scanQuantityNearbyHives++;
                    }
                });

                scanIteration++;

                if (scanIteration == 4)
                {
                    scanIteration = 0;
                    OnScanComplete();
                }
            }
        }

        private void OnScanComplete()
        {
            quantityNearbyFlowers = scanQuantityNearbyFlowers;
            quantityNearbyHives = scanQuantityNearbyHives;
            hivePopSize = (EnumHivePopSize)GameMath.Clamp(quantityNearbyFlowers - 6 * quantityNearbyHives, 0, 2);
            MarkDirty();
        }

        //private String GetHoneyVarietal(String[] flowers, int n)
        //{
        //    // Sort the array
        //    Array.Sort(flowers);

        //    // find the max frequency using
        //    // linear traversal
        //    int max_count = 1;
        //    String flowerSort = flowers[0];
        //    int curr_count = 1;

        //    for (int i = 1; i < n; i++)
        //    {
        //        if (flowers[i] == flowers[i - 1])
        //            curr_count++;
        //        else
        //        {
        //            if (curr_count > max_count)
        //            {
        //                max_count = curr_count;
        //                flowerSort = flowers[i - 1];
        //            }
        //            curr_count = 1;
        //        }
        //    }

        //    // If last element is most frequent
        //    if (curr_count > quantityNearbyFlowers || max_count > quantityNearbyFlowers)
        //    {
        //        if (curr_count >= max_count)
        //        {
        //            flowerSort = flowers[n - 1];
        //        }
        //        return flowerSort;
        //    }
        //    return "blended";
        //}


//Misc Methods

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);


            tree.SetInt("scanIteration", scanIteration);

            tree.SetInt("quantityNearbyFlowers", quantityNearbyFlowers);
            tree.SetInt("quantityNearbyHives", quantityNearbyHives);


            tree.SetInt("scanQuantityNearbyFlowers", scanQuantityNearbyFlowers);
            tree.SetInt("scanQuantityNearbyHives", scanQuantityNearbyHives);

            tree.SetInt("harvestable", Harvestable ? 1 : 0);
            tree.SetDouble("cooldownUntilTotalHours", cooldownUntilTotalHours);
            tree.SetDouble("harvestableAtTotalHours", harvestableAtTotalHours);
            tree.SetInt("hiveHealth", (int)hivePopSize);
            tree.SetFloat("roomness", roomness);
            tree.SetInt("harvestableFrames", harvestableFrames);
            tree.SetBool("activeHive", isActiveHive);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            bool wasHarvestable = Harvestable;

            scanIteration = tree.GetInt("scanIteration");
            harvestableFrames = tree.GetInt("harvestableFrames");
            quantityNearbyFlowers = tree.GetInt("quantityNearbyFlowers");
            quantityNearbyHives = tree.GetInt("quantityNearbyHives");
            isActiveHive = tree.GetBool("activeHive");
            scanQuantityNearbyFlowers = tree.GetInt("scanQuantityNearbyFlowers");
            scanQuantityNearbyHives = tree.GetInt("scanQuantityNearbyHives");

            Harvestable = tree.GetInt("harvestable") > 0;
            cooldownUntilTotalHours = tree.GetDouble("cooldownUntilTotalHours");
            harvestableAtTotalHours = tree.GetDouble("harvestableAtTotalHours");
            hivePopSize = (EnumHivePopSize)tree.GetInt("hiveHealth");
            roomness = tree.GetFloat("roomness");

            if (Harvestable != wasHarvestable && Api != null)
            {
                MarkDirty(true);
            }
        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            BELangstrothStack bottomStack = GetBottomStack();
            if (forPlayer.CurrentBlockSelection == null)
            {


                base.GetBlockInfo(forPlayer, sb);

            }
            else if (Pos == bottomStack.Pos)
            {
                if (bottomStack.harvestableFrames != 0) { sb.AppendLine("Harvestable Frames: " + bottomStack.harvestableFrames); }
                sb.AppendLine(bottomStack.isActiveHive ? "The hive buzzes busily." : "");

                double worldTime = Api.World.Calendar.TotalHours;
                int daysTillHarvest = (int)Math.Round((bottomStack.harvestableAtTotalHours - worldTime) / 24);
                daysTillHarvest = daysTillHarvest <= 0 ? 0 : daysTillHarvest;
                string hiveState = Lang.Get("Nearby flowers: {0}\nPopulation Size: {1}", bottomStack.quantityNearbyFlowers, bottomStack.hivePopSize);
                if (bottomStack.isActiveHive)
                {
                    sb.AppendLine(hiveState);
                    if ((bottomStack.harvestableAtTotalHours - worldTime / 24) > 0 && CountLinedFrames() > 0)
                    {
                        string combPopTime;
                        if (FromGoldenCombsConfig.Current.showcombpoptime)
                        {
                            combPopTime = "Your bees will produce comb in " + (daysTillHarvest < 1 ? "less than one day" : daysTillHarvest + " days");
                        }
                        else
                        {
                            combPopTime = "The bees are out gathering";
                        }
                        sb.AppendLine(combPopTime);
                    }
                    else if (daysTillHarvest == 0 && CountLinedFrames() == 0)
                    {
                        sb.AppendLine("Hive lacks fillable frames.");
                    }
                    else
                    {
                        sb.AppendLine("The bees are out scouting for flowers.");
                    }
                }
            } else
            {
                bottomStack.GetBlockInfo(forPlayer, sb);
            }
        }
    }
}
