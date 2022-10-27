using static BWInf.Classes;
using Path = BWInf.Classes.Path;

namespace BWInf;

public static class Utility
{
    /// <summary>
    /// Key: Value des Feldes, Value: Array mit den values von den zu erreichenden Feldern
    /// </summary>
    public static Dictionary<int, int[]> FieldsDict { get; set; } = new()
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

    public static Dictionary<int, int[]> ReadTextfile(string path)
    {
        var tempFiledDict = new Dictionary<int, int[]>();

        using StreamReader sr = new StreamReader(File.Open(path, FileMode.Open));

        while (!sr.EndOfStream)
        {
            int key = 0;
            int value = 0;

            try
            {
                var line = (sr.ReadLine() ?? "").Split(" ");
                if (line.Length == 0)
                    continue;

                key = int.Parse(line[0]);
                value = int.Parse(line[1]);

            }catch(IOException ex)
            {
                Console.WriteLine("Fehler beim einlesen der Textdatei:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            if (!tempFiledDict.ContainsKey(key))
                tempFiledDict.Add(key, Array.Empty<int>());

            tempFiledDict[key] = tempFiledDict[key].Append(value).ToArray();
        }

        foreach(var kvp in tempFiledDict)
        {
            if (kvp.Value is null)
                tempFiledDict[kvp.Key] = Array.Empty<int>();
        }

        return tempFiledDict;
    }
}