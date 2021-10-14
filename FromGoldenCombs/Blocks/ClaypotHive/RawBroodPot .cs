using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FromGoldenCombs.Blocks
{
    class RawBroodPot : Block
    {
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = "fromgoldencombs:blockhelp-rawclaypothive",
                }
            };

        }

    }
}
