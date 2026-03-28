using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pooling system to reduce garbage collection
/// Place in: Assets/Scripts/Core/ObjectPool.cs
/// 
/// Usage:
///     // Create a pool
///     ObjectPool<Ball> ballPool = new ObjectPool<Ball>(ballPrefab, 10, transform);
///     
///     // Get object from pool
///     Ball ball = ballPool.Get();
///     
///     // Return object to pool
///     ballPool.Return(ball);
///     
///     // Or use the static manager
///     PoolManager.Instance.CreatePool("Balls", ballPrefab, 10);
///     GameObject ball = PoolManager.Instance.Spawn("Balls", position, rotation);
///     PoolManager.Instance.Despawn("Balls", ball);
/// </summary>

// ═══════════════════════════════════════════════════════════════════════
// GENERIC OBJECT POOL
// ═══════════════════════════════════════════════════════════════════════

public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _available;
    private readonly List<T> _all;
    private readonly bool _expandable;
    private readonly int _maxSize;
    
    public int AvailableCount => _available.Count;
    public int TotalCount => _all.Count;
    public int ActiveCount => _all.Count - _available.Count;
    
    /// <summary>
    /// Creates a new object pool
    /// </summary>
    /// <param name="prefab">Prefab to instantiate</param>
    /// <param name="initialSize">Initial pool size</param>
    /// <param name="parent">Parent transform for pooled objects</param>
    /// <param name="expandable">Can pool grow beyond initial size?</param>
    /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
    public ObjectPool(T prefab, int initialSize, Transform parent = null, bool expandable = true, int maxSize = 0)
    {
        _prefab = prefab;
        _parent = parent;
        _expandable = expandable;
        _maxSize = maxSize;
        _available = new Queue<T>(initialSize);
        _all = new List<T>(initialSize);
        
        // Pre-instantiate objects
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    
    /// <summary>
    /// Gets an object from the pool
    /// </summary>
    public T Get()
    {
        T obj;
        
        if (_available.Count > 0)
        {
            obj = _available.Dequeue();
        }
        else if (_expandable && (_maxSize == 0 || _all.Count < _maxSize))
        {
            obj = CreateNewObject();
        }
        else
        {
            Debug.LogWarning($"[ObjectPool] Pool exhausted for {_prefab.name}");
            return null;
        }
        
        if (obj != null)
        {
            obj.gameObject.SetActive(true);
        }
        
        return obj;
    }
    
    /// <summary>
    /// Gets an object and sets its position/rotation
    /// </summary>
    public T Get(Vector3 position, Quaternion rotation)
    {
        T obj = Get();
        
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        
        return obj;
    }
    
    /// <summary>
    /// Returns an object to the pool
    /// </summary>
    public void Return(T obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[ObjectPool] Attempted to return null object");
            return;
        }
        
        obj.gameObject.SetActive(false);
        
        if (_parent != null)
        {
            obj.transform.SetParent(_parent);
        }
        
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        if (!_available.Contains(obj))
        {
            _available.Enqueue(obj);
        }
    }
    
    /// <summary>
    /// Returns all active objects to the pool
    /// </summary>
    public void ReturnAll()
    {
        foreach (T obj in _all)
        {
            if (obj != null && obj.gameObject.activeInHierarchy)
            {
                Return(obj);
            }
        }
    }
    
    /// <summary>
    /// Destroys all pooled objects
    /// </summary>
    public void Clear()
    {
        foreach (T obj in _all)
        {
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
            }
        }
        
        _all.Clear();
        _available.Clear();
    }
    
    /// <summary>
    /// Pre-warms the pool by creating additional objects
    /// </summary>
    public void PreWarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_maxSize > 0 && _all.Count >= _maxSize)
            {
                break;
            }
            
            CreateNewObject();
        }
    }
    
    private T CreateNewObject()
    {
        T obj = Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        obj.gameObject.name = $"{_prefab.name}_Pooled_{_all.Count}";
        _all.Add(obj);
        _available.Enqueue(obj);
        return obj;
    }
}

