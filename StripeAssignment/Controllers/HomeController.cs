namespace StripeAssignment.Controllers
{
  using System.Diagnostics;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Options;
  using Stripe;
  using StripeAssignment.Models;

  public class HomeController : Controller
  {
    private readonly Models.StripeSettings stripeSettings;
    private StripeSettings model = new StripeSettings();

    public HomeController(IOptions<StripeSettings> stripe)
    {
      this.stripeSettings = stripe.Value;
    }

    public IActionResult Index()
    {
      return this.View(new StripeSettings { PublishableKey = this.stripeSettings.PublishableKey });
    }

    public IActionResult About()
    {
      this.ViewData["Message"] = "Your application description page.";

      return this.View();
    }

    public IActionResult Contact()
    {
      this.ViewData["Message"] = "Your contact page.";

      return this.View();
    }

    public IActionResult Charge(string stripeToken, int? amount, string source = null, int? chargeamount = null)
    {
      if ((stripeToken != null && amount != null) || (source != null && chargeamount != null))
      {
        var customers = new StripeCustomerService();
        var charges = new StripeChargeService();

        StripeCharge charge = null;

        // 3d secure process
        if (source != null)
        {
          // incase 3d secure fails
          try
          {
            // charge the card
            charge = charges.Create(new StripeChargeCreateOptions
            {
              Amount = chargeamount * 100,
              Currency = "usd",
              SourceTokenOrExistingSourceId = source
            });
          }
          catch
          {
            return this.View(new StripeAssignment.Models.ChargeResponse() { Success = false, Amount = null, Secure3D = true });
          }
        }

        // normal process (no 3d secure)
        else
        {
          var customer = customers.Create(new StripeCustomerCreateOptions
          {
            SourceToken = stripeToken
          });
          charge = charges.Create(new StripeChargeCreateOptions
          {
            Amount = amount * 100,
            Description = "Sample Charge",
            Currency = "usd",
            CustomerId = customer.Id
          });
        }

        // if card charged successfully
        if (charge.Status == "succeeded")
        {
          return this.View(new StripeAssignment.Models.ChargeResponse() { Success = true, Amount = amount == null ? chargeamount : amount, Secure3D = false });
        }
        else
        {
          return this.View(new StripeAssignment.Models.ChargeResponse() { Success = false, Amount = null, Secure3D = false });
        }
      }
      else if (source != null)
      {
        return this.View(new StripeAssignment.Models.ChargeResponse() { Success = true, Amount = null, Secure3D = true });
      }
      else
      {
        return this.View(new StripeAssignment.Models.ChargeResponse() { Success = null, Amount = null });
      }
    }

    public IActionResult Error()
    {
      return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
  }
}
