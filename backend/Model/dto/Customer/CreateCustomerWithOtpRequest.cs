namespace Backend.Model.dto.Customer
{
    public class CreateCustomerWithOtpRequest
    {
        public CreateCustomer CreateCustomer { get; set; } = new CreateCustomer();
        public string Otp { get; set; } = string.Empty;
    }
}