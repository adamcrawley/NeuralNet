public class NeuronSenseAge : NeuronBase
{
    protected override float Output
    {
        get
        {
            Simulator simulator = Simulator.Instance;
            return (float) simulator.CurrentIteration / simulator.TotalIterations;
        }
    }
}