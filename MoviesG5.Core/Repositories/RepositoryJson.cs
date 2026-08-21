using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MoviesG5.Core
{
    public class RepositoryJson<T> : IRepository<T> where T : IHasId
    {
        private readonly string _filePath;
        private List<T> _items;

        private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

        public RepositoryJson(string filePath)
        {
            _filePath = filePath;
            _items = new List<T>();

            if (!File.Exists(filePath)) return;

            try
            {
                string json = File.ReadAllText(filePath);
                _items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            catch (JsonException)
            {
                _items = new List<T>();
            }
        }

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

        public void Save()
        {
            string json = JsonSerializer.Serialize(_items, options);
            File.WriteAllText(_filePath, json);
        }
    }
}
