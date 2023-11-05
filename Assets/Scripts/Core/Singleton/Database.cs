using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDatabase<T> : IEnumerable<T>, IEnumerable where T : Unit
{
    /// <summary>
    /// Objects is being accessed by multiple threads and is not thread safe, please lock it before iterating.
    /// </summary>
    public IReadOnlyDictionary<Guid, T> Objects => _objects;
    private readonly Dictionary<Guid, T> _objects = new();

    public event Action<T> Added;
    public event Action<T> Removed;
    public bool Add(T obj)
    {
        lock (Objects)
        {
            if (Objects.ContainsKey(obj.Id))
                return false;

            Debug.Log($"Adding {obj.GetType().Name}: {obj.Id}");
            _objects[obj.Id] = obj;
            Added?.Invoke(obj);

            return true;
        }
    }

    public bool Remove(T obj)
    {
        lock (Objects)
        {
            if (!Objects.ContainsKey(obj.Id))
                return false;

            Debug.Log($"Removing {obj.GetType().Name}: {obj.Id}");
            _objects.Remove(obj.Id);
            Removed?.Invoke(obj);

            return true;
        }
    }

    public IEnumerator<T> GetEnumerator() => Objects.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Objects.Values.GetEnumerator();
}

public class Database
{
    public static Database Instance { get; } = new Database();

    public ObjectDatabase<Food> Food { get; } = new ObjectDatabase<Food>();
    public ObjectDatabase<Tree> Trees { get; } = new ObjectDatabase<Tree>();
    public ObjectDatabase<Silica> Silica { get; } = new ObjectDatabase<Silica>();
    public ObjectDatabase<Shelter> Shelters { get; } = new ObjectDatabase<Shelter>();
    public ObjectDatabase<Specimen> Specimen { get; } = new ObjectDatabase<Specimen>();

    public event Action<Unit> UnitAdded;
    public event Action<Unit> UnitRemoved;

    public Database()
    {
        Food.Added += (o) => UnitAdded?.Invoke(o);
        Trees.Added += (o) => UnitAdded?.Invoke(o);
        Silica.Added += (o) => UnitAdded?.Invoke(o);
        Specimen.Added += (o) => UnitAdded?.Invoke(o);
        Shelters.Added += (o) => UnitAdded?.Invoke(o);

        Food.Removed += (o) => UnitRemoved?.Invoke(o);
        Trees.Removed += (o) => UnitRemoved?.Invoke(o);
        Silica.Removed += (o) => UnitRemoved?.Invoke(o);
        Specimen.Removed += (o) => UnitRemoved?.Invoke(o);
        Shelters.Removed += (o) => UnitRemoved?.Invoke(o);
    }

    public bool Add(Unit unit)
    {
        if (unit.Id == default)
            throw new InvalidOperationException("Unit wiht no id cannot be added");

        return unit switch
        {
            Food food => Food.Add(food),
            Tree tree => Trees.Add(tree),
            Silica silica => Silica.Add(silica),
            Shelter shelter => Shelters.Add(shelter),
            Specimen specimen => Specimen.Add(specimen),
            _ => false
        };
    }

    public bool Remove(Unit unit)
    {
        if (unit.Id == default)
            throw new InvalidOperationException("Unit wiht no id cannot be removed");

        return unit switch
        {
            Food food => Food.Remove(food),
            Tree tree => Trees.Remove(tree),
            Silica silica => Silica.Remove(silica),
            Shelter shelter => Shelters.Remove(shelter),
            Specimen specimen => Specimen.Remove(specimen),
            _ => false
        };
    }
}
