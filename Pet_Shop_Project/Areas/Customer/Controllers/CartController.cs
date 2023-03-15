using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;
using Models.ViewModels;
using Pet_Shop_Project.Repository.IRepository;
using Stripe.Checkout;
using System.Security.Claims;
using Utility;

namespace Pet_Shop_Project.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emalSender;
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public int OrderTotal { get; set; }
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emalSender = emailSender;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM = new ShoppingCartVM
            {
                ListCart = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Animal"),
                orderHeader = new OrderHeader()
            };
            foreach (var cart in shoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.count, cart.animal.Price);
                shoppingCartVM.orderHeader.OrderTotal += (cart.Price * cart.count);
            }
            return View(shoppingCartVM);
        }

        public IActionResult Summery()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM = new ShoppingCartVM
            {
                ListCart = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Animal"),
                orderHeader = new OrderHeader()
            };
            shoppingCartVM.orderHeader.applicationUser = _unitOfWork.applicationUser.GetFirstOrDefault(
                u => u.Id == claim.Value);

            shoppingCartVM.orderHeader.Name = shoppingCartVM.orderHeader.applicationUser.Name;
            shoppingCartVM.orderHeader.PhoneNumber = shoppingCartVM.orderHeader.applicationUser.PhoneNumber;
            shoppingCartVM.orderHeader.StreetAdress = shoppingCartVM.orderHeader.applicationUser.StreetAdress;
            shoppingCartVM.orderHeader.City = shoppingCartVM.orderHeader.applicationUser.City;
            shoppingCartVM.orderHeader.State = shoppingCartVM.orderHeader.applicationUser.State;
            shoppingCartVM.orderHeader.PostalCode = shoppingCartVM.orderHeader.applicationUser.PostalCode;

            foreach (var cart in shoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.count, cart.animal.Price);
                shoppingCartVM.orderHeader.OrderTotal += (cart.Price * cart.count);
            }
            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summery")]
        [ValidateAntiForgeryToken]
        public IActionResult SummeryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM.ListCart = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "animal");


            shoppingCartVM.orderHeader.OrderDate = System.DateTime.Now;
            shoppingCartVM.orderHeader.ApplicationUserId = claim.Value;

            foreach (var cart in shoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.count, cart.animal.Price);
                shoppingCartVM.orderHeader.OrderTotal += (cart.Price * cart.count);
            }
            ApplicationUser applicationUser = _unitOfWork.applicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartVM.orderHeader.PatmentStatus = SD.PaymentStatusPending;
                shoppingCartVM.orderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                shoppingCartVM.orderHeader.PatmentStatus = SD.PaymentStatusDelayedPayment;
                shoppingCartVM.orderHeader.OrderStatus = SD.StatusApproved;
            }
            //var maxId = _unitOfWork.orderHeader.GetAll().Max(o => o.Id);
            //shoppingCartVM.orderHeader.Id = maxId + 1;
            _unitOfWork.orderHeader.Add(shoppingCartVM.orderHeader);
            _unitOfWork.Save();
            foreach (var cart in shoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    AnimalId = cart.AnimalId,
                    OrderId = shoppingCartVM.orderHeader.Id,
                    Count = cart.count
                };
                _unitOfWork.orderDetail.Add(orderDetail);
                //_unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Stripe Setting
                var domain = "https://localhost:7097/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.orderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                };

                foreach (var item in shoppingCartVM.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.animal.AnimalName,
                            },
                        },
                        //Price = "{{PRICE_ID}}",
                        Quantity = item.count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                shoppingCartVM.orderHeader.SessionId = session.Id;
                shoppingCartVM.orderHeader.PaymenIntentId = session.PaymentIntentId;
                _unitOfWork.orderHeader.UpdateStripePaymentId(shoppingCartVM.orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save(); // Save the update of SessionId and PaymentIntentId
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = shoppingCartVM.orderHeader.Id });
            }
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == id);
            if (orderHeader.PatmentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                // check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.orderHeader.UpdateStripePaymentId(id, orderHeader.SessionId, session.PaymentIntentId);
                    _unitOfWork.orderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            //_emalSender.SendEmailAsync(orderHeader.applicationUser.Email, "New Order = Pet Shop", "<p>New Order Created</p>");
            List<ShoppingCart> shoppingCarts = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId ==
            orderHeader.ApplicationUserId).ToList(); ;
            HttpContext.Session.Clear();
            _unitOfWork.shoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.shoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.shoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save(); // save changes to the database
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.shoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.count <= 1)
            {
                _unitOfWork.shoppingCart.Remove(cart);
                var count = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
            else
            {
                _unitOfWork.shoppingCart.DecrementCount(cart, 1);
            }
            _unitOfWork.Save(); // save changes to the database
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.shoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.shoppingCart.Remove(cart);
            _unitOfWork.Save(); // save changes to the database
            var count = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;
            HttpContext.Session.SetInt32(SD.SessionCart, count);
            return RedirectToAction(nameof(Index));
        }

        private decimal GetPriceBaseOnQuantity(decimal quantity, decimal price)
        {
            //if (quantity <= 1)
            //{
            //    return price;
            //}
            return price;
        }
    }
}