// ═══════════════════════════════════════════════════════════════════════
// POOL MANAGER (Singleton for easy access)
// ═══════════════════════════════════════════════════════════════════════

public class PoolManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static PoolManager _instance;
    
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PoolManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[PoolManager]");
                    _instance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // POOL STORAGE
    // ═══════════════════════════════════════════════════════════════
    
    private Dictionary<string, GameObjectPool> _pools = new Dictionary<string, GameObjectPool>();
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            ClearAllPools();
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // POOL MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Creates a new pool
    /// </summary>
    public void CreatePool(string poolName, GameObject prefab, int initialSize, bool expandable = true, int maxSize = 0)
    {
        if (_pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"[PoolManager] Pool '{poolName}' already exists");
            return;
        }
        
        Transform poolParent = new GameObject($"Pool_{poolName}").transform;
        poolParent.SetParent(transform);
        
        GameObjectPool pool = new GameObjectPool(prefab, initialSize, poolParent, expandable, maxSize);
        _pools.Add(poolName, pool);
        
        Debug.Log($"[PoolManager] Created pool '{poolName}' with {initialSize} objects");
    }
    
    /// <summary>
    /// Checks if a pool exists
    /// </summary>
    public bool HasPool(string poolName)
    {
        return _pools.ContainsKey(poolName);
    }
    
    /// <summary>
    /// Gets an object from a pool
    /// </summary>
    public GameObject Spawn(string poolName)
    {
        if (!_pools.TryGetValue(poolName, out GameObjectPool pool))
        {
            Debug.LogError($"[PoolManager] Pool '{poolName}' not found");
            return null;
        }
        
        return pool.Get();
    }
    
    /// <summary>
    /// Gets an object from a pool with position and rotation
    /// </summary>
    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(poolName, out GameObjectPool pool))
        {
            Debug.LogError($"[PoolManager] Pool '{poolName}' not found");
            return null;
        }
        
        return pool.Get(position, rotation);
    }
    
    /// <summary>
    /// Returns an object to its pool
    /// </summary>
    public void Despawn(string poolName, GameObject obj)
    {
        if (!_pools.TryGetValue(poolName, out GameObjectPool pool))
        {
            Debug.LogError($"[PoolManager] Pool '{poolName}' not found");
            return;
        }
        
        pool.Return(obj);
    }
    
    /// <summary>
    /// Returns all objects in a pool
    /// </summary>
    public void DespawnAll(string poolName)
    {
        if (!_pools.TryGetValue(poolName, out GameObjectPool pool))
        {
            Debug.LogError($"[PoolManager] Pool '{poolName}' not found");
            return;
        }
        
        pool.ReturnAll();
    }
    
    /// <summary>
    /// Clears a specific pool
    /// </summary>
    public void ClearPool(string poolName)
    {
        if (!_pools.TryGetValue(poolName, out GameObjectPool pool))
        {
            return;
        }
        
        pool.Clear();
        _pools.Remove(poolName);
    }
    
    /// <summary>
    /// Clears all pools
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Clear();
        }
        
        _pools.Clear();
    }
    
    /// <summary>
    /// Gets pool statistics
    /// </summary>
    public string GetPoolStats(string poolName)
    {
        if (!_pools.TryGetValue(poolName, out GameObjectPool pool))
        {
            return $"Pool '{poolName}' not found";
        }
        
        return $"Pool '{poolName}': {pool.ActiveCount} active, {pool.AvailableCount} available, {pool.TotalCount} total";
    }
    
    /// <summary>
    /// Gets all pool statistics
    /// </summary>
    public string GetAllPoolStats()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[PoolManager] Pool Statistics:");
        
        foreach (var kvp in _pools)
        {
            sb.AppendLine($"  {GetPoolStats(kvp.Key)}");
        }
        
        return sb.ToString();
    }
}

// ═══════════════════════════════════════════════════════════════════════
// GAMEOBJECT POOL (Used by PoolManager)
// ═══════════════════════════════════════════════════════════════════════

