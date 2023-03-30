namespace MVE.SalExt;

public class Chooser<T>
{
    protected Random random;
    protected List<T> elements;

    public Chooser(Random random, params T[] elements)
    {
        this.random = random;
        this.elements = new(elements.Length);
        this.elements.AddRange(elements);
    }

    public Chooser(Random random, IEnumerable<T> elements)
    {
        this.random = random;
        this.elements = new(4);
        this.elements.AddRange(elements);
    }

    public T Choose()
        => elements[random.Next(0, elements.Count)];

    public static T ChooseFrom(Random random, IReadOnlyList<T> collection)
        => collection[random.Next(0, collection.Count)];
}

public static class Chooser
{
    public static T Choose<T>(Random random, IReadOnlyList<T> collection)
        => collection[random.Next(0, collection.Count)];

    public static T Choose<T>(Random random, params T[] array)
        => Choose(random, array);
}
