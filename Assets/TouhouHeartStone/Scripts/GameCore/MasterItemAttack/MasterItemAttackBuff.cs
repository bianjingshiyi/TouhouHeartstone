using System.Collections.Generic;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class MasterItemAttackBuffDefine : BuffDefine, IInnateBuffDefine
    {
        public MasterItemAttackBuffDefine() : base(buffDefineRef.defineId)
        {
            cardPoolId = buffDefineRef.cardPoolId;
            propModifierList.Add(new AttackModifier(0) { relatedPropName = buffPropName });
            _effects.Add(new MasterItemAttackEffectPropChanged(buffDefineRef));
            _effects.Add(new MasterItemAttackEffectSetDefine(buffDefineRef));
            _effects.Add(new MasterItemAttackEffectMoveCard(buffDefineRef));
        }
        public override IEffect[] getEffects()
        {
            return _effects.ToArray();
        }
        public List<IEffect> _effects = new List<IEffect>();
        public const string buffPropName = "itemAttack";
        public static readonly DefineReference buffDefineRef = new DefineReference(0, 1);
    }
}
