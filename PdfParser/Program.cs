using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

var pdfPath = @"c:\src\ARID_PONTO\DOC - PADRE BERNADO - Com Anotações (2).pdf";
var outputPath = @"c:\src\ARID_PONTO\PdfParser\pdf_content.txt";

var sb = new StringBuilder();
using var reader = new PdfReader(pdfPath);
using var pdfDoc = new PdfDocument(reader);

for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
{
    var page = pdfDoc.GetPage(i);
    var strategy = new SimpleTextExtractionStrategy();
    var text = PdfTextExtractor.GetTextFromPage(page, strategy);
    sb.AppendLine($"=== PÁGINA {i} ===");
    sb.AppendLine(text);
    sb.AppendLine();
}

File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
Console.WriteLine($"Extraídas {pdfDoc.GetNumberOfPages()} páginas para {outputPath}");
