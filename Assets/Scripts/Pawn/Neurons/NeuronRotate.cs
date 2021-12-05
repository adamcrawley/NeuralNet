public class NeuronRotate : NeuronBase
{
    private readonly float _amount;

    public NeuronRotate(float amount)
    {
        _amount = amount;
    }
    
    protected override void DoUpdate(float probability)
    {
        Pawn.Rotation += _amount;
    }
    
    protected override string GetDebugName()
    {
        return $"{GetType().Name}" + (_amount < 0 ? "Left" : "Right");
    }
}