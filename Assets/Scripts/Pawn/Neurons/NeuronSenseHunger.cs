public class NeuronSenseHunger : NeuronBase
{
    protected override float Output => Pawn.DidEat ? 0.0f : 1.0f;
}