using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using PokedexWpf.Models;
using PokedexWpf.Services;

namespace PokedexWpf.ViewModels;

public class PokemonEditorViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();
    private readonly TypeEfficacyService _typeEfficacyService;
    private Pokemon? _pokemon;
    private string _name = string.Empty;
    private string _code = string.Empty;
    private string _description = string.Empty;
    private double _height;
    private double _weight;
    private PokemonType? _selectedType;
    private PokemonType? _selectedAssignedType;
    private string? _selectedAbility;
    private Pokemon? _selectedEvolution;
    private int _hp;
    private int _attack;
    private int _defense;
    private int _specialAttack;
    private int _specialDefense;
    private int _speed;

    public PokemonEditorViewModel(TypeEfficacyService typeEfficacyService)
    {
        _typeEfficacyService = typeEfficacyService;
        SelectedTypes.CollectionChanged += (_, _) =>
        {
            UpdateDefenseModifiers();
            ValidateSelectedTypes();
        };
        Evolutions.CollectionChanged += (_, _) => ValidateEvolutions();
        SelectedAbilities.CollectionChanged += (_, _) => ValidateAbilities();

        AddTypeCommand = new RelayCommand(AddType, () => SelectedType != null);
        RemoveTypeCommand = new RelayCommand(RemoveSelectedType, () => SelectedAssignedType != null);
        AddAbilityCommand = new RelayCommand(AddAbility, () => !string.IsNullOrWhiteSpace(SelectedAbility));
        RemoveAbilityCommand = new RelayCommand(RemoveSelectedAbility, () => SelectedAbilities.Any());
        AddEvolutionCommand = new RelayCommand(AddEvolution, () => SelectedEvolution != null);
        RemoveEvolutionCommand = new RelayCommand(RemoveEvolution, () => SelectedEvolution != null && Evolutions.Count > 1);
        MoveEvolutionUpCommand = new RelayCommand(MoveEvolutionUp, () => SelectedEvolution != null);
        MoveEvolutionDownCommand = new RelayCommand(MoveEvolutionDown, () => SelectedEvolution != null);
    }

    public ObservableCollection<PokemonType> AvailableTypes { get; } = new();
    public ObservableCollection<string> AvailableAbilities { get; } = new();
    public ObservableCollection<Pokemon> AvailableEvolutions { get; } = new();
    public ObservableCollection<PokemonType> SelectedTypes { get; } = new();
    public ObservableCollection<string> SelectedAbilities { get; } = new();
    public ObservableCollection<Pokemon> Evolutions { get; } = new();
    public ObservableCollection<DamageModifier> DefenseModifiers { get; } = new();

    public RelayCommand AddTypeCommand { get; }
    public RelayCommand RemoveTypeCommand { get; }
    public RelayCommand AddAbilityCommand { get; }
    public RelayCommand RemoveAbilityCommand { get; }
    public RelayCommand AddEvolutionCommand { get; }
    public RelayCommand RemoveEvolutionCommand { get; }
    public RelayCommand MoveEvolutionUpCommand { get; }
    public RelayCommand MoveEvolutionDownCommand { get; }

    public Pokemon? Pokemon => _pokemon;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                ValidateRequired(nameof(Name), _name);
            }
        }
    }

    public string Code
    {
        get => _code;
        set
        {
            if (SetProperty(ref _code, value))
            {
                ValidateRequired(nameof(Code), _code);
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                ValidateRequired(nameof(Description), _description);
            }
        }
    }

    public double Height
    {
        get => _height;
        set
        {
            if (SetProperty(ref _height, value))
            {
                ValidateRange(nameof(Height), _height, 0.1, 20);
            }
        }
    }

    public double Weight
    {
        get => _weight;
        set
        {
            if (SetProperty(ref _weight, value))
            {
                ValidateRange(nameof(Weight), _weight, 0.1, 1000);
            }
        }
    }

    public int Hp
    {
        get => _hp;
        set
        {
            if (SetProperty(ref _hp, value))
            {
                ValidateRange(nameof(Hp), _hp, 1, 255);
            }
        }
    }

    public int Attack
    {
        get => _attack;
        set
        {
            if (SetProperty(ref _attack, value))
            {
                ValidateRange(nameof(Attack), _attack, 1, 255);
            }
        }
    }

    public int Defense
    {
        get => _defense;
        set
        {
            if (SetProperty(ref _defense, value))
            {
                ValidateRange(nameof(Defense), _defense, 1, 255);
            }
        }
    }

    public int SpecialAttack
    {
        get => _specialAttack;
        set
        {
            if (SetProperty(ref _specialAttack, value))
            {
                ValidateRange(nameof(SpecialAttack), _specialAttack, 1, 255);
            }
        }
    }

    public int SpecialDefense
    {
        get => _specialDefense;
        set
        {
            if (SetProperty(ref _specialDefense, value))
            {
                ValidateRange(nameof(SpecialDefense), _specialDefense, 1, 255);
            }
        }
    }

    public int Speed
    {
        get => _speed;
        set
        {
            if (SetProperty(ref _speed, value))
            {
                ValidateRange(nameof(Speed), _speed, 1, 255);
            }
        }
    }

    public PokemonType? SelectedType
    {
        get => _selectedType;
        set
        {
            if (SetProperty(ref _selectedType, value))
            {
                AddTypeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public PokemonType? SelectedAssignedType
    {
        get => _selectedAssignedType;
        set
        {
            if (SetProperty(ref _selectedAssignedType, value))
            {
                RemoveTypeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? SelectedAbility
    {
        get => _selectedAbility;
        set
        {
            if (SetProperty(ref _selectedAbility, value))
            {
                AddAbilityCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public Pokemon? SelectedEvolution
    {
        get => _selectedEvolution;
        set
        {
            if (SetProperty(ref _selectedEvolution, value))
            {
                RemoveEvolutionCommand.RaiseCanExecuteChanged();
                MoveEvolutionUpCommand.RaiseCanExecuteChanged();
                MoveEvolutionDownCommand.RaiseCanExecuteChanged();
                AddEvolutionCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return Enumerable.Empty<string>();
        }

        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    public void LoadFromPokemon(Pokemon pokemon)
    {
        _pokemon = pokemon;
        Name = pokemon.Name;
        Code = pokemon.Code;
        Description = pokemon.Description;
        Height = pokemon.Height;
        Weight = pokemon.Weight;
        Hp = pokemon.Stats.Hp;
        Attack = pokemon.Stats.Attack;
        Defense = pokemon.Stats.Defense;
        SpecialAttack = pokemon.Stats.SpecialAttack;
        SpecialDefense = pokemon.Stats.SpecialDefense;
        Speed = pokemon.Stats.Speed;

        SelectedTypes.Clear();
        foreach (var type in pokemon.Types)
        {
            SelectedTypes.Add(type);
        }

        SelectedAbilities.Clear();
        foreach (var ability in pokemon.Abilities)
        {
            SelectedAbilities.Add(ability);
        }

        Evolutions.Clear();
        foreach (var evolution in pokemon.Evolutions)
        {
            Evolutions.Add(evolution);
        }

        if (Evolutions.Count == 0)
        {
            Evolutions.Add(pokemon);
        }

        ValidateAll();
        UpdateDefenseModifiers();
    }

    public void ApplyChanges(Pokemon pokemon)
    {
        pokemon.Name = Name;
        pokemon.Code = Code;
        pokemon.Description = Description;
        pokemon.Height = Height;
        pokemon.Weight = Weight;

        pokemon.Types.Clear();
        foreach (var type in SelectedTypes)
        {
            pokemon.Types.Add(type);
        }

        pokemon.Abilities.Clear();
        foreach (var ability in SelectedAbilities)
        {
            pokemon.Abilities.Add(ability);
        }

        pokemon.Evolutions.Clear();
        foreach (var evolution in Evolutions)
        {
            pokemon.Evolutions.Add(evolution);
        }

        pokemon.Stats.Hp = Hp;
        pokemon.Stats.Attack = Attack;
        pokemon.Stats.Defense = Defense;
        pokemon.Stats.SpecialAttack = SpecialAttack;
        pokemon.Stats.SpecialDefense = SpecialDefense;
        pokemon.Stats.Speed = Speed;
    }

    private void AddType()
    {
        if (SelectedType != null && !SelectedTypes.Contains(SelectedType))
        {
            SelectedTypes.Add(SelectedType);
        }
    }

    private void RemoveSelectedType()
    {
        if (SelectedAssignedType != null)
        {
            SelectedTypes.Remove(SelectedAssignedType);
        }
    }

    private void AddAbility()
    {
        if (!string.IsNullOrWhiteSpace(SelectedAbility) && !SelectedAbilities.Contains(SelectedAbility))
        {
            SelectedAbilities.Add(SelectedAbility);
        }
    }

    private void RemoveSelectedAbility()
    {
        if (SelectedAbility != null)
        {
            SelectedAbilities.Remove(SelectedAbility);
        }
    }

    private void AddEvolution()
    {
        if (SelectedEvolution != null && !Evolutions.Contains(SelectedEvolution))
        {
            Evolutions.Add(SelectedEvolution);
        }
    }

    private void RemoveEvolution()
    {
        if (SelectedEvolution != null && Evolutions.Contains(SelectedEvolution) && Evolutions.Count > 1)
        {
            Evolutions.Remove(SelectedEvolution);
        }
    }

    private void MoveEvolutionUp()
    {
        if (SelectedEvolution == null)
        {
            return;
        }

        var index = Evolutions.IndexOf(SelectedEvolution);
        if (index > 0)
        {
            Evolutions.Move(index, index - 1);
        }
    }

    private void MoveEvolutionDown()
    {
        if (SelectedEvolution == null)
        {
            return;
        }

        var index = Evolutions.IndexOf(SelectedEvolution);
        if (index >= 0 && index < Evolutions.Count - 1)
        {
            Evolutions.Move(index, index + 1);
        }
    }

    private void UpdateDefenseModifiers()
    {
        DefenseModifiers.Clear();
        foreach (var modifier in _typeEfficacyService.BuildDefenseModifiers(SelectedTypes))
        {
            DefenseModifiers.Add(modifier);
        }
    }

    private void ValidateRequired(string propertyName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            AddError(propertyName, "Aquest camp és obligatori.");
        }
        else
        {
            ClearErrors(propertyName);
        }
    }

    private void ValidateRange(string propertyName, double value, double min, double max)
    {
        if (value < min || value > max)
        {
            AddError(propertyName, $"Valor ha d'estar entre {min} i {max}.");
        }
        else
        {
            ClearErrors(propertyName);
        }
    }

    private void ValidateRange(string propertyName, int value, int min, int max)
    {
        if (value < min || value > max)
        {
            AddError(propertyName, $"Valor ha d'estar entre {min} i {max}.");
        }
        else
        {
            ClearErrors(propertyName);
        }
    }

    private void ValidateSelectedTypes()
    {
        if (!SelectedTypes.Any())
        {
            AddError(nameof(SelectedTypes), "Cal seleccionar com a mínim un tipus.");
        }
        else
        {
            ClearErrors(nameof(SelectedTypes));
        }
    }

    private void ValidateAbilities()
    {
        if (!SelectedAbilities.Any())
        {
            AddError(nameof(SelectedAbilities), "Cal afegir com a mínim una habilitat.");
        }
        else
        {
            ClearErrors(nameof(SelectedAbilities));
        }
    }

    private void ValidateEvolutions()
    {
        if (!Evolutions.Any())
        {
            AddError(nameof(Evolutions), "Cal afegir com a mínim una evolució.");
        }
        else
        {
            ClearErrors(nameof(Evolutions));
        }
    }

    private void ValidateAll()
    {
        ValidateRequired(nameof(Name), Name);
        ValidateRequired(nameof(Code), Code);
        ValidateRequired(nameof(Description), Description);
        ValidateRange(nameof(Height), Height, 0.1, 20);
        ValidateRange(nameof(Weight), Weight, 0.1, 1000);
        ValidateRange(nameof(Hp), Hp, 1, 255);
        ValidateRange(nameof(Attack), Attack, 1, 255);
        ValidateRange(nameof(Defense), Defense, 1, 255);
        ValidateRange(nameof(SpecialAttack), SpecialAttack, 1, 255);
        ValidateRange(nameof(SpecialDefense), SpecialDefense, 1, 255);
        ValidateRange(nameof(Speed), Speed, 1, 255);
        ValidateSelectedTypes();
        ValidateAbilities();
        ValidateEvolutions();
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out var errors))
        {
            errors = new List<string>();
            _errors[propertyName] = errors;
        }

        if (!errors.Contains(error))
        {
            errors.Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
