namespace Example.Business.Collections
{
    public class LoopedList<T>
    {
        private readonly LinkedList<T> _items = new();

        public LoopedList(params T[] items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
        
        public void Add(T value)
        {
            _items.AddLast(value);
        }

        public T Next()
        {
            var next = _items.First.Value;
            _items.RemoveFirst();
            _items.AddLast(next);
            return next;
        }
    }
}

