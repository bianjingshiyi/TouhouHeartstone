using System;
using Microsoft.CodeAnalysis.Scripting;

namespace TouhouHeartstone.Backend.Builtin
{
    public class GeneratedEffect : Effect
    {
        public GeneratedEffect(IGameEnvironment env, string pile, string trigger, string script)
        {
            this.pile = pile;
            this.trigger = trigger;
            this.script = env.createScript(script);
        }
        public override string pile { get; }
        public override string trigger { get; }
        Script script { get; }
        public override void execute(CardEngine engine, Player player, Card card, Card[] targetCards)
        {
            try
            {
                script.RunAsync(new EffectGlobals() { engine = engine, player = player, card = card, targetCards = targetCards });
            }
            catch (Exception e)
            {
                throw new ContentException("卡片" + card.define + "的" + trigger + "效果执行失败：\n" + e.ToString());
            }
        }
    }
}