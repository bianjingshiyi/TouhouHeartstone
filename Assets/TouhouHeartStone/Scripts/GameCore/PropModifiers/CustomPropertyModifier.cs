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
            defaultValue = value;
        }
        public CustomPropertyModifier(string propName, int value, bool isSet)
        {
            propertyName = propName;
            defaultValue = value;
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
            defaultValue = origin.defaultValue;
            isSet = origin.isSet;
            relatedPropName = origin.relatedPropName;
        }
        #endregion
        public override object calcGeneric(IGame game, ICardData card, object prop, object value)
        {
            if (isSet)
                prop = value;
            else
            {
                if (value is int thisIntValue)
                {
                    if (prop is int intValue)
                    {
                        prop = intValue + thisIntValue;
                    }
                    else if (prop == null)
                    {
                        prop = thisIntValue;
                    }
                }
            }
            return prop;
        }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(relatedPropName))
            {
                return $"{getPropName()}=<{relatedPropName}>";
            }
            else
            {
                var value = defaultValue;
                if (isSet)
                    return $"{getPropName()}={value}";
                else
                    return $"{getPropName()}+{value}";
            }
        }
        #endregion
        #region 属性字段
        public bool isSet;
        #endregion
    }
}