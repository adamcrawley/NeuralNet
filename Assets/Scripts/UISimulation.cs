using System.Text;
using TMPro;
using UnityEngine;

public class UISimulation : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private TextMeshProUGUI _fastModeText;
    
    private float _timer;
    private const float _interval = 0.1f;
    private StringBuilder _stringBuilder = new();

    
    
    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        _timer += deltaTime;
        if (_timer >= _interval)
        {
            _timer = 0.0f;
            _stringBuilder.Clear();
            _stringBuilder.Append($"FPS: {Mathf.Round(1.0f / deltaTime)}\n");
            Simulator simulator = Simulator.Instance;
            _stringBuilder.Append($"Simulation Time: {simulator.LastSimulationDuration * 1000.0f:0.00} ms\n");
            _stringBuilder.Append($"Steps per Generation: {simulator.TotalIterations}\n");
            _stringBuilder.Append($"Generation Duration: {simulator.TimePerGeneration:0.00} s\n");
            _stringBuilder.Append($"Current Generation: {simulator.CurrentGeneration}\n");
            _stringBuilder.Append($"Survival Rate: {simulator.SurvivalRate:0.00}%\n\n");
            _stringBuilder.Append($"Pawn Count: {simulator.TotalPawns}\n");
            _stringBuilder.Append($"Genome Length: {simulator.GenomeLength}\n");
            _stringBuilder.Append($"Inner Neurons: {simulator.InnerNeurons}\n");
            _stringBuilder.Append($"Mutation Rate: 1 in {simulator.MutationRate} genes\n\n");
            _text.text = _stringBuilder.ToString();
            _fastModeText.text = simulator.IsFastMode ? "Normal Mode" : "Fast Mode";
        }
    }
}