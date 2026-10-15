using Il2Cpp;
using Il2CppGame;
using Il2CppGRNetwork;

namespace DeathNoteCheaterWithin.Features.Misc
{
    public static class ItemSpawner
    {
        public static void SpawnItem(EItemType itemType)
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.MyPlayer == null) return;

            PlayerMovement myPlayer = gsm.MyPlayer;
            ItemData itemData = ResourceManager.ItemData.GetItemDataByType(itemType);
            if (itemData != null)
            {
                itemData.ShowsInList = true;
                itemData.CanGive = true;
                itemData.OnlyGiveBetweenKillers = false;

                if ((int)itemType == 1) // Death Note
                {
                    if (myPlayer.playerStatus != null && myPlayer.playerStatus.itemList != null)
                    {
                        myPlayer.playerStatus.itemList.Add(itemData);
                    }
                    itemData = ResourceManager.ItemData.GetItemDataByType((EItemType)2);
                }

                myPlayer.AddItem(itemData, myPlayer, (EMsgNoticeType)1);
            }
        }
    }
}
