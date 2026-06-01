namespace JesusTheChrist.Data;

/// <summary>
/// Read/total counts for a set of references.
/// </summary>
/// <param name="Read">The number of references read.</param>
/// <param name="Total">The total number of references.</param>
public readonly record struct Progress(int Read, int Total)
{
    /// <summary>
    /// Gets the fraction read, in the range [0, 1].
    /// </summary>
    public double Fraction => this.Total == 0 ? 0.0 : (double)this.Read / this.Total;
}
