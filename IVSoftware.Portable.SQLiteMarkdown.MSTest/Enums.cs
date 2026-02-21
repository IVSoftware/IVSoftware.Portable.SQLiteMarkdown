using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Common.Exceptions;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    public enum StdIvsEpoch
    {
        [Description("2019-02-22T08:55:00+07:00")]
        IvsEpochMinus5,

        [Description("2019-02-22T09:00:00+07:00")]
        IvsEpoch,

        [Description("2019-02-22T09:05:00+07:00")]
        IvsEpochPlus5,

        [Description("2026-01-01T09:00:00+07:00")]
        January1,
    }

    public static partial class SQLiteMarkdownTestExtensions
    {
        public static DateTimeOffset ToDateTimeOffset(this Enum @this)
        {
            if(@this.ToDateTimeOffsetOrNull() is { } dto)
            {
                return dto;
            }
            else
            {
                @this.ThrowHard<NullReferenceException>($"{@this.ToFullKey()} evaluated to null DateTimeOffset.");
                return default;
            }
        }
        public static DateTimeOffset? ToDateTimeOffsetOrNull(this Enum @this)
        {
            if (@this is null)
                return null;

            var type = @this.GetType();
            var name = Enum.GetName(type, @this);
            if (name is null)
                return null;

            var field = type.GetField(name);
            if (field is null)
                return null;

            var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(
                field,
                typeof(DescriptionAttribute));

            if (attr is null || string.IsNullOrWhiteSpace(attr.Description))
                return null;

            if (DateTimeOffset.TryParse(
                    attr.Description,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var dto))
            {
                return dto;
            }

            return null;
        }
    }
}
