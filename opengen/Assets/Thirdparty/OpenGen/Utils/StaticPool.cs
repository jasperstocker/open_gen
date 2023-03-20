using System.Collections.Generic;

namespace opengen
{
    /// <summary>
    /// Thread Safe static pool of generic items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    
    public static class StaticPool<T> where T : IPoolItem, new()
    {
        //maintain a lock object across the class
        private static readonly object OBJECT = new object(); 
        private static readonly List<T> POOL = new List<T>();
        private static int COUNTER = 0;
        
        public static T Pull()
        {
            lock(OBJECT)
            {
                T output;
             
                if(POOL.Count > 0)
                {
                    output = POOL[0];
                    POOL.RemoveAt(0);
                }
                else
                {
                    output = new T {id = COUNTER};
                    COUNTER++;
                }

                return output;
            }
        }

        public static void Push(T input)
        {
            lock(OBJECT)
            {
                if(input == null)
                {
                    return;
                }

                if(POOL.Contains(input))
                {
                    return;
                }
                input.Clear();
                POOL.Add(input);
            }
        }

        public static bool Contains(T mesh)
        {
            if(mesh == null)
            {
                return false;
            }

            for(int i = 0; i < POOL.Count; i++)
            {
                if(POOL[i].id == mesh.id)
                {
                    return true;
                }
            }

            return false;
        }

        public static void Clear()
        {
            lock(OBJECT)
            {
                POOL.Clear();
                COUNTER = 0;
            }
        }
    }

    public interface IPoolItem
    {
        int id { get; set; }
        void Clear();
    }
}