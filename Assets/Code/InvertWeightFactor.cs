public class InvertWeightFactor : Factor
{
    private WeightedSumFusion _weightSumFusion;

    public InvertWeightFactor(WeightedSumFusion weightSumFactor)
    {
        _weightSumFusion = weightSumFactor;
    }

    public override float getValue()
    {
        return 1 - _weightSumFusion.getValue();
    }
}
