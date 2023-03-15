using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Pet_Shop_Project.Repository.IRepository;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;
using Utility;

namespace Pet_Shop_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Index(string status)
        {
            var orders = _unitOfWork.orderHeader.GetAll();

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orders = _unitOfWork.orderDetail.GetAll().Select(o => o.orderHeader).Distinct();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orders = _unitOfWork.orderHeader.GetAll(U => U.ApplicationUserId == claim.Value);
            }
            switch (status)
            {
                case "pending":
                    orders = orders.Where(u => u.PatmentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orders = orders.Where(u => u.OrderStatus == SD.PaymentStatusApproved);
                    break;
                default:
                    break;
            }

            var orderVM = new OrderVM()
            {
                orderHeader = orders.ToList()
            };
            return View(orderVM);
        }

        public IActionResult Details(int orderId)
        {
            var orderHeader = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderId);
            var orderDetail = _unitOfWork.orderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "animal");

            var orderVM = new OrderVM()
            {
                orderHeaderSingle = orderHeader,
                orderDetail = orderDetail
            };
            return View(orderVM);
        }

        //[ActionName("Details")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Details_PAY_NOW()
        //{
        //    orderVM.orderHeaderSingle = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeaderSingle.Id);
        //    orderVM.orderDetail = _unitOfWork.orderDetail.GetAll(u => u.OrderId == orderVM.orderHeaderSingle.Id, includeProperties: "animal");

        //    // Stripe Setting
        //    var domain = "https://localhost:7097/";
        //    var options = new SessionCreateOptions
        //    {
        //        PaymentMethodTypes = new List<string>
        //        {
        //            "card"
        //        },
        //        LineItems = new List<SessionLineItemOptions>(),
        //        Mode = "payment",
        //        SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={orderVM.orderHeaderSingle.Id}",
        //        CancelUrl = domain + $"admin/order/details?orderid={orderVM.orderHeaderSingle.Id}",
        //    };

        //    foreach (var item in orderVM.orderDetail)
        //    {
        //        var sessionLineItem = new SessionLineItemOptions
        //        {
        //            PriceData = new SessionLineItemPriceDataOptions
        //            {
        //                UnitAmount = (long)(item.Price + 100),
        //                Currency = "usd",
        //                ProductData = new SessionLineItemPriceDataProductDataOptions
        //                {
        //                    Name = item.animal.AnimalName,
        //                },
        //            },
        //            Quantity = item.Count,
        //        };
        //        options.LineItems.Add(sessionLineItem);
        //    }

        //    var service = new SessionService();
        //    Session session = service.Create(options);
        //    //shoppingCartVM.orderHeader.SessionId = session.Id;
        //    //shoppingCartVM.orderHeader.PaymenIntentId = session.PaymentIntentId;
        //    _unitOfWork.orderHeader.UpdateStripePaymentId(orderVM.orderHeaderSingle.Id, session.Id, session.PaymentIntentId);
        //    _unitOfWork.Save(); // Save the update of SessionId and PaymentIntentId
        //    Response.Headers.Add("Location", session.Url);
        //    return new StatusCodeResult(303);
        //}

        //public IActionResult PaymentConfirmation(int orderHeaderid)
        //{
        //    OrderHeader orderHeader = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);
        //    if (orderHeader.PatmentStatus == SD.PaymentStatusDelayedPayment)
        //    {
        //        var service = new SessionService();
        //        Session session = service.Get(orderHeader.SessionId);
        //        // check the stripe status
        //        if (session.PaymentStatus.ToLower() == "paid")
        //        {
        //            //_unitOfWork.orderHeader.UpdateStripePaymentId(orderHeaderid, orderHeader.SessionId, session.PaymentIntentId);
        //            _unitOfWork.orderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
        //            _unitOfWork.Save();
        //        }
        //    }
        //    return View(orderHeaderid);
        //}

        //[HttpPost]
        ////[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateOrderDetails(int orderid)
        //{
        //    var orderHeaderFromDb = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeaderSingle.Id, tracked: false);
        //    orderHeaderFromDb.Name = orderVM.orderHeaderSingle.Name;
        //    orderHeaderFromDb.PhoneNumber = orderVM.orderHeaderSingle.PhoneNumber;
        //    orderHeaderFromDb.StreetAdress = orderVM.orderHeaderSingle.StreetAdress;
        //    orderHeaderFromDb.City = orderVM.orderHeaderSingle.City;
        //    orderHeaderFromDb.State = orderVM.orderHeaderSingle.State;
        //    orderHeaderFromDb.PostalCode = orderVM.orderHeaderSingle.PostalCode;
        //    if (orderVM.orderHeaderSingle.Carrier != null)
        //    {
        //        orderHeaderFromDb.Carrier = orderVM.orderHeaderSingle.Carrier;
        //    }
        //    if (orderVM.orderHeaderSingle.TrackingNumber != null)
        //    {
        //        orderHeaderFromDb.TrackingNumber = orderVM.orderHeaderSingle.TrackingNumber;
        //    }
        //    _unitOfWork.orderHeader.Update(orderHeaderFromDb);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Order Details Updated Successfully.";
        //    return RedirectToAction("Details", "Order", new { orderid = orderHeaderFromDb.Id });
        //}

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_PAY_NOW()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            orderVM.orderHeaderSingle = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeaderSingle.Id);
            orderVM.orderDetail = _unitOfWork.orderDetail.GetAll(u => u.OrderId == orderVM.orderHeaderSingle.Id, includeProperties: "animal");


            foreach (var cart in orderVM.orderDetail)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.animal.Price);
                orderVM.orderHeaderSingle.OrderTotal += (cart.Price * cart.Count);
            }
            ApplicationUser applicationUser = _unitOfWork.applicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                orderVM.orderHeaderSingle.PatmentStatus = SD.PaymentStatusPending;
                orderVM.orderHeaderSingle.OrderStatus = SD.StatusPending;
            }
            else
            {
                orderVM.orderHeaderSingle.PatmentStatus = SD.PaymentStatusDelayedPayment;
                orderVM.orderHeaderSingle.OrderStatus = SD.StatusApproved;
            }

            _unitOfWork.orderHeader.Add(orderVM.orderHeaderSingle);
            _unitOfWork.Save();
            foreach (var cart in orderVM.orderDetail)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    AnimalId = cart.AnimalId,
                    OrderId = shoppingCartVM.orderHeader.Id,
                    Count = cart.Count
                };
                _unitOfWork.orderDetail.Add(orderDetail);
                //_unitOfWork.Save();
            }

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
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={orderVM.orderHeaderSingle.Id}",
                CancelUrl = domain + $"admin/order/details?orderid={orderVM.orderHeaderSingle.Id}",
            };

            foreach (var item in orderVM.orderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price + 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.animal.AnimalName,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            //shoppingCartVM.orderHeader.SessionId = session.Id;
            //shoppingCartVM.orderHeader.PaymenIntentId = session.PaymentIntentId;
            _unitOfWork.orderHeader.UpdateStripePaymentId(orderVM.orderHeaderSingle.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save(); // Save the update of SessionId and PaymentIntentId
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            OrderHeader orderHeader = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);
            if (orderHeader.PatmentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                // check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    //_unitOfWork.orderHeader.UpdateStripePaymentId(orderHeaderid, orderHeader.SessionId, session.PaymentIntentId);
                    _unitOfWork.orderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            return View(orderHeaderid);
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails(int orderid)
        {
            var orderHeaderFromDb = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeaderSingle.Id, tracked: false);
            orderHeaderFromDb.Name = orderVM.orderHeaderSingle.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.orderHeaderSingle.PhoneNumber;
            orderHeaderFromDb.StreetAdress = orderVM.orderHeaderSingle.StreetAdress;
            orderHeaderFromDb.City = orderVM.orderHeaderSingle.City;
            orderHeaderFromDb.State = orderVM.orderHeaderSingle.State;
            orderHeaderFromDb.PostalCode = orderVM.orderHeaderSingle.PostalCode;
            if (orderVM.orderHeaderSingle.Carrier != null)
            {
                orderHeaderFromDb.Carrier = orderVM.orderHeaderSingle.Carrier;
            }
            if (orderVM.orderHeaderSingle.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = orderVM.orderHeaderSingle.TrackingNumber;
            }
            _unitOfWork.orderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderid = orderHeaderFromDb.Id });
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.orderHeader.UpdateStatus(orderVM.orderHeaderSingle.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderid = orderVM.orderHeaderSingle.Id });
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeaderSingle.Id, tracked: false);
            orderHeader.TrackingNumber = orderVM.orderHeaderSingle.TrackingNumber;
            orderHeader.Carrier = orderVM.orderHeaderSingle.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PatmentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PatmentDueDate = DateTime.Now.AddDays(30);
            }
            _unitOfWork.orderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order Shipped Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderid = orderVM.orderHeaderSingle.Id });
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.orderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeaderSingle.Id, tracked: false);
            if (orderHeader.PatmentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymenIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.orderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.orderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled);
            }
            _unitOfWork.Save();

            TempData["success"] = "Order Cancelled Successfully.";
            return RedirectToAction("Details", "Order", new { orderid = orderVM.orderHeaderSingle.Id });
        }

        // includeProperties: "ApplicationUser, Animal"
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeader;
            orderHeader = _unitOfWork.orderHeader.GetAll();

            return Json(new { data = orderHeader });
        }
        #endregion

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