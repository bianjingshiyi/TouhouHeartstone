using System;
using Microsoft.CodeAnalysis.Scripting;

namespace TouhouHeartstone.Backend
{
    public class GeneratedEffect : Effect
    {
        string _pile;
        string _trigger;
        public override string pile
        {
            get { return _pile; }
        }
        public override string trigger
        {
            get { return _trigger; }
        }
        public string filterScript { get; private set; }
        public string actionScript { get; private set; }
        public GeneratedEffect(string pile, string trigger, string filterScript, string actionScript)
        {
            _pile = pile;
            _trigger = trigger;
            this.filterScript = filterScript;
            this.actionScript = actionScript;
        }
        public void setPile(string pile)
        {
            _pile = pile;
        }
        public void setTrigger(string trigger)
        {
            _trigger = trigger;
        }
        public void setFilterScript(string script)
        {
            filterScript = script;
        }
        public void setActionScript(string script)
        {
            actionScript = script;
        }
        public override bool checkTarget(CardEngine engine, Player player, Card card, Card[] targetCards)
        {
            if (string.IsNullOrEmpty(filterScript))
                return true;
            try
            {
                return engine.runFunc<bool>(filterScript, new EffectGlobals() { engine = engine, player = player, card = card, targetCards = targetCards });
            }
            catch (Exception e)
            {
                throw new ContentException("卡片" + card.define + "的" + trigger + "效果执行失败：\n" + e.ToString());
            }
        }
        public override void execute(CardEngine engine, Player player, Card card, Card[] targetCards)
        {
            try
            {
                engine.runAction(actionScript, new EffectGlobals() { engine = engine, player = player, card = card, targetCards = targetCards });
            }
            catch (Exception e)
            {
                throw new ContentException("卡片" + card.define + "的" + trigger + "效果执行失败：\n" + e.ToString());
            }
        }
    }
}