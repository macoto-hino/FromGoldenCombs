using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace FromGoldenCombs.Items
{
    class LangstrothPartCore : Item
    {
        public override string GetHeldItemName(ItemStack itemStack)
        {
            
            if (itemStack.Collectible.Variant["accent"] != null)
            {
                return this.VariantStrict["primary"].ToString().UcFirst() + "-" + this.Variant["accent"].ToString().UcFirst() + " " + base.GetHeldItemName(itemStack).ToString();
            }
            else
            {
                return this.VariantStrict["primary"].ToString().UcFirst() + " " + base.GetHeldItemName(itemStack).ToString();
            }

        }
    }
}
