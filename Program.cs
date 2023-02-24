using CrawlDataWebsiteTool.Models;
using CrawlDataWebsiteToolBasic.Helpers;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Text;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Edge;
using Markdig;
using System.IO;
using OfficeOpenXml;
using PuppeteerSharp;
// /khởi tạo đối tượng LaunchOptions với thuộc tính Headless được set bằng true. Nếu Headless là true, browser sẽ chạy ở chế độ headless (không có giao diện người dùng).
var options = new LaunchOptions
{
    Headless = true // set to true if you want to run the browser in headless mode
};
/// 
/// <param name="currentPath"> Get curent path of project | Lấy đường dẫn của chương trình </param>
/// <param name="savePathExcel"> Path save excel file | Đường dẫn để lưu file excel </param>
/// <param name="baseUrl"> URL website need to crawl | Đường dẫn trang web cần crawl </param>
/// 


var currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
var savePathExcel = currentPath.Split("bin")[0] +@"data-crawl\";
if (!Directory.Exists(savePathExcel))
    {
        Directory.CreateDirectory(savePathExcel);
    }

const string baseUrl = "https://www.hazzys.com";

//List mã loại sản phẩm
var typeCodes = new List<int>() {1};

// List product crawl
// List lưu danh sách các sản phẩm Crawl được
var listDataExport = new List<ProductModel>();
var listLinkProduct = new List<string>();
var linkProductSet = new HashSet<string>(File.ReadAllLines(currentPath.Split("bin")[0]+"saved-product.txt"));
var newLinkProduct = new HashSet<string>();

Console.WriteLine("Please do not turn off the app while crawling!");

// Sử dụng đối tượng BrowserFetcher để tải xuống phiên bản mới nhất của Chromium browser được sử dụng bởi PuppeteerSharp.
await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

//Loop 1
foreach (var typeCode in typeCodes)
{
    var requestUrl = baseUrl + $"/display.do?cmd=getTCategoryMain&TCAT_CD=1000{typeCode}";

    // Khởi tạo trình duyệt
    using (var browser = await Puppeteer.LaunchAsync(options))
    using (var page = await browser.NewPageAsync())
    {
        await page.GoToAsync(requestUrl);
        await page.WaitForSelectorAsync(".gnb-wrap__menu", new WaitForSelectorOptions { Timeout = 60000 });
        // do something with the page

        var stopTime = DateTime.Now.AddMinutes(5);
        while (DateTime.Now < stopTime)
        {
            var elements = (await page.QuerySelectorAllAsync (".pro-wrap__items"))
            .ToList();

            if (elements.Count > 0)
            {
                
                foreach (var element in elements)
                {

                    // Lấy link sản phẩm
                    var linkLabelImageProduct = await element.EvaluateFunctionAsync<string>("e => e.id");

                    var linkProduct =baseUrl + "/product.do?cmd=getProductDetail&PROD_CD=" +linkLabelImageProduct;

                    if (!linkProductSet.Contains(linkProduct))
                    {
                        listLinkProduct.Add(linkProduct);
                        newLinkProduct.Add(linkProduct);
                    }
                }
                break;
            }
        }
    }
}


