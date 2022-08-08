using FromGoldenCombs.Blocks.Langstroth;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace VFromGoldenCombs.Blocks.Langstroth
{
    class LangstrothBase : LangstrothCore
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty && (int)slot.StorageType == 2)
            {
                ItemStack stack = api.World.BlockAccessor.GetBlock(blockSel.Position, 0).OnPickBlock(api.World, blockSel.Position);
                api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                return byPlayer.InventoryManager.TryGiveItemstack(stack);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        //public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        //{

        //    WorldInteraction[] wi;
        //    WorldInteraction[] wi2;
        //    wi = ObjectCacheUtil.GetOrCreate(api, "baseInteractions", () =>
        //    {
        //        List<ItemStack> partlist = new List<ItemStack>();

        //        partlist.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "langstrothsuper-maple-oak-closed"))));
        //        partlist.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "langstrothbrood-maple-oak-closed"))));
        //        partlist.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "langstrothbase-maple-oak-closed"))));

        //        return new WorldInteraction[] {
        //                    new WorldInteraction() {
        //                            ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper-createstack",
        //                            MouseButton = EnumMouseButton.Right,
        //                            Itemstacks = partlist.ToArray()
        //                    }
        //            };
        //    });

        //    wi2 = ObjectCacheUtil.GetOrCreate(api, "baseInteractions2", () =>
        //    {

        //        return new WorldInteraction[] {
        //                    new WorldInteraction() {
        //                            ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper-closed",
        //                            MouseButton = EnumMouseButton.Right,
        //                            HotKeyCode = "sneak"
        //                    }
        //            };

        //    });

        //    return wi.Append(wi2);
        //}
    }
}
