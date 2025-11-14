using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Script de Editor para crear una estructura de carpetas de proyecto Unity optimizada para game dev.
// Colocar este archivo en Assets/Editor/ para que aparezca en el menú Tools -> Project Template
public static class ProjectTemplateCreator
{
    // Lista de carpetas a crear (rutas relativas al proyecto)
    static readonly string[] folders = new[]
    {
        "Assets/Animations",
        "Assets/Audio/Music",
        "Assets/Audio/SoundEffects",
        "Assets/Materials",
        "Assets/Models",
        "Assets/Prefabs",
        "Assets/Scenes",
        "Assets/Shaders",
        "Assets/Scripts/Editor",
        "Assets/Scripts/Resources",
        "Assets/Sprites",
        "Assets/Textures",
        "Assets/UI",
        "Docs",
        "Builds",
        "Tools"
    };

    [MenuItem("Tools/Project Template/Create Folder Structure")]
    public static void CreateFolderStructure()
    {
        int created = 0;
        foreach (var f in folders)
        {
            if (EnsureFolder(f))
            {
                CreateHelpers(f);
                created++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Project Template: carpetas creadas o verificadas: {created}");
    }

    // Crea la carpeta y todas las carpetas padre necesarias usando AssetDatabase
    static bool EnsureFolder(string folderPath)
    {
        folderPath = folderPath.Replace("\\", "/");
        if (AssetDatabase.IsValidFolder(folderPath)) return false;

        // Crear recursivamente
        var parts = folderPath.Split('/');
        var path = parts[0]; // normalmente "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            var next = path + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(path, parts[i]);
            }
            path = next;
        }
        return true;
    }

