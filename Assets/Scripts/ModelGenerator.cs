using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class ModelGenerator
{
    //Model Pieces (Design Type name -> List of parts as list of options)
    public static Dictionary<string, List<List<PixelMatrix>>> modelPieces = new Dictionary<string, List<List<PixelMatrix>>>();

    //Load Assets
    public static void LoadAssets()
    {
        //Model Folders
        string[] modelFolders = Directory.GetDirectories(Application.streamingAssetsPath + "/Models");

        //Replace \\ with /
        for (int i = 0; i < modelFolders.Length; i++)
        {
            modelFolders[i] = modelFolders[i].Replace("\\", "/");
        }

        //Each Model
        for (int i = 0; i < modelFolders.Length; i++)
        {
            //Folder Divided in /
            string[] partsFolder = modelFolders[i].Split('/');

            //Folder name
            string nameFolder = partsFolder[partsFolder.Length - 1];

            //Create Entry
            modelPieces[nameFolder] = new List<List<PixelMatrix>>();

            //All Files
            string[] files = Directory.GetFiles(modelFolders[i]);

            //Replace \\ with /
            for (int j = 0; j < files.Length; j++)
            {
                files[j] = files[j].Replace("\\", "/");
            }

            //Each File
            for (int j = 0; j < files.Length; j++)
            {
                //File Divided in /
                string[] partsFile = files[j].Split('/');

                //File name
                string nameFile = partsFile[partsFile.Length - 1];

                //Ignore .meta Files
                if (nameFile.Contains(".meta"))
                    continue;

                //Part
                int part = Int32.Parse(nameFile[0].ToString());

                //Check if entry exists
                if (modelPieces[nameFolder].Count - 1 < part)
                {
                    //Add list
                    modelPieces[nameFolder].Add(new List<PixelMatrix>());

                    //Add option
                    byte[] bytes = File.ReadAllBytes(files[j]);
                    Texture2D tex = new Texture2D(140, 66, TextureFormat.RGB24, false);
                    tex.filterMode = FilterMode.Point;
                    tex.LoadImage(bytes);
                    modelPieces[nameFolder][part].Add(new PixelMatrix(tex));
                }
                else
                {
                    //Add option
                    byte[] bytes = File.ReadAllBytes(files[j]);
                    Texture2D tex = new Texture2D(140, 66, TextureFormat.RGB24, false);
                    tex.filterMode = FilterMode.Point;
                    tex.LoadImage(bytes);
                    modelPieces[nameFolder][part].Add(new PixelMatrix(tex));
                }
            }
        }
    }

    //Check can Generate
    public static bool CanGenerate(string type)
    {
        return modelPieces.ContainsKey(type);
    }

    //Generate Model
    public static Texture2D GenerateModel(string type)
    {
        //Get candidates for type
        List<List<PixelMatrix>> piecesCandidates = modelPieces[type];

        //Random Combinations
        List<PixelMatrix> selected = new List<PixelMatrix>();
        for (int i = 0; i < piecesCandidates.Count; i++)
        {
            selected.Add(piecesCandidates[i][UnityEngine.Random.Range(0, piecesCandidates[i].Count)]);
        }

        //Graphics Combine
        PixelMatrix combination = DrawingUtils.MultiCombine(selected.ToArray());

        //Center Horizontally
        combination = DrawingUtils.CenterHorizontal(combination);

        //Create Texture2D
        Texture2D result = new Texture2D(combination.width, combination.height);
        result.filterMode = FilterMode.Point;
        result.SetPixels(combination.pixels);
        result.Apply();

        return result;
    }

}