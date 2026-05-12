using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using ProjectSolution.Services.ProductAPI.Data;
using ProjectSolution.Services.ProductAPI.Extensions;
using ProjectSolution.Services.ProductAPI.Models;
using ProjectSolution.Services.ProductAPI.Models.Dto;

namespace ProjectSolution.Services.ProductAPI.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private ResponseDto _responseDto = new();
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [HttpGet]
        public async Task<ResponseDto> GetAllProducts()
        {
            try
            {
                List<ProductDto> products = await _dbContext.Products.Select(p => p.ToProductDto()).ToListAsync();
                if (products == null)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "No product was found";
                    return _responseDto;
                }
                _responseDto.Result = products;
            }
            catch (Exception ex) // this catch block is to catch any unexpected errors that may occur during database access or data processing.
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpGet("{id:int}")]
        public async Task<ResponseDto> GetProductById(int id)
        {
            try
            {
                Product? productById = await _dbContext.Products.FirstOrDefaultAsync(u => u.ProductId == id);
                if (productById == null)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Product couldn't be found by the given id";
                    return _responseDto;
                }
                _responseDto.Result = productById.ToProductDto();
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        //[HttpGet("{productName:string}")] // can't use this route because it conflicts with the GetProductById route
        [HttpGet("GetProductByName/{productName}")] // don't need to specify :string, it's implied, if specified it causes an error, why? lead to routing conflicts or ambiguity.
        //Default Type: ASP.NET Core routing treats path segments as string unless you specify a different type(like int, Guid, etc.). string option is also not supported in route constraints.
        public async Task<ResponseDto> GetProductByName(string productName)
        {
            try
            {
                Product? productByName = await _dbContext.Products.FirstOrDefaultAsync(u => u.ProductName == productName);
                if (productByName == null)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Product couldn't be found by the given name";
                    return _responseDto;
                }
                _responseDto.Result = productByName.ToProductDto();
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Post(ProductDto productDto)
        {
            if (productDto == null)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = "Product cannot be null";
                return _responseDto;
            }
            var product = productDto.ToProduct();
            try
            {
                var existingProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductName == product.ProductName);
                if (existingProduct != null)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "A product with the same name already exists";
                    return _responseDto;
                }

                if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
                {
                    // 1️) Validate extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(productDto.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        throw new Exception("Invalid image format. Only JPG, JPEG, PNG allowed.");

                        //return exits method normally
                        //throw exits method with an exception (eg. HTTP 500)
                    }

                    // 2️) Validate file size (5MB)
                    if (productDto.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        throw new Exception("File size exceeds 5MB limit.");
                    }

                    // 3) Get project root directory
                    //var rootPath = Directory.GetCurrentDirectory();
                    var rootPath = _env.ContentRootPath;

                    // 4) Define upload folder relative path
                    var relativeUploadPath = Path.Combine("Uploads", "ProductImages");

                    // 5) Combine to create absolute directory path
                    var absoluteUploadPath = Path.Combine(rootPath, relativeUploadPath);

                    // 6) Ensure folder exists
                    if (!Directory.Exists(absoluteUploadPath))
                    {
                        Directory.CreateDirectory(absoluteUploadPath);
                    }

                    // here we can save the product first to get the generated ProductId, then we can use that id to generate a unique filename for the image. At this stage we don't have any potential errors related to file handling(eg. extension, size), so we can safely save the product and get its id.
                    await _dbContext.Products.AddAsync(product);
                    await _dbContext.SaveChangesAsync();

                    // 7) Generate safe filename
                    string fileName = $"{product.ProductId}{extension}";

                    // 8) Build full file path
                    var fullFilePath = Path.Combine(absoluteUploadPath, fileName);

                    // 9) Save file
                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await productDto.ImageFile.CopyToAsync(stream);
                    }

                    // 10) Store path + URL
                    var baseUrl =
                        $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                    product.ImageUrl = $"{baseUrl}/api/product/image/{fileName}";
                    product.ImageLocalPath = Path.Combine(relativeUploadPath, fileName);
                }
                else
                {
                    product.ImageUrl = "https://placehold.co/600x400";
                }

                _dbContext.Products.Update(product);
                await _dbContext.SaveChangesAsync();

                _responseDto.Message = "Product created successfully";
                _responseDto.Result = productDto;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Put(ProductDto productDto)
        {
            if (productDto == null)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = "Product cannot be null";
                return _responseDto;
            }
            try
            {
                var existingProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == productDto.ProductId);
                if (existingProduct == null)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Product to be updated couldn't be found";
                    return _responseDto;
                }
                var productWithSameName = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.ProductName == productDto.ProductName && p.ProductId != productDto.ProductId);
                if (productWithSameName != null)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Another product with the same name already exists";
                    return _responseDto;
                }



                if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
                {
                    // if the user is uploading a new image, we need to delete the old image from the server to avoid orphaned files and save storage space.
                    if (!string.IsNullOrEmpty(productDto.ImageLocalPath))
                    {
                        var imagePath = Path.Combine(_env.ContentRootPath, productDto.ImageLocalPath);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    // 1️) Validate extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(productDto.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        throw new Exception("Invalid image format. Only JPG, JPEG, PNG allowed.");

                        //return exits method normally
                        //throw exits method with an exception (eg. HTTP 500)
                    }

                    // 2️) Validate file size (5MB)
                    if (productDto.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        throw new Exception("File size exceeds 5MB limit.");
                    }

                    // 3) Get project root directory
                    //var rootPath = Directory.GetCurrentDirectory();
                    var rootPath = _env.ContentRootPath;

                    // 4) Define upload folder relative path
                    var relativeUploadPath = Path.Combine("Uploads", "ProductImages");

                    // 5) Combine to create absolute directory path
                    var absoluteUploadPath = Path.Combine(rootPath, relativeUploadPath);

                    // 6) Ensure folder exists
                    if (!Directory.Exists(absoluteUploadPath))
                    {
                        Directory.CreateDirectory(absoluteUploadPath);
                    }

                    // 7) Generate safe filename
                    string fileName = $"{productDto.ProductId}{extension}";

                    // 8) Build full file path
                    var fullFilePath = Path.Combine(absoluteUploadPath, fileName);

                    // 9) Save file
                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await productDto.ImageFile.CopyToAsync(stream);
                    }

                    // 10) Store path + URL
                    var baseUrl =
                        $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                    productDto.ImageUrl = $"{baseUrl}/api/product/image/{fileName}";
                    productDto.ImageLocalPath = Path.Combine(relativeUploadPath, fileName);
                }

                //_dbContext.Products.Update(product); // can't update the product like this, see why at the bottom.
                // Instead, update the properties of the existing product entity

                // ✅ Update the tracked entity
                existingProduct.ProductName = productDto.ProductName;
                existingProduct.Price = productDto.Price;
                existingProduct.Description = productDto.Description;
                existingProduct.CategoryName = productDto.CategoryName;
                existingProduct.ImageUrl = productDto.ImageUrl;
                existingProduct.ImageLocalPath = productDto.ImageLocalPath;

                await _dbContext.SaveChangesAsync();
                _responseDto.Message = "Product updated successfully";
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpDelete("{productId:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Delete(int productId) // this argument should have the same name as in the route template
        {
            try
            {
                var product = await _dbContext.Products.FirstOrDefaultAsync(u => u.ProductId == productId);
                if (product == null)
                {
                    _responseDto.IsSuccess = false;
                    _responseDto.Message = "Product couldn't be found";
                    return _responseDto;
                }

                if (!string.IsNullOrEmpty(product.ImageLocalPath))
                {
                    var imagePath = Path.Combine(_env.ContentRootPath, product.ImageLocalPath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    //or,

                    //FileInfo fileInfo = new FileInfo(imagePath);
                    //if (fileInfo.Exists)
                    //{
                    //    fileInfo.Delete();
                    //}

                    //Advantages

                    //FileInfo is useful when you also need metadata like:

                    //file size
                    //creation date
                    //last modified time
                    //file attributes

                    //Example:

                    //long size = fileInfo.Length;
                    //DateTime created = fileInfo.CreationTime;

                    //3️) Performance comparison
                    //Method            Objects created         Performance
                    //File.Exists()     none slightly           faster
                    //FileInfo          creates object          slightly slower

                    //The difference is tiny, but for simple operations we usually prefer the static File API.
                }

                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
                _responseDto.Message = "Product deleted successfully";
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpGet("image/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads", "ProductImages");
            var filePath = Path.Combine(uploadsFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            //return PhysicalFile(filePath, contentType);

            //alternate approach using FileStreamResult:
            //Open file stream (better for large files)
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //Return streamed file
            //return new FileStreamResult(fileStream, contentType);
            return File(fileStream, contentType); // File() vs PhysicalFile() vs FileStreamResult: we can use File() helper method which internally decides the best way to serve the file based on the content type and file size. For small files, it may read the entire file into memory and serve it directly. For larger files, it may use a streaming approach similar to FileStreamResult. PhysicalFile() is more suitable when you want to serve a file directly from disk without any processing, while FileStreamResult gives you more control over how the file is read and served. In this case, since we are serving images which are typically small, using File() is a convenient choice that abstracts away the details while still providing efficient file serving. Use FileStreamResult over File() when you need to handle large files or want more control over the file streaming process, such as setting specific headers or handling range requests for partial content delivery.

            /*
            2️) Try to determine the content type
            if (!provider.TryGetContentType(filePath, out string? contentType))
            What this method does

            It tries to determine the correct MIME type based on the file extension.

            Example:

            filePath = "Uploads/ProductImages/10.jpg"

            The provider reads the extension:

            .jpg

            Then returns:

            image/jpeg
            Method parameters
            TryGetContentType(string filePath, out string contentType)
            Parameter	Meaning
            filePath	the file name/path
            out contentType	output MIME type

            If successful:

            contentType = "image/jpeg"

            If it doesn't recognize the extension → it returns false.

            3️⃣ Fallback MIME type
            contentType = "application/octet-stream";

            This is the default binary file type.

            Meaning:

            unknown binary file

            Browsers treat it as downloadable data.

            Example cases:

            File	MIME type returned
            .jpg	image/jpeg
            .png	image/png
            .abc (unknown)	application/octet-stream
            4️⃣ Return the file
            return PhysicalFile(filePath, contentType);

            PhysicalFile() is an ASP.NET Core method that:

            1️⃣ Reads the file from disk
            2️⃣ Sends it as the HTTP response
            3️⃣ Sets the Content-Type header

            Example HTTP response:

            HTTP/1.1 200 OK
            Content-Type: image/jpeg

            Then the browser:

            renders the image

            or downloads the file

            depending on the type.

            If you return the file without the correct MIME type:

            Content-Type: application/octet-stream

            the browser will download the image instead of displaying it.

            Correct MIME type ensures:

            MIME type	                Browser behavior
            image/jpeg	                show image
            image/png	                show image
            application/pdf	            open PDF viewer
            application/octet-stream    download file

            8️) Quick summary
            Line	                            Purpose
            FileExtensionContentTypeProvider()	maps extension → MIME type
            TryGetContentType()	                finds MIME type
            application/octet-stream	        fallback for unknown types
            PhysicalFile()	                    returns file in HTTP response

            These lines detect the correct MIME type of the file and return it so the browser knows how to handle the file (display it, open it, or download it).
             */
        }
    }
}





//Perfect — your code shows exactly why EF Core is throwing that error.
//Let’s walk through it line by line, then I’ll show you the correct fix (and why it’s the best practice).

//🔴 The exact problem in your code

//This line is the root cause 👇

//var existingProduct = await _dbContext.Products
//    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);


//👉 This line already tracks an entity inside the DbContext.

//EF Core is now tracking:

//existingProduct(ProductId = X)


//Then later you do:

//_dbContext.Products.Update(product);


//But product is a different object instance with the same ProductId.

//So EF Core now sees:

//existingProduct(ProductId = X)  ← tracked
//product(ProductId = X)  ← trying to track again ❌


//🚨 EF Core does NOT allow two tracked entities with the same key
//→ hence the exception.

//🧠 Why EF Core forbids this

//EF Core must know exactly one source of truth per primary key.

//If two objects with the same key were allowed:

//Which one should be saved?

//Which one represents the “real” state?

//So EF Core throws early to protect you.

//✅ Correct & Recommended Fix (BEST PRACTICE)
//✔ Update the already tracked entity

//You queried it — so use it.

// Code above.....

//✅ Why this is the correct approach

//✔ No tracking conflict
//✔ EF Core knows exactly what changed
//✔ Partial updates are safe
//✔ Matches real-world production patterns

//Rule of thumb:
//If you queried an entity → modify that instance → save.

//❌ What NOT to do (your original mistake)
//_dbContext.Products.Update(product); // ❌ wrong here


//Update() is not for this scenario.


//🎯 Interview-ready explanation

//The error occurs because EF Core is already tracking an entity with the same primary key when Update() is called on a different instance. The correct approach is to update the already tracked entity instead of attaching a new one.

//🧠 One - line summary

//You queried the product (EF Core started tracking it), then tried to update a second product instance with the same key — causing a tracking conflict.