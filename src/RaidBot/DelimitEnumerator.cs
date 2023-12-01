namespace RaidBot;

public ref struct DelimitEnumerator(ReadOnlySpan<char> span, ReadOnlySpan<char> delimiters, bool removeEmptyEntries = true, bool trimWhitespace = false)
{
    private ReadOnlySpan<char> _current = default;

    public DelimitEnumerator(string? str, ReadOnlySpan<char> delimiters, bool removeEmptyEntries = true, bool trimWhitespace = false)
        : this(str.AsSpan(), delimiters, removeEmptyEntries, trimWhitespace)
    {
    }

    public readonly ReadOnlySpan<char> Delimiters { get; } = delimiters;

    public readonly bool RemoveEmptyEntries { get; } = removeEmptyEntries;

    public readonly bool TrimWhitespace { get; } = trimWhitespace;

    public readonly ReadOnlySpan<char> Current
    {
        get
        {
            if (TrimWhitespace)
            {
                return _current.Trim();
            }
            return _current;
        }
    }

    public ReadOnlySpan<char> Remainder { get; private set; } = trimWhitespace ? span.Trim() : span;

    public readonly DelimitEnumerator GetEnumerator()
    {
        return this;
    }

    public bool MoveNext()
    {
        while (Remainder.Length > 0)
        {
            int i = Remainder.IndexOfAny(Delimiters);

            if (i == 0)
            {
                if (!RemoveEmptyEntries)
                {
                    Remainder = Remainder[1..];
                    _current = default;
                    return true;
                }
                else
                {
                    do
                    {
                        Remainder = Remainder[1..];
                        i = Remainder.IndexOfAny(Delimiters);
                    }
                    while (i == 0);
                }
            }

            if (i < 0)
            {
                _current = Remainder;
                Remainder = default;
                return !RemoveEmptyEntries || Current.Length != 0;
            }

            _current = Remainder[..i++];
            Remainder = Remainder[i..];
            if (!RemoveEmptyEntries || Current.Length != 0)
            {
                return true;
            }
        }

        return false;
    }
}
