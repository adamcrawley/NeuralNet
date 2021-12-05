public class NeuronInner : NeuronBase
{
    private readonly int _debugId;
    
    public NeuronInner(int debugId)
    {
        _debugId = debugId;
    }
    
    protected override void DoUpdate(float probability)
    {
        InvokeInternal((float) System.Math.Tanh(Inputs));
    }
    
    protected override string GetDebugName()
    {
        return $"{GetType().Name}{_debugId}";
    }
}