using System.Collections.Concurrent;
using static BWInf.Classes;
using Path = BWInf.Classes.Path;

namespace BWInf;

public class Program
{

    public static async Task Main(string[] args)
    {
        // preparation
        Field.CreateFields(Utility.FieldsDict);

        var p1 = new Person()
        {
            Name = "Sasha",
            StartField = Utility.FieldsList
                .Where(f => Equals(f.Value, 1))
                .SingleOrDefault()!,
            Paths = new()
        };

        var p2 = new Person()
        {
            Name = "Mika",
            StartField = Utility.FieldsList
                .Where(f => Equals(f.Value, 2))
                .SingleOrDefault()!,
            Paths = new()
        };

        await Start(p1, p2);
        Console.ReadKey();
    }

    public static async Task Start(Person person1, Person person2)
    {
        // Liste mit laufenden Tasks
        List<Task> OneStep = new();
        
        // Variablen für Person 1
        bool p1Hit = false;
        var path1 = new Path();

        path1.AddField(person1.StartField);
        person1.AddPath(path1);

        var fieldsToMoveFrom1 = new List<Field>()
        {
            person1.StartField
        };

        //--------------------------------------------

        // Variablen für Person 2
        bool p2Hit = false;
        var path2 = new Path();

        path2.AddField(person2.StartField);
        person2.AddPath(path2);

        var fieldsToMoveFrom2 = new List<Field>()
        {
            person2.StartField
        };

        do
        {
            OneStep.Add(Task.Run(async () =>
            {
                var nextFields = await ForEachField(person1, fieldsToMoveFrom1);

                p1Hit = person1.FinishRun;
                if (p1Hit)
                {
                    var destField = Finish(person1);
                    ReviewOtherPerson(person2, destField);
                    Environment.Exit(0);
                }

                fieldsToMoveFrom1.Clear();
                fieldsToMoveFrom1.AddRange(nextFields);
            }));

            OneStep.Add(Task.Run(async () =>
            {
                var nextFields = await ForEachField(person2, fieldsToMoveFrom2);

                p2Hit = person2.FinishRun;
                if (p2Hit)
                {
                    var destField = Finish(person2);
                    ReviewOtherPerson(person1, destField);
                    Environment.Exit(0);
                }

                fieldsToMoveFrom2.Clear();
                fieldsToMoveFrom2.AddRange(nextFields);
            }));

            await Task.WhenAll(OneStep);
            OneStep.Clear();

        } while (!(p1Hit && p2Hit));
    }

    public static async Task<IList<Field>> ForEachField(Person person, IList<Field> fieldsToMoveFrom)
    {
        List<Task> tasks = new();
        ConcurrentBag<Field> fields = new();

        foreach (var field in fieldsToMoveFrom)
        {
            var path = person.Paths
                .OrderByDescending(pp => pp.Count)
                .Where(pp => pp.Value[pp.Count - 1] == field)
                .FirstOrDefault();

            tasks.Add(Task.Run(async () =>
            {
                var newFields = await MoveNextField(person, field, path!.Clone());

                foreach (var nf in newFields)
                    fields.Add(nf);
            }));
        }

        await Task.WhenAll(tasks);

        return fields.ToList();
    }

    public static async Task<IList<Field>> MoveNextField(Person person, Field field, Path path)
    {
        List<Task> tasks = new();
        ConcurrentBag<Field> fields = new();

        foreach (var nextfield in field.NextPossibleFields)
        {
            tasks.Add(Task.Run(() =>
            {
                if (nextfield.PersonOnField is not null && Equals(nextfield.PersonOnField, person))
                    return;

                path.AddField(nextfield);
                person.AddPath(path);

                fields.Add(nextfield);

                if (nextfield.PersonOnField is not null && !Equals(nextfield.PersonOnField, person))
                {
                    nextfield.SecondPerson = person;
                    person.FinishRun = true;
                    return;
                }
                else
                    nextfield.PersonOnField = person;
            }));
        }

        await Task.WhenAll(tasks);

        return fields.ToList();
    }

    public static Field Finish(Person person)
    {
        var shortestPath = person.Paths
            .Where(p => p.Value[p.Count - 1].SecondPerson is not null)
            .OrderByDescending(fp => fp.Count)
            .FirstOrDefault();

        string pathAsString = "";

        foreach (var field in shortestPath!.Value)
        {
            pathAsString += field.Value.ToString() + " - ";
        }
        Console.WriteLine($"{person.Name} finishes the run: {person.FinishRun}");
        Console.WriteLine($"Shortest path for {person.Name}:");
        Console.WriteLine(pathAsString + "finish");
        Console.WriteLine("");

        return shortestPath.Value[shortestPath.Count - 1];
    }

    public static void ReviewOtherPerson(Person person, Field field)
    {
        var shortestPath = person.Paths
            .Where(p => p.Value[p.Count - 1] == field)
            .OrderByDescending(fp => fp.Count)
            .FirstOrDefault();

        string pathAsString = "";

        foreach (var lastField in shortestPath!.Value)
            pathAsString += lastField.Value.ToString() + " - ";

        Console.WriteLine($"Shortest path for {person.Name}:");
        Console.WriteLine(pathAsString + "finish");
        Console.WriteLine("");
    }

}