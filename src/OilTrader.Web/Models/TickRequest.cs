using OilTrader.Contracts;
using System.ComponentModel.DataAnnotations;

namespace OilTrader.Web.Models;

/// <summary>
/// Request object for a new registered tick
/// </summary>
public record TickRequest : IValidatableObject
{
    [Required]
    [MinLength(1)]
    public string? Symbol { get; init; }

    public decimal Bid { get; init; }
    public decimal Ask { get; init; }
    public DateTimeOffset Time { get; init; }
    public long Volume { get; init; }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (Bid <= 0)
        {
            yield return new ValidationResult("Bid must be > 0.", [nameof(Bid)]);
        }

        if (Ask <= 0)
        {
            yield return new ValidationResult("Ask must be > 0.", [nameof(Ask)]);
        }

        if (Ask < Bid)
        {
            yield return new ValidationResult("Ask must be >= Bid.", [nameof(Ask)]);
        }

        if (Time == default)
        {
            yield return new ValidationResult("Time must be set.", [nameof(Time)]);
        }
    }

    /// <summary>
    /// Converts request to a <see cref="Tick"/>
    /// </summary>
    /// <returns>Corresponding tick object</returns>
    public Tick ToDomain() => Tick.Create(
        OilTrader.Contracts.Symbol.From(Symbol!), Price.From(Bid), Price.From(Ask), Time, Volume);
}
