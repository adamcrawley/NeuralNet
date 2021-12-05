using UnityEngine;

public class NeuronSenseObject : NeuronBase
{
    protected override float Output
    {
        get
        {
            Vector3 position = Simulator.Instance.Food == _gameObject
                ? Simulator.Instance.FoodPosition
                : Simulator.Instance.SafePosition;
            Vector3 toObject = position - Pawn.Position;
            
            switch(_senseMode)
            {
                case SenseMode.Forward:
                    float forwardToObject = Vector3.Dot(Pawn.Forward, toObject.normalized);
                    return (forwardToObject + 1.0f) / 2.0f;
                case SenseMode.Right:
                    float rightToObject = Vector3.Dot(Pawn.Right, toObject.normalized);
                    return (rightToObject + 1.0f) / 2.0f;
                case SenseMode.Distance:
                    return 1.0f - Mathf.Clamp01(toObject.magnitude / 200.0f);
            }

            return 0.0f;
        }
    }

    private readonly GameObject _gameObject;
    private readonly SenseMode _senseMode;

    public enum SenseMode
    {
        Forward,
        Right,
        Distance
    }
    
    public NeuronSenseObject(GameObject gameObject, SenseMode senseMode)
    {
        _gameObject = gameObject;
        _senseMode = senseMode;
    }

    protected override string GetDebugName()
    {
        return $"{GetType().Name}{_gameObject.name}{_senseMode}";
    }
}
