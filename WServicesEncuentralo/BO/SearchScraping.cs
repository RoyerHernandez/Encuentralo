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

namespace WServicesEncuentralo.BO
{
    public class SearchScraping
    {
        public async Task<List<Book>> GetBooks(Search search)
        {
            List<Book> books = new List<Book>();
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument doc = htmlWeb.Load("https://www.panamericana.com.co/libros/" + search.Criteria);
            List<HtmlNode> article = new List<HtmlNode>();
            article = doc.DocumentNode.CssSelect(".item__showcase__category").ToList();
            List<Task> tasksitas = new List<Task>();
            foreach (HtmlNode nodo in article)
            {

                Task task = new Task(() => LoadDetails(nodo, books));
                task.Start();

                tasksitas.Add(task);
            }
            while (!books.Count.Equals(article.Count))
            {
                if (!books.Count.Equals(article.Count))
                {
                    await Task.WhenAll(tasksitas.ToArray());

                }
            }
            return books;

        }
        private async void LoadDetails(HtmlNode nodo, List<Book> books)
        {
            string link = nodo.GetAttributeValue("data-href");
            HtmlNode image = nodo.CssSelect(".item__showcase__category__figure").CssSelect("a").CssSelect("img").First();
            HtmlNode infoBook = nodo.CssSelect(".item__showcase__category__namePrice").CssSelect(".item__showcase__category__price").First();

            string linkImage = image.GetAttributeValue("src");
            Book book = new Book
            {
                Name = HttpUtility.HtmlDecode((infoBook.CssSelect("p").Count() == 0)?"":infoBook.CssSelect("p").First().GetAttributeValue("title")),
                Price = clearPrice((infoBook.CssSelect("span") == null) ?"":HttpUtility.HtmlDecode(infoBook.CssSelect("span").First().InnerHtml)),
                Url = link,
                ImageUrl = linkImage
            };


            if (book.Url != null)
            {
                await Task.Run(() =>
                {
                    HtmlWeb htmlWeb = new HtmlWeb();
                    HtmlDocument doc = htmlWeb.Load(book.Url);
                    List<HtmlNode> infoTec = new List<HtmlNode>();
                    HtmlNode infoDetail = doc.DocumentNode.CssSelect(".productDescription").First();
                    book.Description = HttpUtility.HtmlDecode(infoDetail.InnerText);
                    infoTec = doc.DocumentNode.CssSelect("#fichaTecnicaPR").ToList();
                    foreach (var item in infoTec)
                    {
                        book.Author = HttpUtility.HtmlDecode(item.CssSelect(".Autor-es-").ToList()[1].InnerText);
                        book.ISBN = HttpUtility.HtmlDecode(item.CssSelect(".ISBN").First().InnerText);
                        book.Language = HttpUtility.HtmlDecode(item.CssSelect(".Idioma").ToList()[1].InnerText);
                        book.Editorial = HttpUtility.HtmlDecode(doc.DocumentNode.CssSelect(".brand").First().InnerText);
                    }
                    books.Add(book);
                });
            }
        }
        private string clearPrice(string price)
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
    }
}