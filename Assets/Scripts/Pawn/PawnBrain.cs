using System.Linq;
using Unity.Profiling;

public class PawnBrain
{
    private readonly NeuronBase[] _sensorNeurons;
    private readonly NeuronBase[] _innerNeurons;
    private readonly NeuronBase[] _actionNeurons;
    private readonly NeuronBase[] _allNeurons;

    public PawnBrain(Pawn owner, int innerNodeCount)
    {
        Simulator simulator = Simulator.Instance;
        _sensorNeurons = new NeuronBase[]
        {
            new NeuronSenseOscillator(),
            new NeuronSenseObject(simulator.SafeZone, NeuronSenseObject.SenseMode.Distance),
            new NeuronSenseObject(simulator.SafeZone, NeuronSenseObject.SenseMode.Forward),
            new NeuronSenseObject(simulator.SafeZone, NeuronSenseObject.SenseMode.Right),
            new NeuronSenseObject(simulator.Food, NeuronSenseObject.SenseMode.Distance),
            new NeuronSenseObject(simulator.Food, NeuronSenseObject.SenseMode.Forward),
            new NeuronSenseObject(simulator.Food, NeuronSenseObject.SenseMode.Right),
            new NeuronSenseAge(),
            new NeuronSenseHunger(),
            new NeuronSenseSafety()
        };

        _actionNeurons = new NeuronBase[]
        {
            new NeuronMoveForward(),
            new NeuronRotate(45.0f),
            new NeuronRotate(-45.0f),
            new NeuronEat()
        };

        _innerNeurons = new NeuronBase[innerNodeCount];
        for (int i = 0; i < innerNodeCount; ++i)
        {
            _innerNeurons[i] = new NeuronInner(i);
        }

        _allNeurons = _sensorNeurons.Concat(_actionNeurons).Concat(_innerNeurons).ToArray();
        foreach (NeuronBase node in _allNeurons)
        {
            node.Pawn = owner;
        }
    }

    public void LoadGenome(PawnGene[] genomes)
    {
        foreach (NeuronBase node in _allNeurons)
            node.ClearConnections();

        foreach (PawnGene genome in genomes)
        {
            NeuronBase[] sourceArray = genome.SourceType == 1 ? _sensorNeurons : _innerNeurons;
            NeuronBase[] targetArray = genome.TargetType == 1 ? _actionNeurons : _innerNeurons;
            int sourceId = genome.SourceId % sourceArray.Length;
            int targetId = genome.TargetId % targetArray.Length;
            sourceArray[sourceId].AddConnection(targetArray[targetId], genome.Weight);
        }
        
        foreach (NeuronBase node in _allNeurons)
            node.RebuildConnections();
    }

    public void LogBrain()
    {
        foreach (NeuronBase node in _allNeurons)
        {
            node.Log();
        }
    }

    public void Update()
    {
        using var _ = new ProfilerMarker("PawnBrain.Update").Auto();
        
        Invoke(_sensorNeurons);
        Invoke(_innerNeurons);
        UpdateNodes(_innerNeurons);
        UpdateNodes(_actionNeurons);
    }

    private void Invoke(NeuronBase[] nodes)
    {
        using var _ = new ProfilerMarker("PawnBrain.Invoke").Auto();
        foreach (NeuronBase node in nodes)
            node.Invoke();
    }

    private void UpdateNodes(NeuronBase[] nodes)
    {
        using var _ = new ProfilerMarker("PawnBrain.UpdateNodes").Auto();
        foreach (NeuronBase node in nodes)
            node.Update();
    }
}