using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using PokedexWpf.Models;
using PokedexWpf.Services;

namespace PokedexWpf.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly IPokemonRepository _repository;
    private readonly TypeEfficacyService _typeEfficacyService;
    private Pokemon? _selectedPokemon;
    private string _filterCode = string.Empty;
    private string _filterName = string.Empty;
    private PokemonType? _filterType;

    public MainViewModel()
    {
        _repository = new InMemoryPokemonRepository();
        _typeEfficacyService = new TypeEfficacyService();

        Pokemons = new ObservableCollection<Pokemon>(_repository.GetAll());
        Types = new ObservableCollection<PokemonType>(_repository.GetAllTypes());
        Abilities = new ObservableCollection<string>(_repository.GetAllAbilities());

        FilteredPokemon = CollectionViewSource.GetDefaultView(Pokemons);
        FilteredPokemon.Filter = FilterPokemon;

        Editor = new PokemonEditorViewModel(_typeEfficacyService);
        foreach (var type in Types)
        {
            Editor.AvailableTypes.Add(type);
        }

        foreach (var ability in Abilities)
        {
            Editor.AvailableAbilities.Add(ability);
        }

        foreach (var pokemon in Pokemons)
        {
            Editor.AvailableEvolutions.Add(pokemon);
        }

        NewPokemonCommand = new RelayCommand(CreatePokemon);
        DeletePokemonCommand = new RelayCommand(DeleteSelectedPokemon, () => SelectedPokemon != null);
        SavePokemonCommand = new RelayCommand(SavePokemon, () => !Editor.HasErrors && SelectedPokemon != null);
        ClearFiltersCommand = new RelayCommand(ClearFilters, () => HasFilters);

        if (Pokemons.Any())
        {
            SelectedPokemon = Pokemons.First();
        }

        Editor.ErrorsChanged += (_, _) => SavePokemonCommand.RaiseCanExecuteChanged();
    }

    public ObservableCollection<Pokemon> Pokemons { get; }
    public ObservableCollection<PokemonType> Types { get; }
    public ObservableCollection<string> Abilities { get; }
    public ICollectionView FilteredPokemon { get; }
    public PokemonEditorViewModel Editor { get; }

    public RelayCommand NewPokemonCommand { get; }
    public RelayCommand DeletePokemonCommand { get; }
    public RelayCommand SavePokemonCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }

    public Pokemon? SelectedPokemon
    {
        get => _selectedPokemon;
        set
        {
            if (SetProperty(ref _selectedPokemon, value))
            {
                DeletePokemonCommand.RaiseCanExecuteChanged();
                SavePokemonCommand.RaiseCanExecuteChanged();

                if (value != null)
                {
                    Editor.LoadFromPokemon(value.Clone());
                }
            }
        }
    }

    public string FilterCode
    {
        get => _filterCode;
        set
        {
            if (SetProperty(ref _filterCode, value))
            {
                ApplyFilters();
            }
        }
    }

    public string FilterName
    {
        get => _filterName;
        set
        {
            if (SetProperty(ref _filterName, value))
            {
                ApplyFilters();
            }
        }
    }

    public PokemonType? FilterType
    {
        get => _filterType;
        set
        {
            if (SetProperty(ref _filterType, value))
            {
                ApplyFilters();
            }
        }
    }

    public bool HasFilters => !string.IsNullOrWhiteSpace(FilterCode) || !string.IsNullOrWhiteSpace(FilterName) || FilterType != null;

    private bool FilterPokemon(object item)
    {
        if (item is not Pokemon pokemon)
        {
            return false;
        }

        var matchesCode = string.IsNullOrWhiteSpace(FilterCode) || pokemon.Code.Contains(FilterCode, StringComparison.OrdinalIgnoreCase);
        var matchesName = string.IsNullOrWhiteSpace(FilterName) || pokemon.Name.Contains(FilterName, StringComparison.OrdinalIgnoreCase);
        var matchesType = FilterType == null || pokemon.Types.Any(type => type.Id == FilterType.Id);

        return matchesCode && matchesName && matchesType;
    }

    private void ApplyFilters()
    {
        FilteredPokemon.Refresh();
        RaisePropertyChanged(nameof(HasFilters));
        ClearFiltersCommand.RaiseCanExecuteChanged();
    }

    private void ClearFilters()
    {
        FilterCode = string.Empty;
        FilterName = string.Empty;
        FilterType = null;
        ApplyFilters();
    }

    private void CreatePokemon()
    {
        var pokemon = new Pokemon
        {
            Id = Pokemons.Count + 1,
            Code = "NEW",
            Name = "Nou Pokemon",
            Description = "",
            Height = 1.0,
            Weight = 1.0,
        };

        pokemon.Types.Add(Types.First());
        pokemon.Abilities.Add(Abilities.First());
        pokemon.Evolutions.Add(pokemon);

        Pokemons.Add(pokemon);
        Editor.AvailableEvolutions.Add(pokemon);
        SelectedPokemon = pokemon;
    }

    private void DeleteSelectedPokemon()
    {
        if (SelectedPokemon == null)
        {
            return;
        }

        _repository.Delete(SelectedPokemon);
        Pokemons.Remove(SelectedPokemon);
        Editor.AvailableEvolutions.Remove(SelectedPokemon);
        SelectedPokemon = Pokemons.FirstOrDefault();
    }

    private void SavePokemon()
    {
        if (SelectedPokemon == null)
        {
            return;
        }

        Editor.ApplyChanges(SelectedPokemon);
        _repository.Save(SelectedPokemon);
        FilteredPokemon.Refresh();
    }
}
