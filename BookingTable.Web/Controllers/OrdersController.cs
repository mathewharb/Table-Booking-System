using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Entities;
using BookingTable.Entities.Enum;
using BookingTable.Entities.Models;
using BookingTable.Entities.Paypal;
using BookingTable.Web.Business;
using BookingTable.Web.Helpers;
using BookingTable.Web.Security;
using Microsoft.Build.Tasks;
using Order = BookingTable.Entities.Entities.Order;

namespace BookingTable.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ITableRepository _tableRepository = new TableRepository();
        private readonly IFoodRepository _foodRepository = new FoodRepository();
        private readonly IOrdersRepository _ordersRepository = new OrdersRepository();

        // GET
        public ActionResult Booking()
        {
            if (Session["BookingEntry"] != null)
            {
                return PartialView("_Booking");
            }
            return null;
        }

        public ActionResult ViewBooking()
        {
            return View();
        }

        [UserAuthorized]
        public ActionResult SuccessPayment()
        {
            ISettingRepository settingRepository = new SettingRepository();

            var pdtToken = settingRepository.GetSettingByKey(PaypalSettingEnum.PDTToken.ToString());
            var mode = settingRepository.GetSettingByKey(PaypalSettingEnum.Mode.ToString());
            var paypalUrl = settingRepository.GetSettingByKey(mode.Value);

            var payment = PDTHolder.Success(Request.QueryString.Get("tx"), pdtToken.Value, paypalUrl.Value);
            if (payment != null)
            {
                var order = Session["Booking"] as Order;
                if (order != null && order.Id == 0)
                {
                    var subTotal = order.DepositPrice.Value + order.SubTotal;
                    var discount = order.Discount.Value / 100 * subTotal;
                    var tax = order.Tax.Value / 100 * (subTotal - discount);
                    var total = subTotal - discount + tax;
                    if (Math.Round(payment.GrossTotal) == Math.Round(double.Parse(total.ToString())))
                    {
                        var entity = new Order
                        {
                            CustomerId = order.CustomerId,
                            CreatorId = order.CreatorId,
                            LastUpdate = DateTime.Now,
                            CreationTime = DateTime.Now,
                            Name = order.Name,
                            Note = order.Note,
                            DepositPrice = total,
                            SubTotal = order.SubTotal,
                            Tax = order.Tax,
                            Discount = order.Discount,
                            OrderDetails = new List<OrderDetail>(),
                        };
                        foreach (var item in order.OrderDetails)
                        {
                            entity.OrderDetails.Add(new OrderDetail
                            {
                                CreatorId = item.CreatorId,
                                TableId = item.TableId,
                                FoodId = item.FoodId,
                                FoodPrice = item.FoodPrice,
                                Quantity = item.Quantity,
                                Subtotal = item.Subtotal,
                                CreationTime = DateTime.Now,
                                OrderTime = item.OrderTime,
                                LastUpdate = DateTime.Now,
                            });
                        }
                        entity.Note += "\nPayment info:\n\tFull Name: " + payment.PayerFirstName + " " +
                                       payment.PayerLastName + "\n\tEmail: " + payment.PayerEmail + "\n\tGross total: " +
                                       payment.GrossTotal.ToString("c", new CultureInfo("en-us"));
                        if (!_ordersRepository.Save(entity))
                        {
                            return RedirectToAction("Error", "Home");
                        }
                        order.Id = entity.Id;
                        Session.Remove("Booking");
                        Session.Remove("BookingEntry");
                    }
                }
                ViewBag.Payment = payment;
            }
            return View();
        }

        public ActionResult DeleteTable(int id)
        {
            return PartialView("_DeleteTable", _tableRepository.Find(id));
        }
        public ActionResult DeleteFood(int id)
        {
            return PartialView("_DeleteFood", _foodRepository.Find(id));
        }

        public ActionResult ClearBooking()
        {
            return PartialView("_ClearBooking");
        }
        //POST
        [HttpPost]
        public ActionResult Search(SearchModel model)
        {
            if (string.IsNullOrEmpty(model.Date) || string.IsNullOrEmpty(model.Time))
            {
                RedirectToAction("Index", "Home");
            }
            var time = DateTime.ParseExact(model.Date + " " + model.Time, "MM/dd/yyyy hh:mm tt",
                CultureInfo.InvariantCulture);

            if (DateTime.Today.AddDays(int.Parse(WebSetting.GetBookingLimit())) < time)
            {
                RedirectToAction("Index", "Home");
            }

            var data = _tableRepository.GetActivedTablesByFloorAndType(model.FloorId, model.TypeId);

            var tableSelectModels = new TableBll().ConvertTableToTableModelByTime(data, time);
            ViewBag.Floor = model.FloorId;
            ViewBag.Type = model.TypeId;
            ViewBag.Time = time;
            return View(tableSelectModels);

        }

        [HttpPost]
        public int BookingTable(BookingTableModel model)
        {
            try
            {
                ITableRepository tableRepository = new TableRepository();
                if (model != null)
                {
                    var cart = Session["BookingEntry"] as Order;
                    var table = tableRepository.Find(model.TableId);
                    var orderTime = DateTime.ParseExact(model.Time, "MM/dd/yyyy hh:mm tt",
                        CultureInfo.InvariantCulture);

                    if (table != null)
                    {

                        if (cart == null)
                        {
                            cart = new Order
                            {
                                DepositPrice = table.TableType.DepositPrice,
                                OrderDetails = new List<OrderDetail>
                                {
                                    new OrderDetail
                                    {
                                        TableId = table.Id,
                                        Table = table,
                                        OrderTime = orderTime
                                    }
                                }
                            };
                        }
                        else
                        {
                            if (!cart.OrderDetails.Any(x => x.TableId == model.TableId && x.OrderTime.Value.Date == orderTime.Date && x.OrderTime.Value.Hour == orderTime.Hour))
                            {
                                cart.OrderDetails.Add(new OrderDetail { TableId = table.Id, Table = table, OrderTime = orderTime });
                                cart.DepositPrice += table.TableType.DepositPrice;
                            }
                            else
                            {
                                cart.OrderDetails.Remove(cart.OrderDetails.First(x => x.TableId == model.TableId));
                                cart.DepositPrice -= table.TableType.DepositPrice;
                            }
                        }
                        if (cart.OrderDetails != null)
                        {
                            for (var i = 0; i < cart.OrderDetails.Count; i++)
                            {
                                cart.OrderDetails.ElementAt(i).Id = i+1;
                            }
                        }
                        Session["BookingEntry"] = cart;
                        return cart.OrderDetails.Count;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return 0;

        }

        [HttpPost]
        public ActionResult SubmitBooking(Order model)
        {
            if (DateTime.Today.AddDays(int.Parse(WebSetting.GetBookingLimit())) < model.OrderTime)
            {
                RedirectToAction("Index", "Home");
            }

            ISettingRepository settingRepository = new SettingRepository();
            var discount = decimal.Parse(settingRepository.GetSettingByKey(SystemSettingEnum.Discount.ToString()).Value);
            var tax = decimal.Parse(settingRepository.GetSettingByKey(SystemSettingEnum.Tax.ToString()).Value);
            var order = new Order
            {
                LastUpdate = DateTime.Now,
                CreationTime = DateTime.Now,
                Name = Resources.Resources.Content_Booking,
                Note = model.Note,
                DepositPrice = 0,
                Discount = discount,
                Tax = tax,
                OrderDetails = new List<OrderDetail>(),
            };

            var tableBll = new TableBll();
            foreach (var item in model.OrderDetails)
            {
                if (tableBll.GetOrderByTableIdAndTime(item.TableId, item.OrderTime.Value) != null)
                {
                    Session.Remove("BookingEntry");
                    RedirectToAction("Index", "Home");
                }

                var table = _tableRepository.Find(item.TableId);

                if (table == null || table.Active != true)
                    return RedirectToAction("Error", "Home");

                if (!item.FoodId.HasValue)
                {
                    order.OrderDetails.Add(new OrderDetail
                    {
                        CreationTime = DateTime.Now,
                        TableId = item.TableId,
                        Table = table,
                        OrderTime = item.OrderTime
                    });
                    order.DepositPrice += table.TableType.DepositPrice;
                }
                else
                {

                    var food = _foodRepository.Find(item.FoodId.Value);

                    if (!item.Quantity.HasValue || !(item.Quantity > 0) || food.Quantity < item.Quantity.Value)
                        return RedirectToAction("Error", "Home");

                    var sameOrderdetails = order.OrderDetails.FirstOrDefault(x => x.FoodId == food.Id && x.TableId == item.TableId);
                    if (sameOrderdetails == null)
                    {
                        order.OrderDetails.Add(new OrderDetail
                        {
                            TableId = item.TableId,
                            Table = table,
                            Food = food,
                            FoodId = food.Id,
                            FoodPrice = food.Price,
                            Quantity = item.Quantity,
                            Subtotal = food.Price * item.Quantity.Value,
                            CreationTime = DateTime.Now,
                            LastUpdate = DateTime.Now
                        });
                    }
                    else
                    {
                        sameOrderdetails.Quantity++;
                    }
                    order.SubTotal += food.Price * item.Quantity.Value;

                }
            }
            Session["Booking"] = order;

            return RedirectToAction("ViewBooking");
        }


        [HttpPost]
        public ActionResult DeleteFood(Food food)
        {
            var order = Session["Booking"] as Order;

            foreach(var item in order.OrderDetails.Where(x => x.FoodId == food.Id).ToList())
            {
                order.OrderDetails.Remove(item);
            }
            order.DepositPrice = 0;
            order.SubTotal = 0;
            foreach (var item in order.OrderDetails)
            {
                if (!item.FoodId.HasValue)
                {
                    order.DepositPrice += item.Table.TableType.DepositPrice;
                }
                else
                {
                    order.SubTotal += item.Food.Price * item.Quantity.Value;
                }
            }
            Session["Booking"] = order;
            return RedirectToAction("ViewBooking");
        }
        [HttpPost]
        public ActionResult DeleteTable(Table table)
        {
            var tables = Session["BookingEntry"] as Order;
            foreach (var item in tables.OrderDetails.Where(x => x.TableId == table.Id).ToList())
            {
                tables.OrderDetails.Remove(item);
            }
            Session["BookingEntry"] = tables;
            var order = Session["Booking"] as Order;

            foreach (var item in order.OrderDetails.Where(x => x.TableId == table.Id).ToList())
            {
                order.OrderDetails.Remove(item);
            }
            order.DepositPrice = 0;
            order.SubTotal = 0;
            foreach (var item in order.OrderDetails)
            {
                if (!item.FoodId.HasValue)
                {
                    order.DepositPrice += item.Table.TableType.DepositPrice;
                }
                else
                {
                    order.SubTotal += item.Food.Price * item.Quantity.Value;
                }
            }
            if (order.OrderDetails.Count == 0)
            {
                Session.Remove("Booking");
                return RedirectToAction("Index","Home");
            }
                Session["Booking"] = order;
            
            return RedirectToAction("ViewBooking");
        }
        [HttpDelete]
        public ActionResult ClearAndRebook()
        {
            Session.Remove("Booking");
            Session.Remove("BookingEntry");

            return RedirectToAction("Index", "Home");
        }
    }
}