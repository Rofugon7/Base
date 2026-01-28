namespace BaseConLogin.Extensions
{
    public static class CollectionExtensions
    {
        public static void RemoveWhere<T>(this ICollection<T> collection, System.Func<T, bool> predicate)
        {
            var items = new List<T>();
            foreach (var i in collection)
            {
                if (predicate(i)) items.Add(i);
            }
            foreach (var i in items)
            {
                collection.Remove(i);
            }
        }
    }
}
