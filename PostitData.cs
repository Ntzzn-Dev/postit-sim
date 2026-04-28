using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class PostitData
{
    public string Titulo { get; set; }
    public string Icone { get; set; }
    public double PosicaoY { get; set; }
    public int LadoLR { get; set; } // 0 off 1 left 2 right
    public int Cor { get; set; }

    public PostitData(){

    }

    public PostitData(String title, String ic, double py, int lado, int cor){
        this.Titulo = title;
        this.Icone = ic;
        this.PosicaoY = py;
        this.LadoLR = lado;
        this.Cor = cor;
    }

    public static List<PostitData> banco = new List<PostitData>();

    public static void SaveJSON() {
        string caminho = "postits.json";

        string json = JsonSerializer.Serialize(banco, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(caminho, json);
    }

    public static void ReadJSON()
    {
        if (File.Exists("postits.json"))
        {
            string json = File.ReadAllText("postits.json");

            banco = JsonSerializer.Deserialize<List<PostitData>>(json)
                    ?? new List<PostitData>();
        }
        else
        {
            banco = new List<PostitData>();
        }
    }
}