namespace ConfigApi.Mvc.Models;

public static class Pager
{
    public static IEnumerable<int?> Pages(int current, int total, int window = 2)
    {
        if (total <= 1) yield break;

        var shown = new SortedSet<int> { 1, total };

        for (var i = current - window; i <= current + window; i++)
            if (i >= 1 && i <= total) shown.Add(i);

        int? previous = null;

        foreach (var page in shown)
        {
            if (previous is not null && page - previous > 1)
                yield return null;

            yield return page;
            previous = page;
        }
    }
}
