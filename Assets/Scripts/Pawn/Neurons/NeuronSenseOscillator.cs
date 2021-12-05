using UnityEngine;

public class NeuronSenseOscillator : NeuronBase
{
    private const float WavesPerSecond = 1.0f;
    protected override float Output => Mathf.Sin((Mathf.PI / 2) * Simulator.Instance.TickRate * WavesPerSecond);
}