using System;
using Microsoft.CodeAnalysis.Scripting;

namespace TouhouHeartstone.Backend
{
    public class GeneratedEffect : Effect
    {
        string _pile;
        string _trigger;
        string _script;
        public GeneratedEffect(string pile, string trigger, string script)
        {
            _pile = pile;
            _trigger = trigger;
            _script = script;
        }
        public override string pile
        {
            get { return _pile; }
        }
        public override string trigger
        {
            get { return _trigger; }
        }
        public string script
        {
            get { return _script; }
        }
        public void setPile(string pile)
        {
            _pile = pile;
        }
        public void setTrigger(string trigger)
        {
            _trigger = trigger;
        }
        public void setScript(string script)
        {
            _script = script;
        }
        public override void execute(CardEngine engine, Player player, Card card, Card[] targetCards)
        {
            try
            {
                engine.runScript(script, new EffectGlobals() { player = player, card = card, targetCards = targetCards });
            }
            catch (Exception e)
            {
                throw new ContentException("卡片" + card.define + "的" + trigger + "效果执行失败：\n" + e.ToString());
            }
        }
    }
}