namespace BWInf;

public class Program
{

    public static void Main(string[] args)
    {
        var p1 = new Person()
        {
            Name = "Tom",
            StartField = 1,
            Paths = new()
        };

        var p2 = new Person()
        {
            Name = "Mareike",
            StartField = 16,
            Paths = new()
        };

        Utility.FieldsList = (List<Field>)Field.Create();

        List<Task> initalTasks = new()
        {
            Task.Run(() => StartMovement(p1)),
            Task.Run(() => StartMovement(p2))
        };

    }

    public static void StartMovement(Person person)
    {
        
    }

    public static bool MoveField(Person person)
    {

    }


}