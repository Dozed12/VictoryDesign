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
    public static Dictionary<string, List<List<Texture2D>>> modelPieces = new Dictionary<string, List<List<Texture2D>>>();

    //Load Assets
    public static void LoadAssets()
    {
        //Model Folders
        string[] modelFolders = Directory.GetDirectories(Application.streamingAssetsPath  + "/Models");

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
            modelPieces[nameFolder] = new List<List<Texture2D>>();

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

                //If file more than 6 chars probably its bad file (like .meta)
                if(nameFile.Count() > 6)
                    continue;

                //Part
                int part = Int32.Parse(nameFile[0].ToString());

                //Option
                int option = 'a' - nameFile[1];

                //Check if entry exists
                if(modelPieces[nameFolder].Count - 1 < part)
                {
                    //Add list
                    modelPieces[nameFolder].Add(new List<Texture2D>());

                    //Add option
                    byte[] bytes = File.ReadAllBytes(files[j]);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.filterMode = FilterMode.Point;
                    tex.LoadImage(bytes);
                    modelPieces[nameFolder][part].Add(tex);
                }
                else
                {
                    //Add option
                    byte[] bytes = File.ReadAllBytes(files[j]);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.filterMode = FilterMode.Point;
                    tex.LoadImage(bytes);
                    modelPieces[nameFolder][part].Add(tex);
                }
            }
        }
    }
}