using System.Text;

namespace RaidBot;

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendTruncated(this StringBuilder stringBuilder, string str, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(str);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 3);

        if (str.Length <= maxLength)
        {
            stringBuilder.Append(str);
        }
        else
        {
            stringBuilder.Append(str.AsSpan()[..(maxLength - 3)].TrimEnd()).Append("...");
        }

        return stringBuilder;
    }
}
