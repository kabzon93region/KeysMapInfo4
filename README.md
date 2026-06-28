# Keys Map Info 4

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Release](https://img.shields.io/badge/release-v1.3.0-blue)](https://github.com/kabzon93region/KeysMapInfo4/releases/tag/v1.3.0)
[![Download zip](https://img.shields.io/badge/download-zip-brightgreen)](https://github.com/kabzon93region/KeysMapInfo4/releases/tag/v1.3.0)
[![EFT](https://img.shields.io/badge/EFT-16%2E9-orange)](https://www.escapefromtarkov.com/)
[![SPT](https://img.shields.io/badge/SPT-4.0.13-blue)](https://sp-tarkov.com/)
![Deployment](https://img.shields.io/badge/deployment-server_only-lightgrey)

Серверный мод SPT 4: к названиям ключей добавляется карта, фон ячейки в инвентаре окрашивается по локации. Порт `KeysMapInfo` (SPT 3) + цветовое кодирование из ColorCodedKeys.

| | |
|---|---|
| **Разработчик** | [kabzon93region](https://github.com/kabzon93region) |
| **Версия** | 1.3.0 |
| **GitHub** | [KeysMapInfo4](https://github.com/kabzon93region/KeysMapInfo4) |
| **Deployment** | `(server_only)` |
| **Тип** | server |

## Возможности

- Подпись карты к **Name** и **ShortName** ключа (пример: `Ключ от склада (Таможня)`).
- **Цвет фона** ключа в инвентаре / схроне по карте (`BackgroundColor` в шаблоне предмета).
- Только нужные **языки локализации** (`locales`, по умолчанию `ru`).
- Исключение **мусорных** и (опционально) **помеченных** ключей.
- База **~200 ключей** по 9 картам в `keys.json`.

## Установка

1. Скачайте zip из [Releases](https://github.com/kabzon93region/KeysMapInfo4/releases).
2. Распакуйте **в корень** папки с игрой (где `EscapeFromTarkov.exe` и `SPT/`).
3. Должно появиться: `SPT/user/mods/KeysMapInfo4/` (`KeysMapInfo4.dll`, `config.json`, `keys.json`).
4. Перезапустите **SPT server** и **клиент EFT**.

| Режим | Куда ставить |
|-------|--------------|
| Singleplayer SPT | SPT server на том же ПК |
| Fika coop | **SPT server** (один раз на сервер; клиентам мод не нужен) |

> Не ставьте одновременно старый `KeysMapInfo` (SPT 3 / TypeScript) и `KeysMapInfo4`.

## Конфигурация (`config.json`)

| Поле | Описание |
|------|----------|
| `enabled` | Включить мод |
| `format` | Шаблон подписи, `{mapName}` — название карты (по умолчанию ` ({mapName})`) |
| `locales` | Языки для правки названий, напр. `["ru"]` |
| `excludeMarkedKeys` | Не трогать помеченные ключи (жёлтые) |
| `excludeJunkKeys` | Не трогать мусорные ключи из `keys.json` |
| `debug` | Подробные логи сервера |
| `maps` | ID карты → имя для fallback локализации |
| `colorCoding.enabled` | Цвет фона ключей |
| `colorCoding.backgroundColors` | ID карты → цвет (`blue`, `green`, `tracerRed`, …) |
| `colorCoding.changeMarkedKeysBackground` | Менять цвет помеченных (иначе остаются жёлтыми) |

### Цвета по картам (по умолчанию)

| Карта | Цвет |
|-------|------|
| Factory | grey |
| Customs | blue |
| Woods | green |
| Shoreline | orange |
| Interchange | violet |
| Laboratory | tracerYellow |
| Reserve | red |
| Lighthouse | tracerRed |
| Streets | tracerGreen |
| Мусорные ключи | black (`junkKeys`) |

## Логи сервера

При успешном старте:

```
KeysMapInfo4: Конфигурация загружена: 9 карт
KeysMapInfo4: Обработка ключей для языков: ru
KeysMapInfo4: Обработано ключей: ..., окрашено ключей: ...
KeysMapInfo4: Мод загружен. ...
```

## Сборка (разработка)

Из корня `CURSORAIMODING/`:

```cmd
pack_keysmapinfo4.cmd
```

Или: `python tools/pack/pack_keysmapinfo4.py`  
Zip: `releases/KeysMapInfo4_(server_only)_vX.Y.Z_YYYY-MM-DD.zip`

## Публикация на GitHub

```cmd
scripts\github\publish-KeysMapInfo4.cmd
```

## Совместимость

- **EFT** 16.9, **SPT** 4.0.x
- Клиентский BepInEx **не требуется**
- Совместим с Fika (только на SPT server)

## Поддержать проект

Разовый донат: **[DonationAlerts → kabzon93region](https://www.donationalerts.com/r/kabzon93region)**
