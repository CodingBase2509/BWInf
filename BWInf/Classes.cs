namespace BWInf;

public class Classes
{
    /// <summary>
    /// Definiert einen Pfad
    /// </summary>
    public class Path
    {
        /// <summary>
        /// Eine Liste mit <see cref="Field"/> die den Pfad darstellen
        /// </summary>
        public Field[] Value { get; private set; } = Array.Empty<Field>();

        /// <summary>
        /// Gibt die Anzahl an Feldern des Pfades an
        /// </summary>
        public int Count => Value.Length;

        /// <summary>
        /// Fügt ein <see cref="Field"/> dem <see cref="Path"/> hinzu
        /// </summary>
        /// <param name="field"></param>
        public void AddField(Field field)
        {
            var newVal = new Field[Value.Length + 1];

            for (int i = 0; i < Value.Length; i++)
                newVal[i] = Value[i];

            newVal[Value.Length] = field;

            Value = newVal;
        }

        /// <summary>
        /// Erstellt eine Schattenkopie des <see cref="Path"/>
        /// </summary>
        /// <returns>Die Kopie des <see cref="Path"/></returns>
        public Path Clone()
        {
            return new Path()
            {
                Value = (Field[])this.Value.Clone()
            };
        }
    }

    /// <summary>
    /// Definiert ein Feld
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Der nummerische Wert des <see cref="Field"/>
        /// </summary>
        public int Value { get; init; }

        /// <summary>
        /// Ein Array mit den nächsten, von diesem <see cref="Field"/> aus, betretbaren Feldern
        /// </summary>
        public Field[] NextPossibleFields { get; private set; } = Array.Empty<Field>();

        /// <summary>
        /// Die Person, die das Feld betreten hat
        /// </summary>
        public Person? PersonOnField { get; set; }

        /// <summary>
        /// Die zweite Person, die das Feld betreten hat
        /// </summary>
        public Person? SecondPerson { get; set; }

        /// <summary>
        /// Initialisiert ein Objekt der <see cref="Field"/> Klasse
        /// </summary>
        /// <param name="value">Der nummerische Wert des Feldes</param>
        public Field(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Erstellt aus einem Dictionary eine Liste mit <see cref="Field"/> Objekte
        /// </summary>
        /// <param name="fieldDict">Das Dictionary aus dem die Liste erstellt wird</param>
        public static void CreateFields(Dictionary<int, int[]> fieldDict)
        {
            CreateList(fieldDict);
            CreateNextValues(fieldDict);
        }

        /// <summary>
        /// Erstellt die Liste mit <see cref="Field"/>s und fügt jedem sein nummerisches Value hinzu
        /// </summary>
        /// <param name="fieldDict">Das Dictionary aus dem die Liste erstellt wird</param>
        private static void CreateList(Dictionary<int, int[]> fieldDict)
        {
            List<Field> f = new();
            foreach (var field in fieldDict)
                f.Add(new Field(field.Key));
            Utility.FieldsList = f;
        }

        /// <summary>
        /// Füllt die Liste mit <see cref="Field"/>s mit den nächsten möglichen <see cref="Field"/>s
        /// </summary>
        /// <param name="fieldDict">Das Dictionary aus dem die Liste erstellt wird</param>
        private static void CreateNextValues(Dictionary<int, int[]> fieldDict)
        {
            foreach (var field in Utility.FieldsList)
            {
                var values = fieldDict[field.Value];
                foreach (var val in values)
                {
                    field.NextPossibleFields = Utility.FieldsList
                        .Where(f => f.Value == val)
                        .ToArray();
                }
            }
        }

    }

    /// <summary>
    /// Definiert eine Person
    /// </summary>
    public class Person
    {
        public string Name { get; init; }

        public Field StartField { get; init; }

        public List<Path> Paths { get; set; }

        public bool FinishRun { get; set; }
    }
}
