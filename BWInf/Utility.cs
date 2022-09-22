
using System.Collections.Concurrent;

namespace BWInf;

public static class Utility
{

    private static Dictionary<int, int[]> Fields { get; } = new()
    {
        {1, new[] { 8, 4, 18 } },
        {2, new[] { 3, 19 } },
        {3, new[] { 6, 19 } },
        {4, Array.Empty<int>() },
        {5, new[] { 12, 13, 15 } },
        {6, new[] { 2, 12 } },
        {7, new[] { 5, 8 } },
        {8, new[] { 4, 18 } },
        {9, new[] { 1, 4, 14 } },
        {10, new[] { 3, 12, 16, 18 } },
        {11, new[] { 19 } },
        {12, new[] { 3 } },
        {13, new[] { 7, 10 } },
        {14, new[] { 1, 10, 16, 17, 18 } },
        {15, new[] { 12 } },
        {16, new[] { 11, 17, 19, 20 } },
        {17, new[] { 19 } },
        {18, new[] { 7, 13 } },
        {19, new[] { 20 } },
        {20, new[] { 3, 10 } },
    };

    public static Dictionary<int, int[]> GetFields => Fields;

    public static List<Field> FieldsList { get; set; } = new();

    public static List<Path> Paths { get; private set; } = new();

    public static void AddPath(this Person person, Path path)
    {
        bool cantEnter = true;
        do
        {
            if (Monitor.TryEnter(Paths) &&
                Monitor.TryEnter(person.Paths))
            {
                try
                {
                    Paths.Add(path);
                    person.Paths.Add(path);
                    cantEnter = false;
                }
                finally
                {
                    Monitor.Exit(Paths);
                    Monitor.Exit(person.Paths);
                }
            }
        } while (cantEnter);

        cantEnter = true;
    }

}

/// <summary>
/// Pfad
/// </summary>
public class Path
{
    public Field[] Value { get; private set; } = Array.Empty<Field>();

    public int FieldCount => Value.Length;

    public void AddField(Field field)
    {
        var newVal = new Field[Value.Length + 1];

        for (int i = 0; i < Value.Length; i++)
            newVal[i] = Value[i];

        newVal[Value.Length] = field;

        Value = newVal;
    }

    public Path Clone()
    {
        return new Path()
        {
            Value = (Field[])this.Value.Clone()
        };
    } 
}

/// <summary>
/// Feld
/// </summary>
public class Field
{
    public int Value { get; init; }

    public int[] NextPossibleFields { get; init; } = Array.Empty<int>();

    public Field(int value, int[] nextFields)
    {
        Value = value;
        NextPossibleFields = nextFields;
    }

    public static IList<Field> Create()
    {
        List<Field> f = new();
        foreach (var field in Utility.GetFields)
            f.Add(new Field(field.Key, field.Value));
        return f;
    }

}

/// <summary>
/// Person
/// </summary>
public class Person
{
    public string Name { get; init; }

    public int StartField { get; init; }

    public List<Path> Paths { get; set; }
}



