using System;

namespace Common.Parsers;

internal class StringReader
{
    private readonly string _data;
    private int _index;

    public StringReader(string text)
    {
        _data = text.ReplaceLineEndings("\n");
        _index = 0;
    }

    public char Read()
    {
        return _index == _data.Length ? '\0' : _data[_index++];
    }

    public void Rewind()
    {
        _index = Math.Clamp(_index - 1, 0, _data.Length);
    }

    public int CountLines()
    {
        var line = 1;

        for (var i = 0; i < _index; i++)
            if (_data[i] == '\n')
                ++line;

        return line;
    }
}