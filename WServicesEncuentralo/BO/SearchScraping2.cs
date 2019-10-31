using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using WServicesEncuentralo.Models;
using WServicesEncuentralo.DAO;
using System.Data;

namespace WServicesEncuentralo.BO
{
    public class SearchScraping2
    {
        public async Task<List<Book>> GetBooks2(Search search)
        {
            //search.Criteria = "Cazadores de sombras";
            return await Books(search, ConsultaInfoLib());
        }

        private async Task<List<Book>> Books(Search search, List<MotorConfig> motorConfigs)
        {
            List<Book> books = new List<Book>();
            int cantRegistro = 0;
            foreach (MotorConfig config in motorConfigs)
            {
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument doc = htmlWeb.Load(string.Format(config.UrlPagina, search.Criteria));
                List<HtmlNode> article = new List<HtmlNode>();
                article = doc.DocumentNode.CssSelect(config.ContenedorPrincipal).ToList();
                cantRegistro += article.Count();
                List<Task> tasks = new List<Task>();
                foreach (HtmlNode nodo in article)
                {
                    Task task = new Task(() => LoadDetails(nodo, books, config));
                    task.Start();
                    tasks.Add(task);
                }
                while (!books.Count.Equals(cantRegistro))
                {
                    if (!books.Count.Equals(cantRegistro))
                    {
                        await Task.WhenAll(tasks.ToArray());
                    }
                }
            }
            return books;
        }
        private async void LoadDetails(HtmlNode nodo, List<Book> books, MotorConfig config)
        {
            Book book = new Book
            {
                Store = config.Pagina,
                Name = HttpUtility.HtmlDecode(ObtenerTexto(config.RootNombre, nodo)),
                Price = HttpUtility.HtmlDecode(ClearPrice(ObtenerTexto(config.RootPrice, nodo))),
                Url = ObtenerTexto(config.RootUrlArticulo, nodo),
                ImageUrl = ObtenerTexto(config.RootImageUrl, nodo)
            };
            if (book.Url != null)
            {
                await Task.Run(() =>
                {
                    HtmlWeb htmlWeb = new HtmlWeb();
                    HtmlDocument doc = htmlWeb.Load(book.Url);
                    List<HtmlNode> infoTec = new List<HtmlNode>();
                    book.Description = HttpUtility.HtmlDecode(ObtenerTexto(config.RootDescripcion, doc.DocumentNode));

                    book.Author = HttpUtility.HtmlDecode(ObtenerTexto(config.RootAutor, doc.DocumentNode));
                    book.ISBN = HttpUtility.HtmlDecode(ObtenerTexto(config.RootISBN, doc.DocumentNode));
                    book.Language = HttpUtility.HtmlDecode(ObtenerTexto(config.RootLenguaje, doc.DocumentNode));
                    book.Editorial = HttpUtility.HtmlDecode(ObtenerTexto(config.RootEditorial, doc.DocumentNode));

                    books.Add(book);
                });
            }
        }

        private string ObtenerTexto(string root, HtmlNode nodo)
        {
            string textResult = "";
            try
            {

                if (root != "")
                {
                    string[] nod = root.Split('/');
                    int count = 1;
                    HtmlNode node = nodo;
                    foreach (string part in nod)
                    {
                        if (count.Equals(nod.Length))
                        {
                            string[] att = part.Split('|');
                            List<HtmlNode> lstVal = node.CssSelect(att[0]).ToList();
                            int inx = 0;
                            if (lstVal.Count > 1)
                            {
                                inx = lstVal.Count - 1;
                            }
                            if (att[1].Equals("IT"))
                            {
                                textResult = lstVal[inx].InnerText;
                            }
                            else
                            {
                                textResult = node.GetAttributeValue(att[0]).ToString();
                            }
                        }
                        else
                        {
                            if (node.CssSelect(part).Count() > 0)
                            {
                                node = node.CssSelect(part).First();
                                count++;
                            }
                            else
                            {
                                textResult = "";
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            { }
            return textResult;
        }
        private string ClearPrice(string price)
        {
            try
            {
                price = Regex.Replace(price.ToString(), @"[^\w\$.,@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (Exception)
            {

            }

            return price;
        }
        private List<MotorConfig> ConsultaInfoLib()
        {
            DataTable dtResult = new DataTable();
            ClsConexion conn = new ClsConexion() { Nombre = "SP_ConfigMotor" };
            dtResult = conn.EjecutarProcedimiento("strConnRevo");

            List<MotorConfig> motorConfigs = new List<MotorConfig>();
            motorConfigs = (from c in dtResult.AsEnumerable()
                            select new MotorConfig()
                            {
                                Pagina = c["nombreProveedor"].ToString(),
                                UrlPagina = c["rutaUrl"].ToString(),
                                ContenedorPrincipal = c["contenedorPrincipal"].ToString(),
                                RootNombre = c["Nombre"].ToString(),
                                RootPrice = c["Price"].ToString(),
                                RootDescripcion = c["Descripcion"].ToString(),
                                RootAutor = c["Autor"].ToString(),
                                RootISBN = c["ISBN"].ToString(),
                                RootLenguaje = c["Lenguaje"].ToString(),
                                RootEditorial = c["Editorial"].ToString(),
                                RootUrlArticulo = c["UrlArticulo"].ToString(),
                                RootImageUrl = c["ImageUrl"].ToString()
                            }).ToList();
            return motorConfigs;

        }

    }
}