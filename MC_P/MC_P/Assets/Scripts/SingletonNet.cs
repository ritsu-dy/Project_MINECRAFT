using Unity.Netcode;
using UnityEngine;

public class SingletonNet<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find an existing instance in the scene
                _instance = FindObjectOfType<T>();

                // If no instance is found, create a new one
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // Ensure there's only one instance
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Destroy duplicates
        }
    }
}
