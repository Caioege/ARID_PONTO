using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DocxExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string docxPath = @"c:\src\ARID_PONTO\DOC - PADRE BERNADO - Atualizado.docx";
                string outputPath = @"c:\src\ARID_PONTO\docx_content.txt";

                using (ZipArchive archive = ZipFile.OpenRead(docxPath))
                {
                    ZipArchiveEntry entry = archive.GetEntry("word/document.xml");
                    if (entry == null)
                    {
                        Console.WriteLine("Could not find word/document.xml");
                        return;
                    }

                    using (Stream stream = entry.Open())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string xml = reader.ReadToEnd();
                        // Simple regex to extract text from XML tags
                        string text = Regex.Replace(xml, "<[^>]+>", " ");
                        // Clean up whitespace
                        text = Regex.Replace(text, @"\s+", " ");
                        File.WriteAllText(outputPath, text);
                        Console.WriteLine("Successfully extracted text to " + outputPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
