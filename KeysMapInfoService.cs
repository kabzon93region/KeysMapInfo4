using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace KeysMapInfo4;

public class KeysMapInfoConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("format")]
    public string Format { get; set; } = " ({mapName})";

    [JsonPropertyName("excludeMarkedKeys")]
    public bool ExcludeMarkedKeys { get; set; } = false;

    [JsonPropertyName("excludeJunkKeys")]
    public bool ExcludeJunkKeys { get; set; } = true;

    [JsonPropertyName("debug")]
    public bool Debug { get; set; } = false;

    /// <summary>Языки локализации для подписи ключей. По умолчанию только ru.</summary>
    [JsonPropertyName("locales")]
    public List<string> Locales { get; set; } = ["ru"];

    [JsonPropertyName("maps")]
    public Dictionary<string, string> Maps { get; set; } = new();

    [JsonPropertyName("keyTypes")]
    public KeysMapInfoKeyTypes KeyTypes { get; set; } = new();

    [JsonPropertyName("colorCoding")]
    public KeysMapInfoColorCodingConfig ColorCoding { get; set; } = new();
}

public class KeysMapInfoColorCodingConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("changeMarkedKeysBackground")]
    public bool ChangeMarkedKeysBackground { get; set; } = false;

    [JsonPropertyName("backgroundColors")]
    public Dictionary<string, string> BackgroundColors { get; set; } = new()
    {
        ["56f40101d2720b2a4d8b45d6"] = "blue",       // Customs
        ["55f2d3fd4bdc2d5f408b4567"] = "grey",       // Factory
        ["5714dbc024597771384a510d"] = "violet",     // Interchange
        ["5b0fc42d86f7744a585f9105"] = "tracerYellow", // Laboratory
        ["5704e4dad2720bb55b8b4567"] = "tracerRed",  // Lighthouse
        ["5704e5fad2720bc05b8b4567"] = "red",        // Reserve
        ["5704e554d2720bac5b8b456e"] = "orange",     // Shoreline
        ["5704e3c2d2720bac5b8b4567"] = "green",      // Woods
        ["5714dc692459777137212e12"] = "tracerGreen", // Streets
        ["junkKeys"] = "black"
    };
}

public class KeysMapInfoKeyTypes
{
    [JsonPropertyName("mechanical")]
    public string Mechanical { get; set; } = "5c99f98d86f7745c314214b3";

    [JsonPropertyName("keycard")]
    public string Keycard { get; set; } = "5c164d2286f774194c5e69fa";
}

