public class NeuronMoveForward : NeuronBase
{
    protected override void DoUpdate(float probability)
    {
        Pawn.MoveForward(1.0f);
    }
}
