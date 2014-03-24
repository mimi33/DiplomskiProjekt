using System;
using System.Reflection;

namespace DiplomskiProjekt.Classes
{
    //string valueOfAuthenticationMethod = StringEnum.GetStringValue(AuthenticationMethod.FORMS);
    public class StringValue : Attribute
    {
        public StringValue(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }
    }

    public enum NodeType
    {
        [StringValue("Evaluation")]
        Evaluation,
        [StringValue("Tree")]
        Tree,
        [StringValue("Crossover")]
        Crossover,
        [StringValue("Mutation")]
        Mutation,
        [StringValue("Algorithm")]
        Algorithm,
        [StringValue("Log")]
        Log
    }

    public static class StringEnum
    {
        public static string GetStringValue(this Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            var attrs = fi.GetCustomAttributes(typeof(StringValue), false) as StringValue[];
            if (attrs != null && attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(StringValue)) as StringValue;
                if (attribute == null) 
                    continue;

                if (attribute.Value == description)
                    return (T)field.GetValue(null);
            }
            throw new ArgumentException("Not found.", "description");
        }
    }
}
