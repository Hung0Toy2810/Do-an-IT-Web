using Backend.Model.Entity;
using System.Threading.Tasks;
using Backend.Model.Nosql.ViettelPost;

namespace Backend.Service.ViettelPost
{
    public interface IViettelPostAddressValidatorService
    {
        Task<AddressValidationResult> ValidateShippingAddressAsync(ShippingAddress address);
    }
}