using AriD.BibliotecaDeClasses.Entidades;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing.Imaging;
using System.Drawing;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class OrganizacaoController : Controller
    {
        private readonly IServico<Organizacao> _servico;

        public OrganizacaoController(IServico<Organizacao> servico)
        {
            _servico = servico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Organizacao> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<Organizacao> listaPaginada)
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
                return View(new Organizacao { Ativa = true });
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
        public IActionResult Salvar(Organizacao organizacao)
        {
            int id = organizacao.Id;

            if (organizacao.Id == 0)
            {
                organizacao.NomenclaturaServidor = eNomenclaturaServidor.Servidores;
                id = _servico.Adicionar(organizacao);
            }
            else
            {
                var persistido = _servico.Obtenha(organizacao.Id);
                
                persistido.NomenclaturaServidor = organizacao.NomenclaturaServidor;
                persistido.Nome = organizacao.Nome;
                persistido.Endereco = organizacao.Endereco;
                persistido.GestaoMobileAtivo = organizacao.GestaoMobileAtivo;

                if (this.HttpContext.DadosDaSessao().Perfil == ePerfilDeAcesso.AdministradorDeSistema)
                {
                    persistido.EnvioDeMensagemWhatsAppExperimental = organizacao.EnvioDeMensagemWhatsAppExperimental;
                    persistido.Ativa = organizacao.Ativa;
                }

                persistido.RecebeNotificacaoConectividade = organizacao.RecebeNotificacaoConectividade;
                persistido.EmailNotificacaoConectividade = organizacao.EmailNotificacaoConectividade;

                _servico.Atualizar(persistido);
            }

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public ActionResult AutenticarOrganizacao(int id)
        {
            var organizacao = _servico.Obtenha(id);

            var dadosDaSessao = HttpContext.DadosDaSessao();
            if (dadosDaSessao.Perfil != ePerfilDeAcesso.AdministradorDeSistema)
                throw new ApplicationException("Sem permissão");

            HttpContext?.Session?.Clear();

            dadosDaSessao.OrganizacaoId = id;
            dadosDaSessao.OrganizacaoNome = organizacao.Nome;
            dadosDaSessao.Perfil = ePerfilDeAcesso.Organizacao;
            dadosDaSessao.NomenclaturaServidor = organizacao.NomenclaturaServidor;
            dadosDaSessao.GestaoMobileAtivo = organizacao.GestaoMobileAtivo;
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

        private void AjusteContextoDePaginacao(ListaPaginada<Organizacao> listaPaginada)
        {
            var dados = _servico.ObtenhaListaPaginada(
                c => true,
                listaPaginada.Pagina,
                listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}