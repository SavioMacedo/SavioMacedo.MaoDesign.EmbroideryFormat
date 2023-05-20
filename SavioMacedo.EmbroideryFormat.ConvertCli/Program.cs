// See https://aka.ms/new-console-template for more information
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pes;

Console.WriteLine("Caminho: ");
var path = Console.ReadLine();
var embroidery = File.ReadAllBytes(path);
PesFile pesFile = PesFile.Read(embroidery, false, false, 2.0f);
byte[] pec = PecFile.Write(pesFile);
path = path.Replace(".pes", ".pec");
File.WriteAllBytes(path, pec);
