using Lucky.Framework;

namespace Lucky.Kits.Managers
{
    public class Singleton<T> : ManagedBehaviour where T : Singleton<T>
    {
        private static T instance;

        public static T Instance => instance;
        public bool dontDestroyOnLoad = false;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
                if (transform.parent == null && dontDestroyOnLoad)
                    DontDestroyOnLoad(instance);
            }
            else
                Destroy(gameObject);
        }
    }
}