    // -----------------------
    // Cleanup: eliminar carpetas vacías
    // -----------------------
    [MenuItem("Tools/Project Template/Cleanup/Remove Empty Folders")]
    public static void RemoveEmptyFolders()
    {
        // Cargar configuracion (exclusiones)
        var cfg = LoadConfig();

        // Preguntar modo: Dry run / Ejecutar / Cancelar
        int choice = EditorUtility.DisplayDialogComplex("Project Template - Remove Empty Folders",
            "Elegir modo:\n\nDry run = mostrar carpetas vacías que se eliminarían.\nRun = eliminar realmente.",
            "Dry run", "Run", "Cancelar");

        if (choice == 2)
        {
            Debug.Log("Project Template: operación cancelada.");
            return;
        }
        bool dryRun = (choice == 0);
        var projectRoot = Directory.GetCurrentDirectory();
        var assetsRoot = Path.Combine(projectRoot, "Assets").Replace("\\", "/");
        var allDirs = Directory.GetDirectories(assetsRoot, "*", SearchOption.AllDirectories)
            .Select(d => d.Replace("\\", "/"))
            .ToList();

        var empty = new List<string>();
        foreach (var dir in allDirs)
        {
            // Obtener archivos no meta
            var files = Directory.GetFiles(dir)
                .Where(f => !f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .Select(f => Path.GetFileName(f))
                .Where(n => n != ".gitkeep" && n != "README.md")
                .ToArray();

            if (files.Length == 0)
            {
                // Convertir a ruta relativa de Unity (Assets/...)
                var rel = dir.Replace(projectRoot.Replace("\\", "/"), "").TrimStart('/');
                if (!string.IsNullOrEmpty(rel)) empty.Add(rel);
            }
        }

        // Aplicar exclusiones (no listar carpetas dentro de exclusions)
        if (cfg.exclusions != null && cfg.exclusions.Count > 0)
        {
            empty = empty.Where(e => !cfg.exclusions.Any(ex => e.StartsWith(ex + "/", StringComparison.OrdinalIgnoreCase) || e.Equals(ex, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        if (empty.Count == 0)
        {
            EditorUtility.DisplayDialog("Project Template", "No se encontraron carpetas vacías.", "OK");
            return;
        }

        var list = string.Join("\n", empty);
        if (dryRun)
        {
            EditorUtility.DisplayDialog("Project Template - Dry run",
                $"Se han encontrado {empty.Count} carpetas vacías que serían eliminadas:\n\n{list}", "OK");
            Debug.Log($"Project Template (dry run): {empty.Count} carpetas vacías encontradas.\n{list}");
            return;
        }

        if (EditorUtility.DisplayDialog("Project Template - Eliminar carpetas vacías",
            $"Se eliminarán las siguientes carpetas:\n\n{list}\n\n¿Continuar?", "Eliminar", "Cancelar"))
        {
            int removed = 0;
            foreach (var p in empty)
            {
                try
                {
                    if (AssetDatabase.DeleteAsset(p)) removed++;
                    else Debug.LogWarning($"No se pudo eliminar {p}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"No se pudo eliminar {p}: {ex.Message}");
                }
            }
            AssetDatabase.Refresh();
            Debug.Log($"Project Template: eliminadas {removed} carpetas vacías.");
        }
        else
        {
            Debug.Log("Project Template: operación de eliminación cancelada.");
        }
    }

    // -----------------------
    // Organizador básico de assets por extensión
    // -----------------------
    [MenuItem("Tools/Project Template/Organize/Organize Assets (basic)")]
    public static void OrganizeAssets()
    {
        // Cargar configuración (mapas y exclusiones)
        var cfg = LoadConfig();

        var map = cfg.rules ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {".mat", "Assets/Materials"},
            {".png", "Assets/Textures"},
            {".jpg", "Assets/Textures"},
            {".jpeg", "Assets/Textures"},
            {".tga", "Assets/Textures"},
            {".psd", "Assets/Textures"},
            {".fbx", "Assets/Models"},
            {".obj", "Assets/Models"},
            {".prefab", "Assets/Prefabs"},
            {".shader", "Assets/Shaders"},
            {".shadergraph", "Assets/Shaders"},
            {".hlsl", "Assets/Shaders"},
            {".compute", "Assets/Shaders"},
            {".wav", "Assets/Audio/SoundEffects"},
            {".mp3", "Assets/Audio/SoundEffects"},
            {".ogg", "Assets/Audio/SoundEffects"},
            {".aiff", "Assets/Audio/SoundEffects"}
        };

        int choice = EditorUtility.DisplayDialogComplex("Project Template - Organize",
            "Elegir modo:\n\nDry run = mostrar movimientos propuestos.\nRun = ejecutar movimientos.",
            "Dry run", "Run", "Cancelar");
        if (choice == 2)
        {
            Debug.Log("Project Template: organización cancelada.");
            return;
        }
        bool dryRun = (choice == 0);

        var guids = AssetDatabase.FindAssets("", new[] { "Assets" });
        int moved = 0;
        var conflicts = new List<string>();
        var planned = new List<string>();

        var projectRoot = Directory.GetCurrentDirectory();

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (string.IsNullOrEmpty(path)) continue;
            if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) continue;

            // Saltar exclusiones de carpetas
            if (cfg.exclusions != null && cfg.exclusions.Any(ex => path.StartsWith(ex + "/", StringComparison.OrdinalIgnoreCase) || path.Equals(ex, StringComparison.OrdinalIgnoreCase)))
                continue;

            var ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext)) continue;

            if (map.TryGetValue(ext, out var target))
            {
                if (path.StartsWith(target + "/", StringComparison.OrdinalIgnoreCase)) continue; // ya en destino

                EnsureFolder(target);
                var fileName = Path.GetFileName(path);
                var dest = target + "/" + fileName;

                // Si existe, buscar un nombre único
                var fullDestFs = Path.Combine(projectRoot, dest).Replace("\\", "/");
                if (File.Exists(fullDestFs))
                {
                    var baseName = Path.GetFileNameWithoutExtension(fileName);
                    var i = 1;
                    string newDest;
                    do
                    {
                        newDest = $"{target}/{baseName}_{i}{ext}";
                        i++;
                        fullDestFs = Path.Combine(projectRoot, newDest).Replace("\\", "/");
                    } while (File.Exists(fullDestFs));
                    dest = newDest;
                }

                planned.Add($"{path} -> {dest}");

                if (!dryRun)
                {
                    var err = AssetDatabase.MoveAsset(path, dest);
                    if (string.IsNullOrEmpty(err)) moved++;
                    else conflicts.Add($"{path} -> {dest} : {err}");
                }
            }
        }

        AssetDatabase.Refresh();

        // Crear informe en la raíz para diagnóstico
        try
        {
            var root = Directory.GetCurrentDirectory();
            var reportPath = Path.Combine(root, "ProjectTemplate_organize_report.txt");
            using (var w = new StreamWriter(reportPath, false))
            {
                w.WriteLine($"Project Template Organize Report - {DateTime.Now}");
                w.WriteLine($"Mode: {(dryRun ? "dry-run" : "run")}");
                w.WriteLine($"Planned moves: {planned.Count}");
                w.WriteLine("--- Planned ---");
                foreach (var p in planned) w.WriteLine(p);
                w.WriteLine("--- Results ---");
                w.WriteLine($"Moved: {moved}");
                w.WriteLine($"Conflicts: {conflicts.Count}");
                foreach (var c in conflicts) w.WriteLine(c);
            }
            Debug.Log($"Project Template: informe escrito en {reportPath}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"No se pudo escribir informe: {ex.Message}");
        }

        if (dryRun)
        {
            var msg = planned.Count == 0 ? "No hay movimientos propuestos." : string.Join("\n", planned.Take(200));
            EditorUtility.DisplayDialog("Project Template - Organize (dry run)", $"Movimientos propuestos: {planned.Count}\n\n{msg}\n\nSe ha creado ProjectTemplate_organize_report.txt en la raíz.", "OK");
            Debug.Log($"Project Template (dry run): {planned.Count} movimientos propuestos.\n{msg}");
            return;
        }

        Debug.Log($"Project Template: organizados {moved} assets. Conflictos: {conflicts.Count}");
        foreach (var c in conflicts) Debug.LogWarning(c);
        EditorUtility.DisplayDialog("Project Template - Organize", $"Organizados {moved} assets. Conflictos: {conflicts.Count} (ver consola). Se ha creado ProjectTemplate_organize_report.txt en la raíz.", "OK");
    }

    // Añade archivos auxiliares (.gitkeep y README.md) para carpetas que suelen estar vacías
    static void CreateHelpers(string folderPath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), folderPath);
            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

            var gitkeepPath = Path.Combine(fullPath, ".gitkeep");
            if (!File.Exists(gitkeepPath)) File.WriteAllText(gitkeepPath, "");

            // Antes se creaba un README.md en cada carpeta. Se decide no crear README automáticamente
            // para evitar spam de archivos. Si se necesita, usar CreateSampleConfig o crearlos manualmente.
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"No se pudo crear helpers en {folderPath}: {ex.Message}");
        }
    }

    // -----------------------
    // Configuración (JSON)
    // -----------------------
    class ProjectTemplateConfig
    {
        // Reglas: extensión -> carpeta destino
        public Dictionary<string, string> rules;
        // Exclusiones: rutas (por ejemplo "Assets/ThirdParty")
        public List<string> exclusions;
    }

    static ProjectTemplateConfig LoadConfig()
    {
        try
        {
            var root = Directory.GetCurrentDirectory();
            var cfgPath = Path.Combine(root, "ProjectTemplate.config.json");
            if (!File.Exists(cfgPath)) return new ProjectTemplateConfig { rules = null, exclusions = new List<string> { "Assets/ThirdParty", "Assets/TutorialInfo" } };

            var json = File.ReadAllText(cfgPath);
            var cfg = JsonUtility.FromJson<ProjectTemplateConfig>(json);
            if (cfg.exclusions == null) cfg.exclusions = new List<string> { "Assets/ThirdParty", "Assets/TutorialInfo" };
            return cfg;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"No se pudo cargar ProjectTemplate.config.json: {ex.Message}");
            return new ProjectTemplateConfig { rules = null, exclusions = new List<string> { "Assets/ThirdParty", "Assets/TutorialInfo" } };
        }
    }

    [MenuItem("Tools/Project Template/Create sample config")] 
    static void CreateSampleConfig()
    {
        var root = Directory.GetCurrentDirectory();
        var cfgPath = Path.Combine(root, "ProjectTemplate.config.json");
        if (File.Exists(cfgPath))
        {
            EditorUtility.DisplayDialog("Project Template", "Ya existe ProjectTemplate.config.json en la raíz.", "OK");
            return;
        }

        var sample = new ProjectTemplateConfig
        {
            rules = new Dictionary<string, string>
            {
                {".mat", "Assets/Materials"},
                {".png", "Assets/Textures"},
                {".fbx", "Assets/Models"}
            },
            exclusions = new List<string> { "Assets/ThirdParty", "Assets/TutorialInfo" }
        };

        // JsonUtility no serializa diccionarios, así que serializamos manualmente usando simple formatting
        var entries = string.Join(",\n", sample.rules.Select(kv => $"  \"{kv.Key}\": \"{kv.Value}\""));
        var excl = string.Join(",\n", sample.exclusions.Select(e => $"  \"{e}\""));
        var json = "{\n\"rules\": {\n" + entries + "\n},\n\"exclusions\": [\n" + excl + "\n]\n}\n";

        try
        {
            File.WriteAllText(cfgPath, json);
            EditorUtility.DisplayDialog("Project Template", "Archivo ProjectTemplate.config.json creado en la raíz.", "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Project Template", $"No se pudo crear el archivo: {ex.Message}", "OK");
        }
    }
}
