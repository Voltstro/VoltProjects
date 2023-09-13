namespace VoltProjects.Shared.Collections;

public class ListBuilder<T> where T : notnull
{
    private readonly List<T> array = new();

    public void Add(T item)
    {
        lock (array)
        {
            array.Add(item);
        }
    }

    public void AddRange(IReadOnlyList<T> items)
    {
        lock (array)
        {
            for (var i = 0; i < items.Count; i++)
            {
                array.Add(items[i]);
            }
        }
    }

    public IReadOnlyList<T> AsList()
    {
        array.TrimExcess();
        return array;
    }
}