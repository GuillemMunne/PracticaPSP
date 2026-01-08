using PokedexWpf.Models;

namespace PokedexWpf.Services;

public class TypeEfficacyService
{
    private readonly Dictionary<string, Dictionary<string, double>> _table;

    public TypeEfficacyService()
    {
        _table = new Dictionary<string, Dictionary<string, double>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Fire"] = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 0.5,
                ["Water"] = 0.5,
                ["Grass"] = 2.0,
                ["Ice"] = 2.0,
            },
            ["Water"] = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 2.0,
                ["Water"] = 0.5,
                ["Grass"] = 0.5,
            },
            ["Grass"] = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 0.5,
                ["Water"] = 2.0,
                ["Grass"] = 0.5,
            },
            ["Electric"] = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["Water"] = 2.0,
                ["Grass"] = 0.5,
            },
            ["Ice"] = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["Grass"] = 2.0,
                ["Fire"] = 0.5,
            },
            ["Flying"] = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["Grass"] = 2.0,
                ["Electric"] = 0.5,
            },
        };
    }

    public IReadOnlyList<DamageModifier> BuildDefenseModifiers(IEnumerable<PokemonType> defenseTypes)
    {
        var defenseTypeList = defenseTypes.Select(type => type.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (defenseTypeList.Count == 0)
        {
            return Array.Empty<DamageModifier>();
        }

        var attackTypes = _table.Keys.Union(defenseTypeList, StringComparer.OrdinalIgnoreCase).Distinct(StringComparer.OrdinalIgnoreCase);
        var modifiersByLabel = new Dictionary<string, DamageModifier>();

        foreach (var attackType in attackTypes)
        {
            var multiplier = 1.0;
            foreach (var defenseType in defenseTypeList)
            {
                multiplier *= GetMultiplier(attackType, defenseType);
            }

            var label = GetLabel(multiplier);
            if (string.IsNullOrEmpty(label))
            {
                continue;
            }

            if (!modifiersByLabel.TryGetValue(label, out var modifier))
            {
                modifier = new DamageModifier { ModifierLabel = label };
                modifiersByLabel[label] = modifier;
            }

            modifier.Types.Add(new PokemonType { Name = attackType });
        }

        return modifiersByLabel.Values.ToList();
    }

    private double GetMultiplier(string attackType, string defenseType)
    {
        if (_table.TryGetValue(attackType, out var defenseValues) &&
            defenseValues.TryGetValue(defenseType, out var multiplier))
        {
            return multiplier;
        }

        return 1.0;
    }

    private static string GetLabel(double multiplier) => multiplier switch
    {
        0.0 => "x0",
        0.25 => "x1/4",
        0.5 => "x1/2",
        2.0 => "x2",
        4.0 => "x4",
        _ => string.Empty,
    };
}
