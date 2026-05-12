using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectSolution.Services.CouponAPI.Data;
using ProjectSolution.Services.CouponAPI.Extensions;
using ProjectSolution.Services.CouponAPI.Models;
using ProjectSolution.Services.CouponAPI.Models.Dtos;

namespace ProjectSolution.Services.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    [Authorize]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        public CouponAPIController(AppDbContext db)
        {
            _db = db;
            _response = new ResponseDto();
        }

        [HttpGet]
        public async Task<ResponseDto> Get()
        {
            try
            {
                //IEnumerable<Coupon> coupons = _db.Coupons.ToList();
                _response.Result = await _db.Coupons.Select(c => c.ToCouponDto()).ToListAsync();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpGet]
        [Route("{id:int}")]
        public async Task<ResponseDto> Get(int id)
        {
            try
            {
                Coupon coupon = await _db.Coupons.FirstAsync(u => u.CouponId == id);
                _response.Result = coupon.ToCouponDto();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpGet("GetByCode/{code}")]
        public async Task<ResponseDto> GetByCode(string code)
        {
            try
            {
                Coupon coupon = await _db.Coupons.FirstAsync(u => u.CouponCode.ToUpper() == code.Trim().ToUpper());
                _response.Result = coupon.ToCouponDto();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Post([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon newCoupon = couponDto.ToCoupon();
                await _db.Coupons.AddAsync(newCoupon);
                await _db.SaveChangesAsync();

                var options = new Stripe.CouponCreateOptions
                {
                    AmountOff = (long)(couponDto.DiscountAmount * 100), // Convert to paise
                    Name = couponDto.CouponCode,
                    Currency = "inr",
                    Id = couponDto.CouponCode,
                    Duration = "forever",
                };
                var service = new Stripe.CouponService();
                service.Create(options);

                _response.Result = couponDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Put([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon updatedCoupon = couponDto.ToCoupon();
                _db.Coupons.Update(updatedCoupon);
                await _db.SaveChangesAsync();
                _response.Result = couponDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Delete(int id)
        {
            try
            {
                Coupon deletedCoupon = await _db.Coupons.FirstAsync(c => c.CouponId == id);
                _db.Coupons.Remove(deletedCoupon);
                await _db.SaveChangesAsync();


                var service = new Stripe.CouponService();
                service.Delete(deletedCoupon.CouponCode);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
