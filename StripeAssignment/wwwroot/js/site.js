// Write your JavaScript code.

// Create a Stripe client.
var stripe = Stripe(window.stripeToken);

// Create an instance of Elements.
var elements = stripe.elements();

// Custom styling can be passed to options when creating an Element.
// (Note that this demo uses a wider set of styles than the guide below.)
var style = {
  base: {
    color: '#32325d',
    lineHeight: '18px',
    fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
    fontSmoothing: 'antialiased',
    fontSize: '16px',
    '::placeholder': {
      color: '#aab7c4'
    }
  },
  invalid: {
    color: '#fa755a',
    iconColor: '#fa755a'
  }
};

// Create an instance of the card Element.
var card = elements.create('card', { style: style });

// Add an instance of the card Element into the `card-element` <div>.
card.mount('#card-element');

// Handle real-time validation errors from the card Element.
card.addEventListener('change', function (event) {
  var displayError = document.getElementById('card-errors');
  if (event.error) {
    displayError.textContent = event.error.message;
  } else {
    displayError.textContent = '';
  }
});

// Handle form submission.
var form = document.getElementById('payment-form');
form.addEventListener('submit', function (event) {
  event.preventDefault();

  stripe.createSource(card).then(function (result) {
    if (result.error) {  // problem
      // Inform the user if there was an error.
      var errorElement = document.getElementById('card-errors');
      errorElement.textContent = result.error.message;
    } else if (result.source.status === 'chargeable' && (result.source.card.three_d_secure === 'not_supported' || result.source.card.three_d_secure === 'optional')) {
      // Send the token to your server.
      stripeTokenHandler(result.source);
    } else if (result.source.card.three_d_secure === 'required' || result.source.card.three_d_secure === 'recommended') {
      stripe.createSource({
        type: 'three_d_secure',
        amount: parseFloat($('input#charge-amount')[0].value) * 100,
        currency: "usd",
        three_d_secure: {
          card: result.source.id
        },
        redirect: {
          return_url: window.location.origin + '/home/charge?chargeamount=' + $('input#charge-amount')[0].value
        }
      }).then(function (result) {
        window.location = result.source.redirect.url;
      });
    } else {
      alert('error');
    }
  });
});

function stripeTokenHandler(source) {
  // Insert the token ID into the form so it gets submitted to the server
  var form = document.getElementById('payment-form');
  var hiddenInput = document.createElement('input');
  hiddenInput.setAttribute('type', 'hidden');
  hiddenInput.setAttribute('name', 'stripeToken');
  hiddenInput.setAttribute('value', source.id);
  form.appendChild(hiddenInput);

  // Submit the form
  form.submit();
}