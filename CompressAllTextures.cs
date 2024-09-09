using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;

public class TextureCompressor : EditorWindow
{
    #region Variables

    // Variables pour contrôler la qualité de compression et la vitesse de traitement
    bool cruchedCompression = true;     // Activer la compression des textures
    int compressionQuality = 75;    // Qualité de compression par défaut à 75%, bon compromis
    int processingSpeed = 10;       // Vitesse de traitement des textures (nombre de textures traitées par frame)
    
    // Variables pour définir la taille maximale de la texture et résolution basse
    bool maxTextureSize = false;    // Indicateur pour savoir si la taille max de la texture doit être modifiée
    int maxTextureSizeValue = 1024; // Taille maximale de la texture si activée
    bool setTextureToLowRes = false; // Indicateur pour réduire la texture à une basse résolution

    // Variables pour gérer les routines de traitement et d'affichage de messages
    IEnumerator jobRoutine;         // Routine pour la compression des textures
    IEnumerator messageRoutine;     // Routine pour l'affichage des messages

    // Variables pour suivre la progression de la tâche
    float progressCount = 0f;       // Nombre d'éléments traités
    float totalCount = 1f;          // Nombre total d'éléments à traiter

    #endregion

    #region Properties

    // Retourne la progression normalisée (entre 0 et 1)
    float NormalizedProgress
    {
        get { return progressCount / totalCount; }
    }

    // Retourne la progression en pourcentage
    float Progress
    {
        get { return progressCount / totalCount * 100f; }
    }

    // Retourne la progression formatée en pourcentage avec deux décimales
    string FormattedProgress
    {
        get { return Progress.ToString("0.00") + "%"; }
    }

    #endregion

    #region Script Lifecycle

    // Crée une fenêtre de l'éditeur pour la compression des textures
    [MenuItem("Window/Texture compression")]
    static void Init()
    {
        var window = (TextureCompressor)EditorWindow.GetWindow(typeof(TextureCompressor));
        window.Show();
    }

    // Met à jour l'UI à chaque changement
    public void OnInspectorUpdate()
    {
        Repaint();
    }

    // Gestion de l'interface graphique de l'éditeur
    void OnGUI()
    {
        // Titre de la fenêtre
        EditorGUILayout.LabelField("Compress all textures", EditorStyles.boldLabel);

        // Slider pour ajuster la vitesse de traitement
        processingSpeed = EditorGUILayout.IntSlider("Processing speed:", processingSpeed, 10, 20);
        // Slider pour ajuster la qualité de la compression
        // toggle
        cruchedCompression = EditorGUILayout.Toggle("Crunch compression:", cruchedCompression);
        if (cruchedCompression)
        {
            compressionQuality = EditorGUILayout.IntSlider("Compression quality:", compressionQuality, 0, 100);
        }
        // Option pour définir la taille maximale de la texture
        maxTextureSize = EditorGUILayout.Toggle("Set max texture size:", maxTextureSize);
        if (maxTextureSize)
        {
            maxTextureSizeValue = EditorGUILayout.IntField("Max texture size value:", maxTextureSizeValue);
        }

        // Option pour réduire la résolution de la texture
        setTextureToLowRes = EditorGUILayout.Toggle("Set texture to low resolution:", setTextureToLowRes);

        // Bouton pour démarrer ou annuler le traitement
        string buttonLabel = jobRoutine != null ? "Cancel" : "Start";
        if (GUILayout.Button(buttonLabel))
        {
            if (jobRoutine != null)
            {
                // Annuler la routine en cours
                messageRoutine = DisplayMessage("Cancelled. " + FormattedProgress + " complete", 4f);
                jobRoutine = null;
            }
            else
            {
                // Démarrer la routine de compression des textures
                jobRoutine = CrunchTextures();
            }
        }

        // Barre de progression du traitement
        if (jobRoutine != null)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(FormattedProgress);

            var rect = EditorGUILayout.GetControlRect();
            rect.width = rect.width * NormalizedProgress;
            GUI.Box(rect, GUIContent.none);

            EditorGUILayout.EndHorizontal();
        }
        // Affiche un message si disponible
        else if (!string.IsNullOrEmpty(_message))
        {
            EditorGUILayout.HelpBox(_message, MessageType.None);
        }
    }

    // Inscription de la fonction de callback lors de l'activation du script
    void OnEnable()
    {
        EditorApplication.update += HandleCallbackFunction;
    }

    // Fonction appelée à chaque frame pour gérer les routines en arrière-plan
    void HandleCallbackFunction()
    {
        if (jobRoutine != null && !jobRoutine.MoveNext())
            jobRoutine = null;

        if (messageRoutine != null && !messageRoutine.MoveNext())
            messageRoutine = null;
    }

    // Désinscription de la fonction de callback lors de la désactivation du script
    void OnDisable()
    {
        EditorApplication.update -= HandleCallbackFunction;
    }

    #endregion

    #region Logic

    string _message = null; // Variable pour afficher un message dans l'interface

    // Coroutine pour afficher un message temporairement
    IEnumerator DisplayMessage(string message, float duration = 0f)
    {
        if (duration <= 0f || string.IsNullOrEmpty(message))
            goto Exit;

        _message = message;

        while (duration > 0)
        {
            duration -= 0.01667f; // Durée en secondes (60 fps)
            yield return null;
        }

    Exit:
        _message = string.Empty;
    }

    // Coroutine pour compresser les textures
    IEnumerator CrunchTextures()
    {
        // Réinitialiser les messages
        DisplayMessage(string.Empty);

        // Rechercher toutes les textures dans le projet
        var assets = AssetDatabase.FindAssets("t:texture", null)
            .Select(o => AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(o)) as TextureImporter);

        // Filtrer les textures qui nécessitent une compression
        var eligibleAssets = assets.Where(o => o != null)
            .Where(o => o.compressionQuality != compressionQuality || !o.crunchedCompression);

        // Définir le nombre total de textures à traiter
        totalCount = (float)eligibleAssets.Count();
        progressCount = 0f;

        // Paramètres de qualité et de limite de traitement par frame
        int quality = compressionQuality;
        int limiter = processingSpeed;
        

        // Boucle pour traiter chaque texture éligible
        foreach (var textureImporter in eligibleAssets)
        {
            progressCount += 1f;

            // Appliquer la compression
            if (cruchedCompression)
            {
                textureImporter.compressionQuality = quality;
                textureImporter.crunchedCompression = true;
            }
            if (setTextureToLowRes)
            {
                textureImporter.textureCompression = TextureImporterCompression.CompressedLQ;
            }
            if (maxTextureSize)
            {
                textureImporter.maxTextureSize = maxTextureSizeValue;
            }

            // Appliquer les modifications
            AssetDatabase.ImportAsset(textureImporter.assetPath);

            limiter -= 1;
            if (limiter <= 0)
            {
                // Attendre la frame suivante pour éviter le blocage
                yield return null;

                limiter = processingSpeed;
            }
        }

        // Afficher un message une fois la compression terminée
        messageRoutine = DisplayMessage("Compression complete", 6f);
        jobRoutine = null;
    }

    #endregion
}
