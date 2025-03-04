using System;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class CustomPropertyModifier : PropModifier<object>
    {
        #region 公有方法
        #region 构造方法
        public CustomPropertyModifier(string propName, object value)
        {
            propertyName = propName;
            defaultValue = value;
        }
        public CustomPropertyModifier(string propName, object value, bool isSet)
        {
            propertyName = propName;
            defaultValue = value;
            this.isSet = isSet;
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
        public override PropModifier Clone()
        {
            return new CustomPropertyModifier(propertyName, defaultValue is ICloneable cloneable ? cloneable.Clone() : defaultValue, isSet);
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