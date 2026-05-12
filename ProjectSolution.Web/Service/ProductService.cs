using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using static ProjectSolution.Web.Utility.SD; //static is used to import static members of a class so that they can be accessed without class name prefix

namespace ProjectSolution.Web.Service
{
    public class ProductService : IProductService
    {
        private readonly IBaseService _baseService;

        /*** Wrong Approach - Do Not Use This ***/
        //private RequestDto requestDto = new() { Url = ProductAPIBase + "/api/product" };

        public ProductService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseDto> CreateProductAsync(ProductDto product)
        {
            RequestDto requestDto = new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = product,
                Url = ProductAPIBase + "/api/product",
                ContentType = ContentType.MultipartFormData
            };
            var responseDto = await _baseService.SendAsync(requestDto);
            if (responseDto == null)
            {
                return new ResponseDto()
                {
                    IsSuccess = false,
                    Message = "Error occurred while creating product."
                };
            }
            return responseDto;


            /*** Wrong Approach - Do Not Use This ***/
            //requestDto.ApiType = ApiType.POST;
            //requestDto.Data = product;

            //var responseDto = await _baseService.SendAsync(requestDto, false);
            //if (responseDto == null)
            //{
            //    return new ResponseDto()
            //    {
            //        IsSuccess = false,
            //        Message = "Error occurred while creating product."
            //    };
            //}
            //return responseDto;
        }

        public async Task<ResponseDto> UpdateProductAsync(ProductDto productDto)
        {
            RequestDto requestDto = new()
            {
                ApiType = ApiType.PUT,
                Data = productDto,
                Url = ProductAPIBase + "/api/product",
                ContentType = ContentType.MultipartFormData
            };
            var responseDto = await _baseService.SendAsync(requestDto);
            if (responseDto == null)
            {
                return new ResponseDto()
                {
                    IsSuccess = false,
                    Message = "Error occurred while updating product."
                };
            }
            return responseDto;
        }

        public async Task<ResponseDto> DeleteProductAsync(int productId)
        {
            RequestDto requestDto = new()
            {
                ApiType = ApiType.DELETE,
                Url = ProductAPIBase + "/api/product" + "/" + productId
            };
            var responseDto = await _baseService.SendAsync(requestDto);
            if (responseDto == null)
            {
                return new ResponseDto()
                {
                    IsSuccess = false,
                    Message = "Error occurred while deleting the product",
                };
            }
            return responseDto;
        }

        public async Task<ResponseDto> GetAllProductAsync()
        {
            RequestDto requestDto = new()
            {
                Url = ProductAPIBase + "/api/product"
            };
            var responseDto = await _baseService.SendAsync(requestDto);
            if (responseDto == null)
            {
                return new ResponseDto()
                {
                    IsSuccess = false,
                    Message = "Error occurred while retriving the products",
                };
            }
            return responseDto;
        }

        public async Task<ResponseDto> GetProductByIdAsync(int productId)
        {
            RequestDto requestDto = new()
            {
                Url = ProductAPIBase + "/api/product" + "/" + productId
            };
            var responseDto = await _baseService.SendAsync(requestDto);
            if (responseDto == null)
            {
                return new ResponseDto()
                {
                    IsSuccess = false,
                    Message = "Error occurred while retriving the product",
                };
            }
            return responseDto;
        }

        public Task<ResponseDto> GetProductByNameAsync(string productName)
        {
            // not required as per current requirements
            throw new NotImplementedException();
        }
    }
}


/*
 * Let’s go slow and concrete, with a timeline, not theory.

🔴 The key assumption (this is where confusion starts)

In your second approach, this must be true:

private RequestDto requestDto;


Meaning:

requestDto is a field

It is shared by all calls to CreateProductAsync

It is NOT recreated per request

Now remember:

ASP.NET Core handles multiple requests at the same time (concurrently).

🧠 What “two users at the same time” really means

It does NOT mean:

One finishes

Then the other starts

It means:

Both execute interleaved on different threads

Their code lines can run in any order

🧪 Step-by-step timeline (this is the key)

Assume:

requestDto is shared

Two users: User A and User B

Initial state
requestDto.Data = null

🟢 Thread A (User A)
requestDto.Data = ProductA;


Memory now:

requestDto.Data → ProductA

🔵 Thread B (User B) starts BEFORE A sends the request
requestDto.Data = ProductB;


Memory now:

requestDto.Data → ProductB

🟢 Thread A continues
await _baseService.SendAsync(requestDto);


❌ Thread A THINKS it’s sending ProductA
❌ But Data now contains ProductB

➡️ User A sends User B’s product

💥 This is called a RACE CONDITION

Two threads are:

Racing to modify the same object

Without synchronization

Producing non-deterministic bugs

These bugs are:

Random

Hard to reproduce

Often appear only in production

🔥 Why this is especially dangerous in web apps

ASP.NET Core is highly concurrent

Requests are processed in parallel

Even a Scoped service can serve many requests sequentially but concurrently awaited

And if the service is:

Singleton → 💣 catastrophic

Scoped → ❌ still unsafe

Transient → ❌ still unsafe if field reused

✅ Why the FIRST approach is safe
RequestDto requestDto = new RequestDto
{
    ApiType = ApiType.POST,
    Data = product
};


Each request gets:

Request A → its own RequestDto
Request B → its own RequestDto


No shared memory
No overwrites
No race condition

🧠 Simple rule to remember (burn this in)

If data changes per request, it must be a local variable — never a field.

🎯 Interview-ready explanation

Reusing a mutable DTO as a field causes race conditions because multiple concurrent requests can overwrite shared state. Each request must create its own request-specific objects to ensure thread safety.

🧠 One-line summary

The bug happens because two concurrent requests modify the same shared object, causing one request to send the other request’s data.
 */
