# Publish to GitHub — Keys Map Info 4

**Статус:** `ready`  
**GitHub:** Release + zip  
**Версия:** `1.3.0`  
**Deployment:** `(server_only)`

## 1. Подготовка (уже сделано этим скриптом)

Папка: `github-repos/KeysMapInfo4/`

## 2. Создать репозиторий и запушить

```powershell
cd github-repos/KeysMapInfo4
git init
git add .
git commit -m "Source backup Keys Map Info 4 v1.3.0"
git branch -M main
git remote add origin https://github.com/kabzon93region/KeysMapInfo4.git
git push -u origin main
```

Или автоматически:

```powershell
python CURSORAIMODING/tools/publish/publish_github_release.py KeysMapInfo4 --create-repo
```

## 3. GitHub Release

Прикрепить zip (только игровые файлы, без INSTALL.md):

`\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\KeysMapInfo4_(server_only)_v1.3.0_2026-06-29.zip`

```powershell
gh release create v1.3.0 "\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\KeysMapInfo4_(server_only)_v1.3.0_2026-06-29.zip" ^
  --title "Keys Map Info 4 v1.3.0" ^
  --notes-file CHANGELOG.md
```

## Описание репозитория (suggested)

SPT 4: подпись карты к ключам в локализации + цвет фона по карте (порт KeysMapInfo / ColorCodedKeys).

SPT 4.0 + Fika 2.3 headless stack. Deployment: `(server_only)`.
