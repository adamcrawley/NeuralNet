public class NeuronSenseSafety : NeuronBase
{
    protected override float Output => Simulator.Instance.SafeBounds.Contains(Pawn.Position) ? 1.0f : 0.0f;
}