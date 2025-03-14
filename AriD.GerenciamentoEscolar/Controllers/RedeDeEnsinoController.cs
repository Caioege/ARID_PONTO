using AriD.BibliotecaDeClasses.Entidades;
using AriD.GerenciamentoEscolar.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using System.Drawing;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class RedeDeEnsinoController : Controller
    {
        private readonly IServico<RedeDeEnsino> _servico;

        public RedeDeEnsinoController(IServico<RedeDeEnsino> servico)
        {
            _servico = servico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<RedeDeEnsino> listaPaginada)
        {
            try
            {
                AjusteContextoDePaginacao(listaPaginada);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<RedeDeEnsino> listaPaginada)
        {
            try
            {
                AjusteContextoDePaginacao(listaPaginada);
                return View("_TabelaPaginada", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Adicionar()
        {
            try
            {
                return View(new RedeDeEnsino { Ativa = true });
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult Alterar(int id)
        {
            try
            {
                return View("Alterar", _servico.Obtenha(id));
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(RedeDeEnsino redeDeEnsino)
        {
            int id = redeDeEnsino.Id;

            if (redeDeEnsino.Id == 0)
                id = _servico.Adicionar(redeDeEnsino);
            else
                _servico.Atualizar(redeDeEnsino);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public ActionResult AutenticarRedeDeEnsino(int id)
        {
            var redeDeEnsino = _servico.Obtenha(id);

            var dadosDaSessao = HttpContext.DadosDaSessao();
            if (dadosDaSessao.Perfil != ePerfilDeAcesso.AdministradorDeSistema)
                throw new ApplicationException("Sem permissão");

            HttpContext?.Session?.Clear();

            dadosDaSessao.RedeDeEnsinoId = id;
            dadosDaSessao.RedeDeEnsinoNome = redeDeEnsino.Nome;
            dadosDaSessao.Perfil = ePerfilDeAcesso.RedeDeEnsino;
            this.Autenticar(dadosDaSessao);

            return Json(new { sucesso = true });
        }

        [HttpGet]
        public ActionResult Brasao(int id)
        {
            try
            {
                var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "brasoes", $"{id}.png");
                var imageFileStream = System.IO.File.OpenRead(path);
                return File(imageFileStream, "image/jpeg");
            }
            catch (Exception ex)
            {
                return File("~/img/brasoes/sem-foto.jpg", "image/png");
            }
        }

        [HttpPost]
        public ActionResult PostBrasao(int id, IFormFile file)
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
                var imagem = ImageToByteArray(ResizeImage(ByteArrayToImage(arquivo), new Size(140, 140)));

                var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "brasoes", $"{id}.png");
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                    fs.Write(imagem, 0, (int)imagem.Length);
            }

            return Json(new { sucesso = true, mensagem = "Imagem atualizada." });
        }

        private static byte[] ImageToByteArray(System.Drawing.Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private static System.Drawing.Image ResizeImage(System.Drawing.Image imgToResize, Size size)
            => (System.Drawing.Image)(new Bitmap(imgToResize, size));

        private System.Drawing.Image ByteArrayToImage(byte[] bytesArr)
        {
            using (MemoryStream memstr = new MemoryStream(bytesArr))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(memstr);
                return img;
            }
        }

        private void AjusteContextoDePaginacao(ListaPaginada<RedeDeEnsino> listaPaginada)
        {
            var dados = _servico.ObtenhaListaPaginada(
                c => true,
                listaPaginada.Pagina,
                listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}
