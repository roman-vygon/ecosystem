using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
public class GenericPool<T> where T: LivingEntity
{
    private readonly Stack<T> m_Pool;
    private readonly HashSet<T> m_Active;

    private readonly Func<Species, T> m_FuncCreate;
    private readonly Action<T> m_ActionGet;
    private readonly Action<T> m_ActionReturn;

    private readonly Species species;
    public GenericPool(Func<Species, T> createFunction, Action<T> onGet, Action<T> onReturn, Species species, int capacity = 0) 
    {
        Assert.IsNotNull(createFunction);

        m_Pool = new Stack<T>(capacity);
        m_Active = new HashSet<T>();

        this.species = species;
        m_FuncCreate = createFunction;
        m_ActionGet = onGet;
        m_ActionReturn = onReturn;
    }

    public void Prewarm(int capacity)
    {
        for (int i = 0; i < capacity; i++)
            Return(m_FuncCreate.Invoke(species));
    }

    public T Get()
    {
        T element;
        if (m_Pool.Count == 0)
            element = m_FuncCreate.Invoke(species);
        else
            element = m_Pool.Pop();

        if (m_ActionGet != null)
            m_ActionGet.Invoke(element);

        m_Active.Add(element);
        return element;
    }

    public void Return(T element)
    {
#if DEBUG
        if (m_Pool.Contains(element))
        {
            Debug.Log("pool contains");
            return;
        }
            //throw new InvalidOperationException("Element '" + element + "' already in pool.");
#endif

        if (m_ActionReturn != null)
            m_ActionReturn.Invoke(element);

        m_Active.Remove(element);
        m_Pool.Push(element);
    }
}
