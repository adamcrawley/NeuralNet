using UnityEngine;
using Random = UnityEngine.Random;

public class PawnGene
{
    private int _encoding;

    private const int BitsPerId = 7;
    private const int BitsForWeight = 32 - WeightOffset;
    private const int SourceOffset = 0;
    private const int TargetOffset = BitsPerId + 1;
    private const int WeightOffset = TargetOffset + BitsPerId + 1;
    private const int IDMaxValue = (1 << BitsPerId) - 1;
    private const int WeightMaxValue = (1 << BitsForWeight) - 1;
    private const int SourceIdMask = IDMaxValue << 1;
    private const int TargetIdMask = IDMaxValue << TargetOffset + 1;
    private const int SourceTypeMask = 1 << SourceOffset;
    private const int TargetTypeMask = 1 << TargetOffset;
    private const uint WeightMask = (uint) WeightMaxValue << WeightOffset;
    private const float WeightRange = 8.0f;

    public int SourceType { get; private set; }
    public int SourceId { get; private set; }
    public int TargetType { get; private set; }
    public int TargetId { get; private set; }
    public float Weight { get; private set; }

    public PawnGene()
    {
        Randomize();
    }

    public void Randomize()
    {
        _encoding = 0;
        _encoding |= Random.Range(0, 2) << SourceOffset;
        _encoding |= Random.Range(0, IDMaxValue + 1) << SourceOffset + 1;
        _encoding |= Random.Range(0, 2) << TargetOffset;
        _encoding |= Random.Range(0, IDMaxValue + 1) << TargetOffset + 1;
        _encoding |= Random.Range(0, WeightMaxValue) << WeightOffset;
        CacheValues();
    }

    public void Copy(PawnGene other, bool mutate)
    {
        _encoding = other._encoding;

        if (mutate)
        {
            Mutate();
        }
        else
        {
            CacheValues();
        }
    }

    private void CacheValues()
    {
        SourceType = (_encoding & SourceTypeMask) >> SourceOffset;
        SourceId = (_encoding & SourceIdMask) >> SourceOffset + 1;
        TargetType = (_encoding & TargetTypeMask) >> TargetOffset;
        TargetId = (_encoding & TargetIdMask) >> TargetOffset + 1;
        Weight = (((_encoding & WeightMask) >> WeightOffset) - WeightMaxValue / 2) / (WeightMaxValue / WeightRange);
    }

    private void Mutate()
    {
        _encoding ^= 1 << Random.Range(0, 32);
        CacheValues();
    }
}