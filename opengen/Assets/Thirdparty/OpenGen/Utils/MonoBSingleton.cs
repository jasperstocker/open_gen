using UnityEngine;

namespace opengen 
{
    //Forgive me father, for I have sinned.
    public abstract class MonoBSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T INSTANCE;
        private static bool INSTANTIATED;

        public static T Instance
        {
            get
            {
                if (INSTANTIATED)
                {
                    return INSTANCE;
                }

                var type = typeof(T);
                var objects = FindObjectsOfType<T>();

                if (objects.Length > 0)
                {
                    INSTANCE = objects[0];
                    if (objects.Length > 1)
                    {
                        Debug.LogWarning("There is more than one instance of Singleton of type \"" + type + "\". Keeping the first. Destroying the others.");
                        for (var i = 1; i < objects.Length; i++)
                        {
                            DestroyImmediate(objects[i].gameObject);
                        }
                    }
                    INSTANTIATED = true;
                    return INSTANCE;
                }
                Instantiate();
                return INSTANCE;
            }
        }

        public static T InstanceNoInstantiate
        {
            get
            {
                if (INSTANTIATED)
                {
                    return INSTANCE;
                }

                var type = typeof(T);
                var objects = FindObjectsOfType<T>();

                if (objects.Length > 0)
                {
                    INSTANCE = objects[0];
                    if (objects.Length > 1)
                    {
                        Debug.LogWarning("There is more than one instance of Singleton of type \"" + type + "\". Keeping the first. Destroying the others.");
                        for (var i = 1; i < objects.Length; i++)
                        {
                            DestroyImmediate(objects[i].gameObject);
                        }
                    }
                    INSTANTIATED = true;
                    return INSTANCE;
                }

                return null;
            }
        }

        public static void Instantiate()
        {
            if(INSTANTIATED)
            {
                return;
            }

            var gameObject = new GameObject();
            var type = typeof(T);
            gameObject.name = type.ToString();
            INSTANCE = gameObject.AddComponent<T>();
            INSTANTIATED = true;
        }

        public static bool IsInstantiated { get { return INSTANTIATED; } }

        protected virtual void OnDestroy()
        {
            INSTANCE = null;
            INSTANTIATED = false;
        }
    }
}