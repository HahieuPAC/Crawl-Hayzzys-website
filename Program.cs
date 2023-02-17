using CrawlDataWebsiteTool.Models;
using CrawlDataWebsiteToolBasic.Functions;
using CrawlDataWebsiteToolBasic.Helpers;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Text;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Edge;

/// 
/// <param name="currentPath"> Get curent path of project | Lấy đường dẫn của chương trình </param>
/// <param name="savePathExcel"> Path save excel file | Đường dẫn để lưu file excel </param>
/// <param name="baseUrl"> URL website need to crawl | Đường dẫn trang web cần crawl </param>
/// 


var currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
var savePathExcel = currentPath.Split("bin")[0] +@"Data Crawl\";

const string baseUrl = "https://www.hazzys.com";

//List mã loại sản phẩm
var typeCodes = new List<int>() {1};

// List product crawl
// List lưu danh sách các sản phẩm Crawl được
var listDataExport = new List<ProductModel>();

Console.WriteLine("Please do not turn off the app while crawling!");

//Loop
foreach (var typeCode in typeCodes)
{
    var requestUrl = baseUrl + $"/display.do?cmd=getTCategoryMain&TCAT_CD=1000{typeCode}";
    Console.WriteLine(requestUrl);

    var driver = new EdgeDriver(currentPath.Split("bin")[0]);
    driver.Navigate().GoToUrl(requestUrl);
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1000));
    wait.Until(d => d.FindElements(By.ClassName("pro-wrap__items")).Count > 0);

    var stopTime = DateTime.Now.AddMinutes(5);
        while (DateTime.Now < stopTime)
        {
            var elements = driver.FindElements(By.ClassName("pro-wrap__items"));

            if (elements.Count > 0)
            {
                foreach (var element in elements)
                {
                    // tên sản phẩm
                    var nameProduct = element
                    .FindElement(By.CssSelector(".pro-wrap__obj .pro-name"))
                    .Text
                    .ReplaceMultiToEmpty(new List<string>() { "/", "|", "?", ":", "*", ">", "<"});

                // Phân loại
                var typeProduct = element
                    .FindElement(By.CssSelector(".pro-wrap__obj .pro-brand"))
                    .Text;

                    // Giá bán
                    var sellPrice = element
                    .FindElement(By.CssSelector(".pro-wrap__obj .pro-util .pro-util__sale"))
                    .Text;

                    // Tải ảnh

                    var linkLabelImageProduct = Path.GetFileName(element
                    .FindElement(By.CssSelector(".pro-wrap__img img"))
                    .GetAttribute("src")).Split("_00.jpg")[0];

                    var linkProduct =baseUrl + "/product.do?cmd=getProductDetail&PROD_CD=" +linkLabelImageProduct;
                    var folderPath = Path.Combine(savePathExcel, "Images", nameProduct);

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    driver.Navigate().GoToUrl(linkLabelImageProduct);
                    wait.Until(g => g.FindElements(By.CssSelector(".pro-detail__photo .swiper-slide img")).Count > 0);
                    while (DateTime.Now < stopTime)
                    {
                        var imgProdetail = driver.FindElements(By.ClassName(".pro-detail__photo .swiper-slide img"));
                        if (imgProdetail.Count > 0)
                        { 
                            foreach (var ImgDetail in imgProdetail )
                            {
                                var linkImgDetail = ImgDetail.GetAttribute("src");
                                var fileNameImage = name


                                WebClient webClient = new WebClient();
                                webClient.DownloadFile(new Uri)



                            }
                        }
                    }

                    

                    // Add Product to listDataExport
                    // Thêm sản phẩm vào listDataExport
                    listDataExport.Add(new ProductModel()
                    {
                        ProductName = nameProduct,
                        ProductType = typeProduct,
                        DiscountPrice = sellPrice,
                    });
                }
                break;
            }
        }

    driver.Close();
}
var fileName = DateTime.Now.Ticks + "_Hayzzys-crawl.xlsx";


// Export data to Excel
ExportToExcel<ProductModel>.GenerateExcel(listDataExport, savePathExcel + fileName, "_hayzzys-crawl");
