using MoviesG5.Core;

namespace MoviesG5.Tests.Fakes
{
    public class InMemoryRepository<T> : IRepository<T> where T : IHasId
    {
        private readonly List<T> _items = new();

        public int SaveCallCount { get; private set; }

        public void Add(T item)
        {
            item.Id = _items.Count > 0 ? _items.Max(i => i.Id) + 1 : 1;
            _items.Add(item);
        }

        public T? GetById(int id) => _items.FirstOrDefault(i => i.Id == id);

        public void Update(T item)
        {
            int index = _items.FindIndex(i => i.Id == item.Id);
            if (index != -1) _items[index] = item;
        }

        public void Remove(int id) => _items.RemoveAll(i => i.Id == id);

        public List<T> GetAll() => _items;

        public void Save() => SaveCallCount++;
    }
}
