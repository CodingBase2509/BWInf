using static BWInf.Classes;
using Path = BWInf.Classes.Path;

namespace BWInf;

public static class Utility
{
    /// <summary>
    /// Key: Value des Feldes, Value: Array mit den values von den zu erreichenden Feldern
    /// </summary>
    public static Dictionary<int, int[]> FieldsDict { get; } = new()
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
 
    /// <summary>
    /// Eine Liste mit den fertigen <see cref="Field"/> Objekten
    /// </summary>
    public static List<Field> FieldsList { get; set; } = new();

    /// <summary>
    /// Eine threadsichere Methode um einer <see cref="Person"/> einen <see cref="Path"/> hinzuzufügen
    /// </summary>
    /// <param name="person">Die Person der der <see cref="Path"/> hinzugefügt werden soll</param>
    /// <param name="path">Der <see cref="Path"/> der hinzugefügt werden soll</param>
    public static void AddPath(this Person person, Path path)
    {
        bool cantEnter;
        do
        {
            if (cantEnter = Monitor.TryEnter(person.Paths))
            {
                try
                {
                    person.Paths.Add(path);
                }
                finally
                {
                    Monitor.Exit(person.Paths);
                    cantEnter = false;
                }
            }
        } while (cantEnter);
    }
}