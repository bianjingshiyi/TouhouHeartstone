using UnityEngine;
using TouhouHeartstone;
using UI;
using Card = UI.Card;
using TouhouCardEngine;
using System.Collections;

namespace Game
{
    using Arg = THHPlayer.DiscoverEventArg;
    public class DiscoverAnim : EventAnimation<Arg>
    {
        TableAnimation _anim;
        public override bool forceQueueAnimations => true;
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;
            var defines = arg.defines;

            if (player == table.player)
            {
                yield break;
            }

            table.ui.Waiting.SetActive(true);

            while (!record.isCompleted)
                yield return null;

            table.ui.Waiting.SetActive(false);

            if (record.isCanceled)
            {
                yield break;
            }
            var discovered = table.getRecordCardSnapshot(record, EVENTVAR.CHOSEN_CARD);

            Card discoveredUI = null;
            var isShow = arg.isShow;
            if (isShow)
            {
                discoveredUI = table.createInvokeCard(discovered, table.ui.LeftDisplayImage.rectTransform.position, table.cardPrefab.rectTransform.rect.height * table.ui.SelfHandList.rectTransform.localScale.y * 1.2f, isShow);
                discoveredUI.asAnimator.SetBool("isFaceUp", isShow);
            }
            if (discoveredUI != null)
            {
                while (!SimpleAnimHelper.update(table, ref _anim, discoveredUI.ShowAnim, discoveredUI.animator, _time))
                    yield return null;
                Object.Destroy(discoveredUI.gameObject);
            }
        }
    }

}