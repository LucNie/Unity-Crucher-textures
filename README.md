# Texture Compressor

Le **Texture Compressor** est un outil Unity qui permet de compresser toutes les textures d'un projet en fonction de divers paramètres définis par l'utilisateur. Ce script est conçu pour être utilisé dans l'éditeur Unity et offre une interface graphique simple pour contrôler le processus de compression.

## Fonctionnalités

- Compression des textures avec la possibilité de choisir la qualité.
- Réglage de la vitesse de traitement des textures.
- Option pour définir une taille maximale pour les textures.
- Possibilité de réduire les textures à une basse résolution.

## Installation

1. Copiez le script `TextureCompressor.cs` dans le dossier `Assets` de votre projet Unity.
2. Ouvrez Unity et accédez à **Window > Texture compression** pour lancer l'outil.

## Utilisation

1. **Compression des textures :** 
   - Activez la compression des textures en cochant "Crunch compression".
   - Ajustez la qualité de compression à l'aide du curseur "Compression quality".

2. **Vitesse de traitement :** 
   - Définissez la vitesse de traitement des textures avec le curseur "Processing speed".

3. **Paramètres de taille et de résolution :**
   - Activez "Set max texture size" pour limiter la taille des textures.
   - Entrez la valeur de la taille maximale.
   - Cochez "Set texture to low resolution" pour réduire la résolution des textures.

4. **Démarrer le traitement :**
   - Cliquez sur le bouton "Start" pour commencer la compression des textures.
   - Cliquez sur "Cancel" pour annuler le traitement en cours.

5. **Suivi de la progression :** 
   - Une barre de progression indique le pourcentage de textures traitées.

## Notes

- Assurez-vous d'avoir sauvegardé votre projet avant d'exécuter le script, car les modifications appliquées aux textures peuvent être irréversibles.
- Ce script est principalement destiné à optimiser les textures pour une utilisation dans les jeux Unity.
