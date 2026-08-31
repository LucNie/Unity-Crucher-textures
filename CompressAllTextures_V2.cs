using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TextureCompressor : EditorWindow
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    private const int MIN_PROCESSING_SPEED = 1;
    private const int MAX_PROCESSING_SPEED = 50;

    private const int MIN_CRUNCH_QUALITY = 0;
    private const int MAX_CRUNCH_QUALITY = 100;

    private const int DEFAULT_CRUNCH_QUALITY = 75;
    private const int DEFAULT_MAX_TEXTURE_SIZE = 1024;
    private const int DEFAULT_PROCESSING_SPEED = 10;


    // ============================================================
    // ENUMS
    // ============================================================

    /// <summary>
    /// Niveau de compression utilisé par Unity.
    /// </summary>
    private enum CompressionLevel
    {
        LowQuality,
        NormalQuality,
        HighQuality
    }


    // ============================================================
    // SETTINGS
    // ============================================================

    [Header("Compression")]

    // Niveau de compression de la texture
    private CompressionLevel compressionLevel =
        CompressionLevel.NormalQuality;

    // Utiliser Crunch Compression
    private bool useCrunchCompression = true;

    // Qualité Crunch
    private int crunchCompressionQuality =
        DEFAULT_CRUNCH_QUALITY;


    [Header("Processing")]

    // Nombre de textures traitées par frame
    private int processingSpeed =
        DEFAULT_PROCESSING_SPEED;


    [Header("Texture Size")]

    // Modifier la taille maximale des textures
    private bool setMaxTextureSize = false;

    // Taille maximale
    private int maxTextureSize =
        DEFAULT_MAX_TEXTURE_SIZE;


    // ============================================================
    // RUNTIME VARIABLES
    // ============================================================

    // Coroutine principale
    private IEnumerator compressionJob;

    // Coroutine pour les messages
    private IEnumerator messageJob;

    // Progression
    private int processedTextures = 0;
    private int totalTextures = 0;

    // Message affiché dans la fenêtre
    private string statusMessage = string.Empty;


    // ============================================================
    // PROPERTIES
    // ============================================================

    /// <summary>
    /// Progression entre 0 et 1.
    /// </summary>
    private float NormalizedProgress
    {
        get
        {
            if (totalTextures <= 0)
                return 1f;

            return (float)processedTextures / totalTextures;
        }
    }


    /// <summary>
    /// Progression en pourcentage.
    /// </summary>
    private float ProgressPercentage
    {
        get
        {
            return NormalizedProgress * 100f;
        }
    }


    /// <summary>
    /// Progression formatée.
    /// </summary>
    private string FormattedProgress
    {
        get
        {
            return $"{ProgressPercentage:0.00}%";
        }
    }


    /// <summary>
    /// Vérifie si une compression est actuellement en cours.
    /// </summary>
    private bool IsProcessing
    {
        get
        {
            return compressionJob != null;
        }
    }


    // ============================================================
    // WINDOW
    // ============================================================

    [MenuItem("Window/Texture Compression")]
    private static void OpenWindow()
    {
        TextureCompressor window =
            GetWindow<TextureCompressor>();

        window.titleContent =
            new GUIContent("Texture Compressor");

        window.minSize = new Vector2(350f, 300f);

        window.Show();
    }


    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    private void OnEnable()
    {
        EditorApplication.update += UpdateCoroutines;
    }


    private void OnDisable()
    {
        EditorApplication.update -= UpdateCoroutines;
    }


    /// <summary>
    /// Permet de mettre à jour l'interface pendant le traitement.
    /// </summary>
    private void OnInspectorUpdate()
    {
        Repaint();
    }


    /// <summary>
    /// Met à jour les coroutines à chaque frame Unity.
    /// </summary>
    private void UpdateCoroutines()
    {
        if (compressionJob != null)
        {
            if (!compressionJob.MoveNext())
            {
                compressionJob = null;
            }
        }

        if (messageJob != null)
        {
            if (!messageJob.MoveNext())
            {
                messageJob = null;
            }
        }
    }


    // ============================================================
    // GUI
    // ============================================================

    private void OnGUI()
    {
        DrawHeader();

        EditorGUILayout.Space(10);

        DrawCompressionSettings();

        EditorGUILayout.Space(10);

        DrawProcessingSettings();

        EditorGUILayout.Space(10);

        DrawTextureSettings();

        EditorGUILayout.Space(15);

        DrawStartButton();

        EditorGUILayout.Space(10);

        DrawProgress();

        DrawStatusMessage();
    }


    // ============================================================
    // GUI - HEADER
    // ============================================================

    private void DrawHeader()
    {
        EditorGUILayout.LabelField(
            "Texture Compressor",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            "Compress and optimize all textures in the project.",
            EditorStyles.wordWrappedMiniLabel
        );
    }


    // ============================================================
    // GUI - COMPRESSION
    // ============================================================

    private void DrawCompressionSettings()
    {
        EditorGUILayout.LabelField(
            "Compression",
            EditorStyles.boldLabel
        );


        // --------------------------------------------------------
        // Compression Quality
        // --------------------------------------------------------

        string[] compressionOptions =
        {
            "Low Quality",
            "Normal Quality",
            "High Quality"
        };

        compressionLevel =
            (CompressionLevel)EditorGUILayout.Popup(
                "Compression",
                (int)compressionLevel,
                compressionOptions
            );


        // --------------------------------------------------------
        // Crunch
        // --------------------------------------------------------

        useCrunchCompression =
            EditorGUILayout.Toggle(
                "Use Crunch Compression",
                useCrunchCompression
            );


        // --------------------------------------------------------
        // Crunch Quality
        // --------------------------------------------------------

        if (useCrunchCompression)
        {
            crunchCompressionQuality =
                EditorGUILayout.IntSlider(
                    "Crunch Compression Quality",
                    crunchCompressionQuality,
                    MIN_CRUNCH_QUALITY,
                    MAX_CRUNCH_QUALITY
                );
        }
    }


    // ============================================================
    // GUI - PROCESSING
    // ============================================================

    private void DrawProcessingSettings()
    {
        EditorGUILayout.LabelField(
            "Processing",
            EditorStyles.boldLabel
        );


        processingSpeed =
            EditorGUILayout.IntSlider(
                "Processing Speed",
                processingSpeed,
                MIN_PROCESSING_SPEED,
                MAX_PROCESSING_SPEED
            );


        EditorGUILayout.HelpBox(
            "Higher values process more textures per frame, " +
            "but may temporarily freeze the Unity Editor.",
            MessageType.None
        );
    }


    // ============================================================
    // GUI - TEXTURE SETTINGS
    // ============================================================

    private void DrawTextureSettings()
    {
        EditorGUILayout.LabelField(
            "Texture Settings",
            EditorStyles.boldLabel
        );


        setMaxTextureSize =
            EditorGUILayout.Toggle(
                "Set Max Texture Size",
                setMaxTextureSize
            );


        if (setMaxTextureSize)
        {
            maxTextureSize =
                EditorGUILayout.IntField(
                    "Max Texture Size",
                    maxTextureSize
                );


            // Évite une valeur invalide
            maxTextureSize =
                Mathf.Max(32, maxTextureSize);
        }
    }


    // ============================================================
    // GUI - START / CANCEL
    // ============================================================

    private void DrawStartButton()
    {
        string buttonText =
            IsProcessing ? "Cancel" : "Start Compression";


        if (GUILayout.Button(
            buttonText,
            GUILayout.Height(30f)))
        {
            if (IsProcessing)
            {
                CancelCompression();
            }
            else
            {
                StartCompression();
            }
        }
    }


    // ============================================================
    // GUI - PROGRESS
    // ============================================================

    private void DrawProgress()
    {
        if (!IsProcessing)
            return;


        EditorGUILayout.LabelField(
            $"Progress: {FormattedProgress}"
        );


        Rect progressRect =
            EditorGUILayout.GetControlRect(
                GUILayout.Height(18f)
            );


        // Background
        GUI.Box(
            progressRect,
            GUIContent.none
        );


        // Progression
        Rect filledRect = progressRect;

        filledRect.width *=
            NormalizedProgress;


        GUI.Box(
            filledRect,
            GUIContent.none
        );


        EditorGUILayout.LabelField(
            $"{processedTextures} / {totalTextures} textures"
        );
    }


    // ============================================================
    // GUI - STATUS MESSAGE
    // ============================================================

    private void DrawStatusMessage()
    {
        if (string.IsNullOrEmpty(statusMessage))
            return;


        EditorGUILayout.HelpBox(
            statusMessage,
            MessageType.Info
        );
    }


    // ============================================================
    // COMPRESSION CONTROL
    // ============================================================

    /// <summary>
    /// Lance la compression.
    /// </summary>
    private void StartCompression()
    {
        statusMessage = string.Empty;

        processedTextures = 0;
        totalTextures = 0;

        compressionJob =
            CompressTextures();
    }


    /// <summary>
    /// Annule la compression actuelle.
    /// </summary>
    private void CancelCompression()
    {
        compressionJob = null;

        ShowMessage(
            $"Compression cancelled. {FormattedProgress} complete.",
            4f
        );
    }


    // ============================================================
    // TEXTURE COMPRESSION
    // ============================================================

    private IEnumerator CompressTextures()
    {
        // --------------------------------------------------------
        // Récupération des textures
        // --------------------------------------------------------

        List<TextureImporter> textures =
            FindTextureImporters();


        totalTextures = textures.Count;


        // Rien à faire
        if (totalTextures == 0)
        {
            ShowMessage(
                "All textures are already correctly configured.",
                5f
            );

            yield break;
        }


        // --------------------------------------------------------
        // Paramètres sélectionnés
        // --------------------------------------------------------

        TextureImporterCompression selectedCompression =
            GetSelectedCompression();


        int limiter = processingSpeed;


        // --------------------------------------------------------
        // Traitement
        // --------------------------------------------------------

        foreach (TextureImporter texture in textures)
        {
            // ----------------------------------------------------
            // Compression Low / Normal / High
            // ----------------------------------------------------

            texture.textureCompression =
                selectedCompression;


            // ----------------------------------------------------
            // Crunch Compression
            // ----------------------------------------------------

            texture.crunchedCompression =
                useCrunchCompression;


            if (useCrunchCompression)
            {
                texture.compressionQuality =
                    crunchCompressionQuality;
            }


            // ----------------------------------------------------
            // Taille maximale
            // ----------------------------------------------------

            if (setMaxTextureSize)
            {
                texture.maxTextureSize =
                    maxTextureSize;
            }


            // ----------------------------------------------------
            // Appliquer les changements
            // ----------------------------------------------------

            AssetDatabase.ImportAsset(
                texture.assetPath,
                ImportAssetOptions.ForceUpdate
            );


            processedTextures++;

            limiter--;


            // ----------------------------------------------------
            // Attendre la frame suivante
            // ----------------------------------------------------

            if (limiter <= 0)
            {
                yield return null;

                limiter = processingSpeed;
            }
        }


        // --------------------------------------------------------
        // Terminé
        // --------------------------------------------------------

        compressionJob = null;


        ShowMessage(
            $"Compression complete! {processedTextures} textures processed.",
            6f
        );
    }


    // ============================================================
    // FIND TEXTURES
    // ============================================================

    /// <summary>
    /// Recherche toutes les textures du projet
    /// qui nécessitent une modification.
    /// </summary>
    private List<TextureImporter> FindTextureImporters()
    {
        TextureImporterCompression selectedCompression =
            GetSelectedCompression();


        string[] textureGUIDs =
            AssetDatabase.FindAssets(
                "t:Texture"
            );


        List<TextureImporter> textures =
            new List<TextureImporter>();


        foreach (string guid in textureGUIDs)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);


            TextureImporter importer =
                AssetImporter.GetAtPath(path)
                as TextureImporter;


            if (importer == null)
                continue;


            // Vérifie si la texture nécessite une modification
            bool needsCompressionChange =
                importer.textureCompression !=
                selectedCompression;


            bool needsCrunchChange =
                importer.crunchedCompression !=
                useCrunchCompression;


            bool needsCrunchQualityChange =
                useCrunchCompression &&
                importer.compressionQuality !=
                crunchCompressionQuality;


            bool needsMaxSizeChange =
                setMaxTextureSize &&
                importer.maxTextureSize !=
                maxTextureSize;


            bool needsUpdate =
                needsCompressionChange ||
                needsCrunchChange ||
                needsCrunchQualityChange ||
                needsMaxSizeChange;


            if (needsUpdate)
            {
                textures.Add(importer);
            }
        }


        return textures;
    }


    // ============================================================
    // COMPRESSION TYPE
    // ============================================================

    /// <summary>
    /// Convertit notre enum en valeur Unity.
    /// </summary>
    private TextureImporterCompression GetSelectedCompression()
    {
        switch (compressionLevel)
        {
            case CompressionLevel.LowQuality:
                return TextureImporterCompression.CompressedLQ;


            case CompressionLevel.NormalQuality:
                return TextureImporterCompression.Compressed;


            case CompressionLevel.HighQuality:
                return TextureImporterCompression.CompressedHQ;


            default:
                return TextureImporterCompression.Compressed;
        }
    }


    // ============================================================
    // MESSAGE
    // ============================================================

    /// <summary>
    /// Affiche un message temporairement.
    /// </summary>
    private void ShowMessage(
        string message,
        float duration)
    {
        statusMessage = message;

        messageJob =
            ClearMessageAfterDelay(duration);
    }


    /// <summary>
    /// Efface le message après un certain temps.
    /// </summary>
    private IEnumerator ClearMessageAfterDelay(
        float duration)
    {
        float timer = 0f;


        while (timer < duration)
        {
            timer += 0.01667f;

            yield return null;
        }


        statusMessage = string.Empty;
        messageJob = null;
    }
}
