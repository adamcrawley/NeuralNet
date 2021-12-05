using System.Collections.Generic;
using UnityEngine;

public class NeuronBase
{
    private readonly struct Connection
    {
        public NeuronBase Target { get; }
        public float Weight { get; }

        public Connection(NeuronBase target, float weight)
        {
            Target = target;
            Weight = weight;
        }
    }

    private readonly List<Connection> _connections = new();
    private readonly Dictionary<NeuronBase, List<float>> _requestedConnections = new();
    private readonly System.Random _random = new();
    protected float Inputs { get; private set; }
    protected virtual float Output => 1.0f;
    public Pawn Pawn { get; set; }

    public void AddConnection(NeuronBase neuron, float weight)
    {
        if (_requestedConnections.TryGetValue(neuron, out List<float> weights))
        {
            weights.Add(weight);
        }
        else
        {
            List<float> weightList = new List<float>();
            weightList.Add(weight);
            _requestedConnections.Add(neuron, weightList);
        }
    }

    public void ClearConnections()
    {
        _connections.Clear();
        foreach (KeyValuePair<NeuronBase, List<float>> kvp in _requestedConnections)
        {
            kvp.Value.Clear();
        }
    }

    public void RebuildConnections()
    {
        foreach (KeyValuePair<NeuronBase, List<float>> kvp in _requestedConnections)
        {
            List<float> weights = kvp.Value;
            int weightCount = weights.Count;
            if (weightCount == 0)
                continue;
            
            float totalWeight = 0.0f;
            for (int i = 0; i < weightCount; ++i)
            {
                totalWeight += weights[i];
            }
            
            _connections.Add(new Connection(kvp.Key, totalWeight / weightCount));
        }
    }

    public void Invoke()
    {
        InvokeInternal(Output);
    }

    protected void InvokeInternal(float output)
    {
        for (int i = 0; i < _connections.Count; ++i)
        {
            Connection connection = _connections[i];
            connection.Target.Stimulate(output * connection.Weight);
        }
    }

    private void Stimulate(float input)
    {
        Inputs += input;
    }

    public void Update()
    {
        double probability = System.Math.Tanh(Inputs);
        if (_random.NextDouble() < probability)
        {
            DoUpdate((float)probability);
        }
        Inputs = 0.0f;
    }

    protected virtual void DoUpdate(float probability) { }

    public void Log()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        for (int i = 0; i < _connections.Count; ++i)
        {
            Connection connection = _connections[i];
            Debug.Log("edge [color=" + (connection.Weight < 0.0f ? "red]" : "green]"));
            Debug.Log($"{GetDebugName()} -> {connection.Target.GetDebugName()}");
        }
    }

    protected virtual string GetDebugName()
    {
        return GetType().Name;
    }
}