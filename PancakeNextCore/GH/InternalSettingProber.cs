using Grasshopper2;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH;
internal interface ISettingAccessor
{
    string GetDefaultString();
    string GetString();
    bool SetByString(string str);
    string InConfigName { get; }
    string PropertyName { get; }
    public Delegate[] GetInvocationList();
}
internal static class InternalSettingProber
{
    private static class SettingAccessorFactory
    {
        public static ISettingAccessor Create(FieldInfo pi)
        {
            try
            {
                var type = pi.FieldType;
                if (type == typeof(Settings.Setting<string>))
                {
                    return new StringSetting(pi);
                }
                else if (type == typeof(Settings.Setting<int>))
                {
                    return new IntSetting(pi);
                }
                else if (type == typeof(Settings.Setting<float>))
                {
                    return new FloatSetting(pi);
                }
                else if (type == typeof(Settings.Setting<double>))
                {
                    return new DoubleSetting(pi);
                }
                else if (type == typeof(Settings.Setting<bool>))
                {
                    return new BoolSetting(pi);
                }
            }
            catch
            {
            }

            return new UnknownSetting(pi);
        }
    }
    private abstract class SettingAccessor<T>(FieldInfo pi) : ISettingAccessor
    {
        private readonly FieldInfo _pi = pi;
        private readonly Settings.Setting<T> _cachedSetting = (Settings.Setting<T>)pi.GetValue(null)!;

        protected Settings.Setting<T> CachedSetting => _cachedSetting;
        public string GetString()
        {
            return ToString(Get());
        }
        public bool SetByString(string str)
        {
            if (!TryParse(str, out var v))
            {
                return false;
            }

            Set(v);
            return true;
        }
        protected abstract string ToString(T value);
        protected abstract bool TryParse(string str, out T value);
        private Settings.Setting<T> SettingObject => _cachedSetting;
        private T Get() => SettingObject.Value;
        private void Set(T value) => SettingObject.Value = value;
        public string InConfigName => SettingObject.Name;
        public string PropertyName => _pi.Name;
        public T DefaultValue => SettingObject.Default;
        public string GetDefaultString() => ToString(DefaultValue);
        public Delegate[] GetInvocationList()
        {
            var handler = CachedSetting.GetEventHandler<EventArgs>(nameof(Settings.Setting<bool>.Changed));
            if (handler is null) return [];

            return handler.GetInvocationList();
        }
    }
    private sealed class IntSetting(FieldInfo pi) : SettingAccessor<int>(pi)
    {
        protected override string ToString(int value) => value.ToString(CultureInfo.InvariantCulture);

        protected override bool TryParse(string str, out int value) => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
    private sealed class FloatSetting(FieldInfo pi) : SettingAccessor<float>(pi)
    {
        protected override string ToString(float value) => value.ToString(CultureInfo.InvariantCulture);

        protected override bool TryParse(string str, out float value) => float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
    private sealed class DoubleSetting(FieldInfo pi) : SettingAccessor<double>(pi)
    {
        protected override string ToString(double value) => value.ToString(CultureInfo.InvariantCulture);

        protected override bool TryParse(string str, out double value) => double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
    private sealed class StringSetting(FieldInfo pi) : SettingAccessor<string>(pi)
    {
        protected override string ToString(string value) => value;

        protected override bool TryParse(string str, out string value)
        {
            value = str;
            return true;
        }
    }
    private sealed class BoolSetting(FieldInfo pi) : SettingAccessor<bool>(pi)
    {
        protected override string ToString(bool value) => value ? "true" : "false";

        protected override bool TryParse(string str, out bool value)
        {
            var input = str.Trim().ToLowerInvariant();
            if (input is "true")
            {
                value = true;
                return true;
            }
            else if (input is "false")
            {
                value = false;
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }
    }
    private sealed class UnknownSetting(FieldInfo pi) : ISettingAccessor
    {
        public string GetDefaultString() => "<unknown>";
        public string GetString() => "<unknown>";
        public bool SetByString(string str) => false;

        public Delegate[] GetInvocationList() => [];

        public string InConfigName => "<unknown>";
        public string PropertyName => pi.Name;
    }

    public static IEnumerable<ISettingAccessor> FindSettings()
    {
        return typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
            .Where(pi => IsAnticipatedType(pi.FieldType))
            .Select(SettingAccessorFactory.Create);
    }

    private static bool IsAnticipatedType(Type pi)
    {
        return pi.IsGenericType && pi.GetGenericTypeDefinition() == typeof(Settings.Setting<>);
    }
}
