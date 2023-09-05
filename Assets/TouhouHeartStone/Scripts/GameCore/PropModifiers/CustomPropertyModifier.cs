using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class CustomPropertyModifier : PropModifier<object>
    {
        #region 公有方法
        #region 构造方法
        public CustomPropertyModifier(string propName, int value)
        {
            propertyName = propName;
            this.value = value;
        }
        public CustomPropertyModifier(string propName, int value, bool isSet)
        {
            propertyName = propName;
            this.value = value;
            this.isSet = isSet;
        }
        public CustomPropertyModifier(string propName, string relatedPropName, bool isSet)
        {
            propertyName = propName;
            this.relatedPropName = relatedPropName;
            this.isSet = isSet;
        }
        protected CustomPropertyModifier(CustomPropertyModifier origin)
        {
            propertyName = origin.getPropName();
            value = origin.value;
            isSet = origin.isSet;
            relatedPropName = origin.relatedPropName;
        }
        #endregion
        public override object calcGeneric(IGame game, Card card, object value)
        {
            if (isSet)
                value = this.value;
            else
            {
                if (this.value is int thisIntValue)
                {
                    if (value is int intValue)
                    {
                        value = intValue + thisIntValue;
                    }
                    else if (value == null)
                    {
                        value = thisIntValue;
                    }
                }
            }
            return value;
        }
        public override PropModifier clone()
        {
            return new CustomPropertyModifier(this);
        }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(relatedPropName))
            {
                return getPropName() + "=<" + relatedPropName + ">";
            }
            else if (isSet)
                return getPropName() + "=" + value;
            else
                return getPropName() + "+" + value;
        }
        #endregion
        #region 属性字段
        public bool isSet;
        #endregion
    }
}