// Loop 2
foreach (var link in listLinkProduct)
{

    // Khởi tạo trình duyệt
    using (var browser = await Puppeteer.LaunchAsync(options))
    using (var page = await browser.NewPageAsync())
    {
        await page.GoToAsync(link, new NavigationOptions { Timeout = 120000 });
        await page.WaitForSelectorAsync(".pro-img-area", new WaitForSelectorOptions { Timeout = 120000});
        // do something with the page

        while (true)
        {
            var elements = (await page.QuerySelectorAllAsync (".contents"))
            .ToList();
            
             if (elements.Count == 0 || await page.EvaluateExpressionAsync<bool>("window.innerHeight + window.scrollY >= document.body.scrollHeight"))
            {
                break;
            }

               // scroll xuống dưới trang để tải thêm phần tử
            await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)");
            await page.WaitForTimeoutAsync(1000);

             if (elements.Count > 0)
            {
                foreach (var element in elements)
                {
                     // Tên sản phẩm
                    var nameProduct = "";
                    try
                    {
                        nameProduct = await element.EvaluateFunctionAsync<string>("() => document.querySelector('.tit-depth02').textContent");
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Không tìm thấy đối tượng, bỏ qua và tiếp tục chương trình");
                        nameProduct = "Lấy dữ liệu lỗi";
                    }       

                    // Phân loại
                     var typeProduct = "";
                    try
                    {
                        typeProduct =
                         await element.EvaluateFunctionAsync<string>("() => document.querySelector('.pro-detail__cont .pro-detail__cont--aside .box-info .txt').textContent");
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Không tìm thấy đối tượng, bỏ qua và tiếp tục chương trình");
                        typeProduct = "Lấy dữ liệu lỗi";
                    }

                     // Giá bán
                    string sellPrice="";
                    try
                    {
                        sellPrice = 
                        (await element.EvaluateFunctionAsync<string>("() => document.querySelector('.pro-detail__cont .pro-detail__cont--aside .box-cash .box-cash__pay .discount').textContent"))
                        .ReplaceMultiToEmpty(new List<string>() {"원"});
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Không tìm thấy đối tượng, bỏ qua và tiếp tục chương trình");
                        sellPrice = "Lấy dữ liệu lỗi";
                    }

                    // Giá gốc
                    String originPrice="";

                    try
                    {
                        originPrice = await element.EvaluateFunctionAsync<string>("() => document.querySelector('.pro-detail__cont .pro-detail__cont--aside .box-cash .box-cash__sale .sale').textContent");
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Không tìm thấy đối tượng, bỏ qua và tiếp tục chương trình");
                        originPrice = "Lấy dữ liệu lỗi";
                    }

                      //Chênh lệch
                    String retail="";
                    try 
                    {
                        retail = await element.EvaluateFunctionAsync<string>("() => document.querySelector('.pro-detail__cont .pro-detail__cont--aside .box-cash .box-cash__sale .retail').textContent");
                        
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Không tìm thấy đối tượng, bỏ qua và tiếp tục chương trình");
                        retail = "Lấy dữ liệu lỗi";
                    }

                    //Giới thiệu sản phẩm
                    var nodeIntroProducts= await element.QuerySelectorAllAsync(".view-info__cont p");
                    
                    var introProducts = new List<string>();

                    foreach (var nodeIntroProduct in nodeIntroProducts)
                    {
                        try
                        {
                            introProducts
                            .Add((await nodeIntroProduct.EvaluateFunctionAsync<string>("node => node.textContent")).TrimStart());
                        }
                        catch (NoSuchElementException)
                        {
                            Console.WriteLine("Không tìm thấy đối tượng, bỏ qua và tiếp tục chương trình");

                            introProducts.Add("Lấy dữ liệu lỗi");
                        }
                    }
                    var introProduct = string.Join("\n", introProducts);

                     // Mã sản phẩm, trọng lượng, nguyên liệu
                    var nodeInfoes = await element.QuerySelectorAllAsync(".box-line.box-line__tb2 .box-line__list li span");
                    var infoes = new List<string>();
                    foreach (var nodeInfo in nodeInfoes)
                    {
                        try
                        {
                            infoes.Add(await nodeInfo.EvaluateFunctionAsync<string>("node => node.textContent"));
                        }
                        catch
                        {
                            Console.WriteLine("Không tìm thấy đối tượng, bỏ qua và tiếp tục chương trình");
                            
                        }
                    }

                    var productCode = infoes[0];
                    var meterial = infoes[1];
                    var mass = infoes[2];

                     // Hình ảnh
                    var nodesDetailImg = await element.QuerySelectorAllAsync(".pro-img-area img");

                    var folderPath = Path.Combine(savePathExcel, "images", nameProduct);

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    foreach ( var nodeDetailImg in nodesDetailImg)
                    {
                        var linkDetailImg = await nodeDetailImg.EvaluateFunctionAsync<string>("e => e.src");
                        var fileNameImg = Path.GetFileName(linkDetailImg)+".jpg";
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
                        ProductType = typeProduct,
                        DiscountPrice = sellPrice,
                        OriginPrice = originPrice,
                        Retail= retail,
                        ProducOrder=(listDataExport.Count+1).ToString(),
                        Currency = "원 (won)",
                        IntroProduct = introProduct,
                        ProductCode = productCode,
                        Meterial = meterial,
                        Mass = mass
                    });
                }
            }
            break;
        }
    }
    
}

var fileName = DateTime.Now.Ticks + "_Hayzzys-crawl.xlsx";


// Export data to Excel
ExportToExcel<ProductModel>.GenerateExcel(listDataExport, savePathExcel + fileName, "_hayzzys-crawl");
File.AppendAllLines(currentPath.Split("bin")[0]+"saved-product.txt", newLinkProduct);

// Read Data from excel file 
var fileExcel = savePathExcel + fileName;
var package = new ExcelPackage(new FileInfo(fileExcel));
var Worksheet = package.Workbook.Worksheets[0];
var rows = Worksheet.Cells[Worksheet.Dimension.Address].GroupBy(cell => cell.Start.Row).Skip(1).ToList();

// Duyệt từng hàng
foreach (var row in rows)
{
    var rowData = new List<string>();
    
    foreach (var cell in row)
    {
        rowData.Add(cell.Value.ToString());
    }
    var markdown = new StringBuilder();
    var name = rowData[1];
    var type = rowData[2];
    var intro = rowData[3];
    var code = rowData[4];
    var material = rowData[5];
    var mass = rowData[6];
    var sell = rowData[7];
    var origin = rowData[8];
    var retail = rowData[9];
    var currency = rowData[10];

    markdown.AppendLine($"# {name} ({type})");
    markdown.AppendLine($"- Mã sản phẩm: {code}");
    markdown.AppendLine($"- Chất liệu: {material}");
    markdown.AppendLine($"- Khối lượng: {mass}");
    markdown.AppendLine($"- Giá bán: {sell} {currency}");
    markdown.AppendLine($"- Giá gốc: {origin} {currency}");
    markdown.AppendLine($"- Giảm giá: {retail} {currency}");
    
    markdown.AppendLine();
    markdown.AppendLine(intro);
    markdown.AppendLine(); 

    // Save MD file
    var mdFileName = name+".md";
    var mdFilePath = Path.Combine(savePathExcel, "md-file", mdFileName);
    if (!Directory.Exists(mdFilePath))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(mdFilePath));
    }
    File.WriteAllText(mdFilePath, markdown.ToString());

}
