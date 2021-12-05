using Unity.Profiling;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField]
    private Material _default;

    [SerializeField]
    private Material _ateFood;

    private int _genomeLength;
    private int _neuronCount;
    private Renderer _renderer;
    
    private PawnGene[] _genome;
    private PawnBrain _brain;
    public bool DidEat { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }
    public float Rotation { get; set; }

    public void Initialize(int genomeLength, int innerNeurons)
    {
        _genomeLength = genomeLength;
        _neuronCount = innerNeurons;
        _genome = new PawnGene[_genomeLength];
        for (int i = 0; i < _genomeLength; ++i)
        {
            _genome[i] = new PawnGene();
        }

        _brain = new PawnBrain(this, _neuronCount);
        _brain.LoadGenome(_genome);
        _renderer = GetComponent<Renderer>();
        Reset();
    }

    public void Reset()
    {
        DidEat = false;
        _renderer.material = _default;
    }

    public void SpawnAsChild(Pawn pawnA, Pawn pawnB)
    {
        int randomOffsetAmount = _genomeLength / 3;
        int genesToCopyFromA = (_genomeLength / 2) + Random.Range(-randomOffsetAmount, randomOffsetAmount + 1);
        int mutationRate = Simulator.Instance.MutationRate;
        for (int i = 0; i < _genomeLength; ++i)
        {
            Pawn parentToCopy = i < genesToCopyFromA ? pawnA : pawnB;
            _genome[i].Copy(parentToCopy._genome[i], Random.Range(0, mutationRate) == 0);
        }

        _brain.LoadGenome(_genome);
    }

    public void SpawnAsNew()
    {
        for (int i = 0; i < _genomeLength; ++i)
        {
            _genome[i].Randomize();
        }

        _brain.LoadGenome(_genome);
    }

    public void PreUpdate()
    {
        using var _ = new ProfilerMarker("Pawn.PreUpdate").Auto();
        var pawnTransform = transform;
        Position = pawnTransform.position;
        Forward = pawnTransform.forward;
        Right = pawnTransform.right;
    }
    
    public void DoUpdate()
    {
        using var _ = new ProfilerMarker("Pawn.Update").Auto();
        _brain.Update();
    }

    public void PostUpdate()
    {
        using var _ = new ProfilerMarker("Pawn.PostUpdate").Auto();
        transform.position = Position;
        transform.Rotate(Vector3.up, Rotation);
        Rotation = 0.0f;
        if (DidEat)
        {
            _renderer.material = _ateFood;
        }
    }

    public void MoveForward(float distance)
    {
        Position += Forward * distance;
    }

    public void TryEat()
    {
        if ((Position - Simulator.Instance.FoodPosition).sqrMagnitude < 100.0f)
        {
            DidEat = true;
        }
    }

    public void Log()
    {
        _brain.LogBrain();
    }
}