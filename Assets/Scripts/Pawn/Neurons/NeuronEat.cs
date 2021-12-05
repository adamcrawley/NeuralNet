public class NeuronEat : NeuronBase
{
    protected override void DoUpdate(float probability)
    {
        Pawn.TryEat();
    }
}