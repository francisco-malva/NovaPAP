#region

using System.Globalization;

#endregion

namespace Common.Parsers;

public static class SExpressionParser
{
    public static object? Parse(string expression)
    {
        var reader = new StringReader(expression);
        return Parse(reader);
    }

    private static object? Parse(StringReader reader)
    {
        while (true)
        {
            var character = reader.Read();

            if (character == '\0')
                return null;

            if (char.IsWhiteSpace(character))
                continue;

            if (char.IsNumber(character))
                return ParseNumber(reader);

            switch (character)
            {
                case '(':
                    return ParseList(reader);
                case '"':
                    return ParseString(reader);
            }
        }
    }

    private static List<object?> ParseList(StringReader reader)
    {
        var list = new List<object?>();

        var line = reader.CountLines();

        while (true)
        {
            var character = reader.Read();

            if (character is ')')
                break;

            if (character == '\0') throw new Exception($"Error on line {line}, list terminator expected.");

            reader.Rewind();

            list.Add(Parse(reader));
        }

        return list;
    }

    private static string ParseString(StringReader reader)
    {
        var buffer = string.Empty;

        while (true)
        {
            var character = reader.Read();

            switch (character)
            {
                case '"':
                    return buffer;
                case '\n':
                    continue;
                case '\0':
                    throw new Exception($"Error on line {reader.CountLines()}, expected string terminator.");
                default:
                    buffer += character;
                    break;
            }
        }
    }

    private static object ParseNumber(StringReader reader)
    {
        reader.Rewind();

        var line = reader.CountLines();

        var buffer = string.Empty;

        while (true)
        {
            var character = reader.Read();

            if (character == '\0' || char.IsWhiteSpace(character) || character is '(' or ')')
            {
                reader.Rewind();
                break;
            }

            buffer += character;
        }

        if (int.TryParse(buffer, NumberStyles.Any, CultureInfo.InvariantCulture, out var integer))
            return integer;

        if (double.TryParse(buffer, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatingPoint))
            return floatingPoint;

        throw new Exception($"Error on line {line}, could not parse number with contents \"{buffer}\".");
    }
}