public class GameObjectPool
{
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly Queue<GameObject> _available;
    private readonly List<GameObject> _all;
    private readonly bool _expandable;
    private readonly int _maxSize;
    
    public int AvailableCount => _available.Count;
    public int TotalCount => _all.Count;
    public int ActiveCount => _all.Count - _available.Count;
    
    public GameObjectPool(GameObject prefab, int initialSize, Transform parent = null, bool expandable = true, int maxSize = 0)
    {
        _prefab = prefab;
        _parent = parent;
        _expandable = expandable;
        _maxSize = maxSize;
        _available = new Queue<GameObject>(initialSize);
        _all = new List<GameObject>(initialSize);
        
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    
    public GameObject Get()
    {
        GameObject obj;
        
        if (_available.Count > 0)
        {
            obj = _available.Dequeue();
            
            // Check if object was destroyed externally
            if (obj == null)
            {
                _all.RemoveAll(o => o == null);
                return Get();
            }
        }
        else if (_expandable && (_maxSize == 0 || _all.Count < _maxSize))
        {
            obj = CreateNewObject();
            _available.Dequeue(); // Remove from available since we're using it
        }
        else
        {
            Debug.LogWarning($"[GameObjectPool] Pool exhausted for {_prefab.name}");
            return null;
        }
        
        obj.SetActive(true);
        return obj;
    }
    
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = Get();
        
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        
        return obj;
    }
    
    public void Return(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        
        obj.SetActive(false);
        
        if (_parent != null)
        {
            obj.transform.SetParent(_parent);
        }
        
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        if (!_available.Contains(obj))
        {
            _available.Enqueue(obj);
        }
    }
    
    public void ReturnAll()
    {
        foreach (GameObject obj in _all)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                Return(obj);
            }
        }
    }
    
    public void Clear()
    {
        foreach (GameObject obj in _all)
        {
            if (obj != null)
            {
                Object.Destroy(obj);
            }
        }
        
        _all.Clear();
        _available.Clear();
    }
    
    private GameObject CreateNewObject()
    {
        GameObject obj = Object.Instantiate(_prefab, _parent);
        obj.SetActive(false);
        obj.name = $"{_prefab.name}_Pooled_{_all.Count}";
        _all.Add(obj);
        _available.Enqueue(obj);
        return obj;
    }
}

// ═══════════════════════════════════════════════════════════════════════
// POOLED OBJECT COMPONENT (Optional - for auto-return)
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// Attach to prefabs that need auto-return functionality
/// </summary>
public class PooledObject : MonoBehaviour
{
    [SerializeField] private string _poolName;
    [SerializeField] private float _autoReturnDelay = 0f;
    
    private Coroutine _autoReturnCoroutine;
    
    public string PoolName
    {
        get => _poolName;
        set => _poolName = value;
    }
    
    private void OnEnable()
    {
        if (_autoReturnDelay > 0f)
        {
            _autoReturnCoroutine = StartCoroutine(AutoReturnRoutine());
        }
    }
    
    private void OnDisable()
    {
        if (_autoReturnCoroutine != null)
        {
            StopCoroutine(_autoReturnCoroutine);
            _autoReturnCoroutine = null;
        }
    }
    
    private System.Collections.IEnumerator AutoReturnRoutine()
    {
        yield return new WaitForSeconds(_autoReturnDelay);
        ReturnToPool();
    }
    
    /// <summary>
    /// Returns this object to its pool
    /// </summary>
    public void ReturnToPool()
    {
        if (string.IsNullOrEmpty(_poolName))
        {
            Debug.LogWarning($"[PooledObject] No pool name set for {gameObject.name}");
            gameObject.SetActive(false);
            return;
        }
        
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(_poolName, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Returns this object to its pool after a delay
    /// </summary>
    public void ReturnToPool(float delay)
    {
        StartCoroutine(ReturnToPoolDelayed(delay));
    }
    
    private System.Collections.IEnumerator ReturnToPoolDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }
}