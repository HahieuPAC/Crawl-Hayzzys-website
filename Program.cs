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
var typeCodes = new List<int>() {1, 2, 3, 4, 5};

// List product crawl
// List lưu danh sách các sản phẩm Crawl được
var listDataExport = new List<ProductModel>();
var listLinkProduct = new List<string>();

Console.WriteLine("Please do not turn off the app while crawling!");

//Loop 1
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
                    

                    // Lấy link sản phẩm
                    var linkLabelImageProduct = Path.GetFileName(element
                    .FindElement(By.CssSelector(".pro-wrap__img img"))
                    .GetAttribute("src")).Split("_00.jpg")[0];

                    var linkProduct =baseUrl + "/product.do?cmd=getProductDetail&PROD_CD=" +linkLabelImageProduct;

                    listLinkProduct.Add(linkProduct);
                }
                break;
            }
        }
        driver.Close();
}

//Loop 2
foreach (var link in listLinkProduct)
{
    Console.WriteLine(link);
    var driver = new EdgeDriver(currentPath.Split("bin")[0]);
    driver.Navigate().GoToUrl(link);
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1000));
    wait.Until(d => d.FindElements(By.ClassName("pro-detail")).Count > 0);

    var stopTime = DateTime.Now.AddMinutes(5);
    while (DateTime.Now < stopTime)
    {
        
        var elements = driver.FindElements(By.CssSelector(".pro-detail"));
        if (elements.Count > 0)
        {
            foreach (var element in elements)
            {
                // Tên sản phẩm
                var nameProduct = element
                .FindElement(By.ClassName("tit-depth02"))
                .Text
                .ReplaceMultiToEmpty(new List<string>() { "/", "|", "?", ":", "*", ">", "<"});

                // Phân loại
                var TypeProduct = element
                .FindElement(By.CssSelector(".pro-detail__cont .pro-detail__cont--aside .box-info .txt"))
                .Text;

                // Giá bán
                var sellPrice =  element
                .FindElement(By.CssSelector(".pro-detail__cont .pro-detail__cont--aside .box-cash .box-cash__pay .discount"))
                .Text;

                // Giá gốc
                var originPrice = element
                .FindElement(By.CssSelector(".pro-detail__cont .pro-detail__cont--aside .box-cash .box-cash__sale .sale"))
                .Text;

                //Chênh lệch
                var retail = element
                .FindElement(By.CssSelector(".pro-detail__cont .pro-detail__cont--aside .box-cash .box-cash__sale .retail"))
                .Text;


                // Hình ảnh
                var nodesDetailImg = element.FindElements(By.CssSelector(".pro-detail__photo .pro-detail-small .swiper-slide img"));

                Console.WriteLine(nodesDetailImg.Count);

                var folderPath = Path.Combine(savePathExcel, "Images", nameProduct);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                foreach ( var nodeDetailImg in nodesDetailImg)
                {
                    var linkDetailImg = nodeDetailImg.GetAttribute("src");
                    var fileNameImg = Path.GetFileName(linkDetailImg);
                    var filePathImg = Path.Combine(folderPath, fileNameImg);
                    try
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(new Uri(linkDetailImg), filePathImg);
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine($"Lỗi tải ảnh từ đường dẫn: {linkDetailImg}, {ex.Message}");

                    }
                }

                 // Thêm sản phẩm vào listDataExport
                 listDataExport.Add(new ProductModel()
                 {
                    ProductName = nameProduct,
                    ProductType = TypeProduct,
                    DiscountPrice = sellPrice,
                    OriginPrice = originPrice,
                    Retail = retail
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
