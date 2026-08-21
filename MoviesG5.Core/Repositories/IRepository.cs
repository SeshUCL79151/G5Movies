using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.Core
{
    public interface IRepository<T>
    {
        void Add(T item); // Create
        T? GetById(int id); // Read
        void Update(T item); // Update
        void Remove(int id); // Delete
        List<T> GetAll();
        void Save();
    }
}
