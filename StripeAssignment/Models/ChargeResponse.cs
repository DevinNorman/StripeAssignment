namespace StripeAssignment.Models
{
  public class ChargeResponse
  {
    public bool? Success { get; set; }

    public int? Amount { get; set; }

    public bool Secure3D { get; set; }
  }
}
