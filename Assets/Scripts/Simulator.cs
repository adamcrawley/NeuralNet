using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Simulator : MonoBehaviour
{
    [Header("Simulation Setup")]
    [SerializeField]
    [Min(1)]
    private int _iterations = 300;

    [SerializeField]
    private float _fixedTickRate = 1.0f / 30.0f;

    [SerializeField]
    [Min(1)]
    private int _skipCount = 10;

    [SerializeField]
    private AnimationCurve _survivalCurve = new (new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

    [SerializeField]
    private int _pawnsPerThread = 100;
    
    [Header("Pawn Setup")]
    [SerializeField]
    private Pawn _pawnPrefab;

    [SerializeField]
    [Min(1)]
    private int _pawnCount = 1000;
    
    [SerializeField]
    [Min(1)]
    private int _genomeLength = 24;

    [SerializeField]
    [Min(1)]
    private int _mutationRate = 1000;

    [SerializeField]
    [Min(1)]
    private int _innerNeurons = 4;
    
    [SerializeField]
    [Min(0.0f)]
    private float _spawnDistanceRange = 25.0f;
    
    [Header("Objects Setup")]
    [SerializeField]
    private GameObject _food;

    [SerializeField]
    private GameObject _safeZone;

    [SerializeField]
    private bool _autoNudge;

    [SerializeField]
    [Min(0.0f)]
    private float _autoNudgeThreshold = 20.0f;
    
    [SerializeField]
    private float _autoNudgeDistance = 10.0f;

    [SerializeField]
    private float _autoNudgeMaxDistance = 160.0f;
    
    private ObjectPool<Pawn> _pawnPool;
    private List<Pawn> _alivePawns;
    private int _generationCount;
    private float _tickTimer;
    private Vector3 _foodStartPos;
    private Vector3 _safeStartPos;
    private float _generationStartTime;
    private CountdownEvent _countdownEvent;
    private WaitCallback _pawnUpdateCallback;

    public static Simulator Instance { get; private set; }
    public int CurrentIteration { get; private set; }
    public int TotalIterations => _iterations;
    public float TickRate => _fixedTickRate;
    public GameObject SafeZone => _safeZone;
    public GameObject Food => _food;
    public int MutationRate => _mutationRate;
    public Vector3 SafePosition { get; private set; }
    public Vector3 FoodPosition { get; private set; }
    public Bounds SafeBounds { get; private set; }
    public float LastSimulationDuration { get; private set; }
    public float TimePerGeneration { get; private set; }
    public float CurrentGeneration => _generationCount;
    public float GenomeLength => _genomeLength;
    public float InnerNeurons => _innerNeurons;
    public float TotalPawns => _pawnCount;
    public float SurvivalRate { get; private set; }
    public bool IsFastMode { get; private set; }

    private void Awake()
    {
        Instance = this;
        _alivePawns = new List<Pawn>(_pawnCount);
        _pawnPool = new ObjectPool<Pawn>(CreatePawn,
            OnSpawnPawn,
            x => x.gameObject.SetActive(false),
            Destroy,
            false,
            _pawnCount,
            _pawnCount);

        for (int i = 0; i < _pawnCount; ++i)
        {
            _alivePawns.Add(_pawnPool.Get());
        }

        OnStartNewGeneration();
        _foodStartPos = _food.transform.localPosition;
        _safeStartPos = _safeZone.transform.localPosition;
        _countdownEvent = new CountdownEvent(_pawnCount / _pawnsPerThread);
        _pawnUpdateCallback = SimulatePawnUpdate;
    }

    private void OnDestroy()
    {
        _pawnPool.Dispose();
        _countdownEvent.Dispose();
        _alivePawns.Clear();
        Instance = null;
    }

    private Pawn CreatePawn()
    {
        Pawn newPawn = Instantiate(_pawnPrefab, transform);
        newPawn.Initialize(_genomeLength, _innerNeurons);
        return newPawn;
    }
    
    private void OnSpawnPawn(Pawn pawn)
    {
        Vector3 position = new Vector3(Random.Range(-_spawnDistanceRange, _spawnDistanceRange), 0.0f,
            Random.Range(-_spawnDistanceRange, _spawnDistanceRange));
        Vector2 direction = Random.insideUnitCircle.normalized;
        Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
        pawn.transform.SetPositionAndRotation(position, rotation);
        pawn.Reset();
        pawn.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Restart();
            
        if (Input.GetKeyDown(KeyCode.F))
            ToggleFastMode();

        if (Input.GetKeyDown(KeyCode.D))
        {
            _alivePawns[Random.Range(0, _alivePawns.Count)].Log();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Skip();
        }
        else if (IsFastMode)
        {
            _tickTimer = 0.0f;
            Simulate();
        }
        else
        {
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= TickRate)
            {
                _tickTimer -= TickRate;
                Simulate();
            }
        }
    }

    private void Restart()
    {
        for (int i = _alivePawns.Count - 1; i >= 0; --i)
        {
            Pawn pawn = _alivePawns[i];
            _pawnPool.Release(pawn);
        }
        _alivePawns.Clear();

        while (_alivePawns.Count < _pawnCount)
        {
            Pawn child = _pawnPool.Get();
            child.SpawnAsNew();
            _alivePawns.Add(child);
        }

        _food.transform.localPosition = _foodStartPos;
        _safeZone.transform.localPosition = _safeStartPos;
        _generationCount = 0;
        SurvivalRate = 0.0f;
        OnStartNewGeneration();
    }

    private void OnStartNewGeneration()
    {
        // do
        // {
        //     float range = _spawnDistanceRange * Mathf.Min(_generationCount + 1, 3);
        //     _safeZone.transform.position = new Vector3(Random.Range(-range, range), 0.0f,
        //         Random.Range(-range, range));
        //     _food.transform.position = new Vector3(Random.Range(-range, range), 0.0f,
        //         Random.Range(-range, range));
        // } while (Vector3.Distance(_safeZone.transform.position, _food.transform.position) < 40.0f);
        _generationStartTime = Time.realtimeSinceStartup;
        CurrentIteration = 0;
    }
    
    private void Simulate()
    {
        float startTime = Time.realtimeSinceStartup;
        SafePosition = _safeZone.transform.position;
        SafeBounds = new Bounds(SafePosition, _safeZone.transform.localScale);
        FoodPosition = _food.transform.position;
        
        if (CurrentIteration >= _iterations)
        {
            CurrentIteration = 0;
            float zoneDistances = Vector3.Distance(FoodPosition, SafePosition);
            for (int i = _alivePawns.Count - 1; i >= 0; --i)
            {
                Pawn pawn = _alivePawns[i];
                Vector3 pos = pawn.transform.position;
                bool didEat = pawn.DidEat;
                bool foodChance = didEat && Random.Range(0.0f, 1.0f) < 
                    _survivalCurve.Evaluate(1.0f - Mathf.Clamp01(Vector3.Distance(SafePosition, pos) / zoneDistances));
                if (SafeBounds.Contains(pos) && didEat || foodChance)
                    continue;

                _pawnPool.Release(pawn);
                _alivePawns.RemoveAt(i);
            }

            int survivorCount = _alivePawns.Count;
            if (survivorCount < 2)
            {
                Debug.Log("Generation Failed. Starting Over.");
                Restart();
                return;
            }
            
            Pawn[] parents = _alivePawns.ToArray();
            for (int i = survivorCount - 1; i >= 0; --i)
            {
                Pawn pawn = _alivePawns[i];
                _pawnPool.Release(pawn);
            }
            _alivePawns.Clear();
            
            while (_alivePawns.Count < _pawnCount)
            {
                Pawn parentA = parents[Random.Range(0, survivorCount)];
                Pawn parentB = parentA;
                while (parentA != parentB)
                {
                    parentB = parents[Random.Range(0, survivorCount)];
                }

                Pawn child = _pawnPool.Get();
                child.SpawnAsChild(parentA, parentB);
                _alivePawns.Add(child);
            }

            SurvivalRate = (float) survivorCount / _pawnCount * 100.0f;
            Debug.Log($"Generation {_generationCount} survival rate: {SurvivalRate}");
            Debug.Log($"Generation {++_generationCount}");

            if (_autoNudge && SurvivalRate > _autoNudgeThreshold && zoneDistances < _autoNudgeMaxDistance)
            {
                _food.transform.localPosition += Vector3.right * _autoNudgeDistance;
                _safeZone.transform.localPosition += Vector3.left * _autoNudgeDistance;
            }
            
            TimePerGeneration = Time.realtimeSinceStartup - _generationStartTime;
            OnStartNewGeneration();
            return;
        }
        
        for (int i = 0; i < _alivePawns.Count; ++i)
        {
            _alivePawns[i].PreUpdate();
        }

        int threadCount = _pawnCount / _pawnsPerThread;
        _countdownEvent.Reset(threadCount);
        for (int i = 0; i < threadCount; ++i)
        {
            int startIndex = i * _pawnsPerThread;
            ThreadPool.QueueUserWorkItem(_pawnUpdateCallback, startIndex);
        }

        _countdownEvent.Wait();

        for (int i = 0; i < _alivePawns.Count; ++i)
        {
            _alivePawns[i].PostUpdate();
        }
        
        ++CurrentIteration;
        LastSimulationDuration = Time.realtimeSinceStartup - startTime;
    }

    private void SimulatePawnUpdate(object index)
    {
        int startIndex = (int) index;
        for (int i = 0; i < _pawnsPerThread; ++i)
        {
            _alivePawns[i + startIndex].DoUpdate();
        }
        _countdownEvent.Signal();
    }

    public void ToggleFastMode()
    {
        IsFastMode = !IsFastMode;
    }

    public void Reset()
    {
        Restart();
    }

    public void Skip()
    {
        int currentGeneration = _generationCount;
        while (_generationCount < currentGeneration + _skipCount)
        {
            Simulate();
        }   
    }
}