public class KeysDatabase
{
    public Dictionary<string, List<string>> Maps { get; set; } = new();
    public List<string> MarkedKeys { get; set; } = new();
    public List<string> JunkKeys { get; set; } = new();
}

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class KeysMapInfoService(
    ISptLogger<KeysMapInfoService> logger,
    DatabaseServer databaseServer,
    ModHelper modHelper,
    LocaleService localeService
) : IOnLoad
{
    private KeysMapInfoConfig _config = null!;
    private KeysDatabase _keysDb = null!;

    public Task OnLoad()
    {
        try
        {
            LoadConfig();
            LoadKeysDatabase();
            var processed = ProcessKeys();
            if (processed.Processed == 0 && _config.Enabled)
            {
                logger.Warning(
                    $"KeysMapInfo4: Ключи не обработаны (карт в конфиге: {_config.Maps.Count}, " +
                    $"карт в keys.json: {_keysDb.Maps.Count}). Проверьте config.json и keys.json.");
            }
            else
            {
                logger.Success(
                    $"KeysMapInfo4: Мод загружен. Обработано ключей: {processed.Processed}, пропущено: {processed.Skipped}, " +
                    $"локализаций: {processed.LocaleUpdates}, цветов: {processed.ColorUpdates}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"KeysMapInfo4: Критическая ошибка: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private void LoadConfig()
    {
        try
        {
            var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            _config = modHelper.GetJsonDataFromFile<KeysMapInfoConfig>(modPath, "config.json");

            if (_config.Debug)
            {
                logger.Info($"KeysMapInfo4: Конфигурация загружена: enabled={_config.Enabled}, format={_config.Format}, " +
                            $"excludeMarked={_config.ExcludeMarkedKeys}, excludeJunk={_config.ExcludeJunkKeys}, " +
                            $"locales={string.Join(",", _config.Locales)}, " +
                            $"maps={string.Join(",", _config.Maps.Keys)}");
            }

            logger.Info($"KeysMapInfo4: Конфигурация загружена: {_config.Maps.Count} карт");
            if (_config.Maps.Count == 0)
            {
                logger.Error("KeysMapInfo4: Словарь maps пуст — проверьте config.json (формат camelCase из SPT 3)");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"KeysMapInfo4: Ошибка загрузки конфигурации: {ex.Message}");
            _config = new KeysMapInfoConfig();
        }
    }

    private void LoadKeysDatabase()
    {
        try
        {
            var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            _keysDb = modHelper.GetJsonDataFromFile<KeysDatabase>(modPath, "keys.json");

            if (_config.Debug)
            {
                logger.Info($"KeysMapInfo4: База данных ключей загружена: {_keysDb.Maps.Count} карт");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"KeysMapInfo4: Ошибка загрузки базы данных ключей: {ex.Message}");
            _keysDb = new KeysDatabase();
        }
    }

    private (int Processed, int Skipped, int LocaleUpdates, int ColorUpdates) ProcessKeys()
    {
        if (!_config.Enabled)
        {
            logger.Info("KeysMapInfo4: Мод отключён в конфигурации");
            return (0, 0, 0, 0);
        }

        var tables = databaseServer.GetTables();
        if (tables.Templates?.Items == null)
        {
            logger.Error("KeysMapInfo4: База данных игры недоступна");
            return (0, 0, 0, 0);
        }

        var localeIds = ResolveTargetLocales();
        if (localeIds.Count == 0)
        {
            logger.Error("KeysMapInfo4: Нет доступных языков из config.locales — проверьте config.json");
            return (0, 0, 0, 0);
        }

        var items = tables.Templates.Items;

        logger.Info($"KeysMapInfo4: Обработка ключей для языков: {string.Join(", ", localeIds)}");

        var processedKeys = 0;
        var skippedKeys = 0;
        var updatedNames = 0;
        var coloredKeys = 0;

        foreach (var (mapId, keyIds) in _keysDb.Maps)
        {
            if (!_config.Maps.TryGetValue(mapId, out var mapName))
            {
                logger.Error($"KeysMapInfo4: Неизвестная карта: {mapId}");
                continue;
            }

            foreach (var keyId in keyIds)
            {
                if (ShouldProcessKey(keyId))
                {
                    var (localeUpdates, colorUpdates) = ProcessKey(items, localeIds, keyId, mapName, mapId);
                    updatedNames += localeUpdates;
                    coloredKeys += colorUpdates;
                    processedKeys++;
                }
                else
                {
                    skippedKeys++;
                }
            }
        }

        coloredKeys += ApplyJunkKeyColors(items);

        logger.Info(
            $"KeysMapInfo4: Обработано ключей: {processedKeys}, пропущено: {skippedKeys}, " +
            $"обновлено строк локализации: {updatedNames}, окрашено ключей: {coloredKeys}");
        return (processedKeys, skippedKeys, updatedNames, coloredKeys);
    }

    private List<string> ResolveTargetLocales()
    {
        var requested = _config.Locales is { Count: > 0 }
            ? _config.Locales
            : ["ru"];

        var supported = localeService.GetServerSupportedLocales();
        if (supported == null || supported.Count == 0)
        {
            logger.Error("KeysMapInfo4: Сервер не сообщил поддерживаемые языки локализации");
            return [];
        }

        var resolved = new List<string>();
        foreach (var localeId in requested)
        {
            if (string.IsNullOrWhiteSpace(localeId))
                continue;

            var normalized = localeId.Trim().ToLowerInvariant();
            if (supported.Contains(normalized))
            {
                if (!resolved.Contains(normalized))
                    resolved.Add(normalized);
                continue;
            }

            logger.Warning($"KeysMapInfo4: Язык '{localeId}' не найден на сервере, пропуск");
        }

        return resolved;
    }

    private bool ShouldProcessKey(string keyId)
    {
        if (_config.ExcludeMarkedKeys && _keysDb.MarkedKeys.Contains(keyId))
        {
            if (_config.Debug)
                logger.Debug($"KeysMapInfo4: Пропущен помеченный ключ: {keyId}");
            return false;
        }

        if (_config.ExcludeJunkKeys && _keysDb.JunkKeys.Contains(keyId))
        {
            if (_config.Debug)
                logger.Debug($"KeysMapInfo4: Пропущен мусорный ключ: {keyId}");
            return false;
        }

        return true;
    }

    private (int LocaleUpdates, int ColorUpdates) ProcessKey(
        Dictionary<MongoId, TemplateItem> items,
        IEnumerable<string> localeIds,
        string keyId,
        string mapName,
        string mapId)
    {
        var mongoKeyId = new MongoId(keyId);

        if (!items.TryGetValue(mongoKeyId, out var item))
        {
            if (_config.Debug)
                logger.Debug($"KeysMapInfo4: Предмет не найден: {keyId}");
            return (0, 0);
        }

        if (_config.Debug)
            logger.Debug($"KeysMapInfo4: Проверяем предмет {keyId}: Parent={item.Parent}, Name={item.Name}");

        if (!IsKey(item))
            return (0, 0);

        var colorUpdates = ApplyMapKeyColor(item, keyId, mapId);
        var localeUpdates = UpdateKeyLocales(localeIds, keyId, mapName, mapId);
        return (localeUpdates, colorUpdates);
    }

    private int UpdateKeyLocales(IEnumerable<string> localeIds, string keyId, string mapName, string mapId)
    {
        var updated = 0;
        var nameKey = $"{keyId} Name";
        var shortNameKey = $"{keyId} ShortName";
        var mapNameKey = $"{mapId} Name";

        foreach (var localeId in localeIds)
        {
            var localeStrings = localeService.GetLocaleDb(localeId);
            if (localeStrings == null || localeStrings.Count == 0)
            {
                if (_config.Debug)
                    logger.Debug($"KeysMapInfo4: Локализация {localeId} недоступна для ключа {keyId}");
                continue;
            }

            var localizedMapName = localeStrings.TryGetValue(mapNameKey, out var mapLoc) ? mapLoc : mapName;

            if (localeStrings.TryGetValue(nameKey, out var originalName) && !string.IsNullOrEmpty(originalName))
            {
                localeStrings[nameKey] = FormatKeyName(originalName, localizedMapName);
                updated++;

                if (_config.Debug)
                    logger.Debug($"KeysMapInfo4: {localeId} Name {keyId}: \"{originalName}\" → \"{localeStrings[nameKey]}\"");
            }

            if (localeStrings.TryGetValue(shortNameKey, out var originalShortName) && !string.IsNullOrEmpty(originalShortName))
            {
                localeStrings[shortNameKey] = FormatKeyName(originalShortName, localizedMapName);
                updated++;

                if (_config.Debug)
                    logger.Debug($"KeysMapInfo4: {localeId} ShortName {keyId}: \"{originalShortName}\" → \"{localeStrings[shortNameKey]}\"");
            }

            if (_config.Debug &&
                !localeStrings.ContainsKey(nameKey) &&
                !localeStrings.ContainsKey(shortNameKey))
            {
                logger.Debug($"KeysMapInfo4: Нет локализации для ключа {keyId} на языке {localeId}");
            }
        }

        return updated;
    }

    private int ApplyMapKeyColor(TemplateItem item, string keyId, string mapId)
    {
        if (!_config.ColorCoding.Enabled)
            return 0;

        if (item.Properties == null)
        {
            if (_config.Debug)
                logger.Debug($"KeysMapInfo4: У ключа {keyId} нет Properties, цвет не применён");
            return 0;
        }

        var isMarkedKey = _keysDb.MarkedKeys.Contains(keyId);
        string? color;

        if (isMarkedKey && !_config.ColorCoding.ChangeMarkedKeysBackground)
        {
            color = "yellow";
        }
        else if (!_config.ColorCoding.BackgroundColors.TryGetValue(mapId, out color) || string.IsNullOrWhiteSpace(color))
        {
            if (_config.Debug)
                logger.Debug($"KeysMapInfo4: Цвет для карты {mapId} не задан в colorCoding.backgroundColors");
            return 0;
        }

        item.Properties.BackgroundColor = color;

        if (_config.Debug)
            logger.Debug($"KeysMapInfo4: Цвет {color} для ключа {keyId} (карта {mapId})");

        return 1;
    }

    private int ApplyJunkKeyColors(Dictionary<MongoId, TemplateItem> items)
    {
        if (!_config.ColorCoding.Enabled || _keysDb.JunkKeys.Count == 0)
            return 0;

        if (!_config.ColorCoding.BackgroundColors.TryGetValue("junkKeys", out var junkColor) ||
            string.IsNullOrWhiteSpace(junkColor))
        {
            junkColor = "black";
        }

        var colored = 0;
        foreach (var junkKeyId in _keysDb.JunkKeys)
        {
            if (!items.TryGetValue(new MongoId(junkKeyId), out var item) || item.Properties == null)
                continue;

            item.Properties.BackgroundColor = junkColor;
            colored++;

            if (_config.Debug)
                logger.Debug($"KeysMapInfo4: Цвет {junkColor} для мусорного ключа {junkKeyId}");
        }

        return colored;
    }

    private bool IsKey(TemplateItem item)
    {
        var parent = item.Parent.ToString();
        return parent == _config.KeyTypes.Mechanical ||
               parent == _config.KeyTypes.Keycard;
    }

    private string FormatKeyName(string originalName, string mapName)
    {
        return originalName + _config.Format.Replace("{mapName}", mapName);
    }
}
