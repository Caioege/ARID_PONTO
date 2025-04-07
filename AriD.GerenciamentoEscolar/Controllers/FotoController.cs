using AriD.GerenciamentoEscolar.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class FotoController : Controller
    {
        public FotoController() 
        {
            
        }

        [HttpGet]
        public IActionResult Aluno(int id)
        {
            try
            {
                var redeDeEnsinoId = this.HttpContext.DadosDaSessao().RedeDeEnsinoId;
                var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "redeDeEnsino", $"{redeDeEnsinoId}", $"{id}.png");

                if (!Path.Exists(caminhoArquivo))
                    return File("~/img/pessoas/sem-foto.png", "image/png");

                var imageFileStream = System.IO.File.OpenRead(caminhoArquivo);
                return File(imageFileStream, "image/png");
            }
            catch (Exception)
            {
                return File("~/img/pessoas/sem-foto.png", "image/png");
            }
        }

        [HttpPost]
        public ActionResult SalvarFotoServidor(int id, IFormFile file)
        {
            try
            {
                byte[] arquivo = null;
                if (file.Length > 0)
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        arquivo = ms.ToArray();
                    }

                if (!file.ContentType.Contains("image"))
                    throw new ApplicationException("O item selecionado não é uma imagem.");

                if (arquivo != null)
                {
                    var imagem = ImageToByteArray(ResizeImage(ByteArrayToImage(arquivo), new Size(140, 160)));

                    var redeDeEnsinoId = this.HttpContext.DadosDaSessao().RedeDeEnsinoId;

                    var pastaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "redeDeEnsino", $"{redeDeEnsinoId}");

                    if (!Path.Exists(pastaBase))
                        Directory.CreateDirectory(pastaBase);

                    var caminho = Path.Combine(pastaBase, $"{id}.png");

                    using (FileStream fs = new FileStream(caminho, FileMode.OpenOrCreate, FileAccess.Write))
                        fs.Write(imagem, 0, (int)imagem.Length);
                }

                return Json(new { sucesso = true, mensagem = "A imagem foi atualizada." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult RemoverFotoServidor(int id)
        {
            try
            {
                var redeDeEnsinoId = this.HttpContext.DadosDaSessao().RedeDeEnsinoId;
                var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "redeDeEnsino", $"{redeDeEnsinoId}", $"{id}.png");

                if (Path.Exists(caminhoArquivo))
                    System.IO.File.Delete(caminhoArquivo);

                return Json(new { sucesso = true, mensagem = "A imagem foi removida." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        private static byte[] ImageToByteArray(System.Drawing.Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private static System.Drawing.Image ResizeImage(System.Drawing.Image imgToResize, Size size) => (System.Drawing.Image)(new Bitmap(imgToResize, size));

        private System.Drawing.Image ByteArrayToImage(byte[] bytesArr)
        {
            using (MemoryStream memstr = new MemoryStream(bytesArr))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(memstr);
                return img;
            }
        }
    }
}
