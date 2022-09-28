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

        // Ausführung des Programmes
        // Für jeden Durchlauf ein Feld weiter gehen
        do
        {
            OneStep.Add(Task.Run(async () =>
            {
                // Geht für jedes Feld auf die nächsten möglichen Felder
                var nextFields = await ForEachField(person1, fieldsToMoveFrom1);

                // setzt ob die Person sich auf einem Feld getroffen haben
                p1Hit = person1.FinishRun;
                if (p1Hit)
                {
                    // gibt die benötigten Infos aus
                    var destField = Finish(person1);
                    // gibt Infos über die andere Person aus
                    ReviewOtherPerson(person2, destField);
                    // beendet das Programm
                    Environment.Exit(0);
                }

                // leert die liste und fügt die nächsten Felder für den nächsten Druchlauf hinzu
                fieldsToMoveFrom1.Clear();
                fieldsToMoveFrom1.AddRange(nextFields);
            }));

            OneStep.Add(Task.Run(async () =>
            {
                // Geht für jedes Feld auf die nächsten möglichen Felder
                var nextFields = await ForEachField(person2, fieldsToMoveFrom2);

                // setzt ob die Person sich auf einem Feld getroffen haben
                p2Hit = person2.FinishRun;
                if (p2Hit)
                {
                    // gibt die benötigten Infos aus
                    var destField = Finish(person2);
                    // gibt Infos über die andere Person aus
                    ReviewOtherPerson(person1, destField);
                    // beendet das Programm
                    Environment.Exit(0);
                }

                // leert die liste und fügt die nächsten Felder für den nächsten Druchlauf hinzu
                fieldsToMoveFrom2.Clear();
                fieldsToMoveFrom2.AddRange(nextFields);
            }));

            // wartet das beide Personen ihre Schritte beendet haben
            await Task.WhenAll(OneStep);
            // leert die liste mit den Aufgaben
            OneStep.Clear();

        // führt die Schritte so lange aus, solange die Personen sich nicht treffen
        } while (!(p1Hit && p2Hit));
    }

    /// <summary>
    /// Führt für jedes Feld bestimmte Schritte aus
    /// </summary>
    /// <param name="person">Die Person, für die die Schritte durchgeführt werden</param>
    /// <param name="fieldsToMoveFrom">Die Liste mit den <see cref="Field"/>s für die die Schritte ausgeführt werden</param>
    /// <returns>Eine Liste</returns>
    public static async Task<IList<Field>> ForEachField(Person person, IList<Field> fieldsToMoveFrom)
    {
        List<Task> tasks = new();
        ConcurrentBag<Field> fields = new();

        foreach (var field in fieldsToMoveFrom)
        {
            // sucht den Pfad von der Person, wo das aktuelle Feld das letzte Element ist
            var path = person.Paths
                .OrderByDescending(pp => pp.Count)
                .Where(pp => pp.Value[pp.Count - 1] == field)
                .FirstOrDefault();

            tasks.Add(Task.Run(async () =>
            {
                // geht für das ausgewählte Feld einen schritt weiter
                var newFields = await MoveNextField(person, field, path!.Clone());
                
                // setzt die nächsten felder auf die Liste
                foreach (var nf in newFields)
                    fields.Add(nf);
            }));
        }

        // // wartet das die Aufgaben für alle Felder fertig sind
        await Task.WhenAll(tasks);

        // gibt eine Liste mit den nächsten Feldern zurück
        return fields.ToList();
    }

    /// <summary>
    /// Geht ein Feld weiter
    /// </summary>
    /// <param name="person">Die Person, die einen Schritt weiter geht</param>
    /// <param name="field">Das Feld von dem aus gegangen wird</param>
    /// <param name="path">Der Pfad, an dem das jeweilige Feld dran gehanden wird</param>
    /// <returns></returns>
    public static async Task<IList<Field>> MoveNextField(Person person, Field field, Path path)
    {
        List<Task> tasks = new();
        ConcurrentBag<Field> fields = new();

        // Für jedes mögliche nächste Feld des jetzigen Feldes
        foreach (var nextfield in field.NextPossibleFields)
        {
            tasks.Add(Task.Run(() =>
            {
                // prüft ob eine Person auf dem Feld steht und es die Person selbst ist
                if (nextfield.PersonOnField is not null && Equals(nextfield.PersonOnField, person))
                    return;

                // kopiert den Pfad für das neue Feld
                var nextPath = path.Clone();
                //fügt das nächste Feld dem Pfad hinzu
                nextPath.AddField(nextfield);
                // fügt der Person dem Pfad hinzu
                person.AddPath(nextPath);

                // fügt das nächste Feld dem ConcurrentBag hinzu
                fields.Add(nextfield);

                // prüft ob auf dem nächsten Feld eine Person schon drauf ist und es nicht sie selbst ist
                if (nextfield.PersonOnField is not null && !Equals(nextfield.PersonOnField, person))
                {
                    // setzt sich als zweites auf das Feld
                    nextfield.SecondPerson = person;
                    // definiert, das sich die Personen getroffen habe - Ziel erreicht
                    person.FinishRun = true;
                    return;
                }
                else // ansonsten setzt sich die Person auf das Feld
                    nextfield.PersonOnField = person;
            }));
        }

        // wartet das die Aufgaben für alle nächsten Felder fertig sind
        await Task.WhenAll(tasks);

        // gibt eine Liste mit den nächsten Feldern zurück
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
            if (Equals(field, person.StartField))
                pathAsString += field.Value.ToString() + " (start Feld) - ";
            else
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
        {
            if (Equals(lastField, person.StartField))
                pathAsString += field.Value.ToString() + " (start Feld) - ";
            else
                pathAsString += field.Value.ToString() + " - ";
        }

        Console.WriteLine($"Shortest path for {person.Name}:");
        Console.WriteLine(pathAsString + "finish");
        Console.WriteLine("");
    }

}