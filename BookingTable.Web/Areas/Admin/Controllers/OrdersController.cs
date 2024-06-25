using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Entities;
using BookingTable.Entities.Enum;
using BookingTable.Entities.Models;
using BookingTable.Web.Business;
using BookingTable.Web.Helpers;
using BookingTable.Web.Security;

namespace BookingTable.Web.Areas.Admin.Controllers
{
    [AdminAuthorized]
    [Permission("Orders")]
    public class OrdersController : Controller
    {
        private readonly IOrdersRepository _ordersRepository = new OrdersRepository();
        private readonly IFoodRepository _foodRepository = new FoodRepository();
        private readonly ITableRepository _tableRepository = new TableRepository();

        //GET
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BookingList()
        {
            var data = _ordersRepository.GetOrders();
            return View("BookingList", data.Where(x => x.Completed == null || x.Completed.Value != true && x.Customer != null));
        }

        public ActionResult OrdersOfTable(int tableId)
        {
            var data = _ordersRepository.GetOrders();
            ViewBag.TableName = _tableRepository.Find(tableId).Name;
            return View("OrdersOfTable", data.Where(x=>x.OrderDetails.Any(y=>y.TableId == tableId)));
        }

        public ActionResult OrderFood(string tableString, string timeString)
        {
            if (!string.IsNullOrEmpty(tableString) && !string.IsNullOrEmpty(timeString))
            {
                var data = new OrderModel
                {
                    TableString = tableString,
                    TimeString = timeString,
                    FoodList = _foodRepository.GetValidFoods()
                };

                return PartialView("_OrderFood", data);
            }

            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Error_OrderFoodGet,
                Title = Resources.Resources.Content_Error,
                Type = MessageTypeEnum.Error.ToString()
            };

            return Json(message, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ViewOrders(int tableId)
        {
            var distanceTime = WebSetting.GetTimeDistanceSetting();

            //Get order details
            var orders =
                _tableRepository.Find(tableId)
                    .OrderDetails.OrderBy(x => x.Order.OrderTime)
                    .GroupBy(x => x.Order)
                    .ToList();


            var currentOrderIndex = 0;
            for (int i = 0; i < orders.Count; i++)
            {
                if (orders.ElementAt(i).Key.Completed != true)
                {
                    currentOrderIndex = i;
                    break;
                }
            }

            //Current order in center
            ViewBag.CurrentOrders = currentOrderIndex;
            ViewBag.TableId = tableId;
            if (orders.Count <= 10)
                return PartialView("_ViewOrder", orders.Select(x => x.Key).ToList());

            if (currentOrderIndex > 5)
                return PartialView("_ViewOrder", orders.Select(x => x.Key).Skip(currentOrderIndex - 5).Take(10).ToList());

            return PartialView("_ViewOrder", orders.Select(x => x.Key).Take(10).ToList());
        }

        public ActionResult UpdateOrder(int orderId)
        {
            var entity = _ordersRepository.Find(orderId);
            entity.OrderDetails = entity.OrderDetails.OrderBy(x => x.TableId).ToList();
            if (entity.Completed != true)
            {
                return View(entity);
            }

            return RedirectToAction("Error", "Home");
        }

        public ActionResult Details(int orderId)
        {
            ISettingRepository settingRepository = new SettingRepository();
            var tax = settingRepository.GetSettingByKey(SystemSettingEnum.Tax.ToString());
            var discount = settingRepository.GetSettingByKey(SystemSettingEnum.Discount.ToString());
            var entity = _ordersRepository.Find(orderId);
            entity.Discount = decimal.Parse(discount.Value)*entity.SubTotal/100;
            entity.Tax = decimal.Parse(tax.Value)*(entity.SubTotal - entity.Discount.Value)/100;
            _ordersRepository.Save(entity);
            return View(entity);

        }

        public ActionResult PrintInvoice(int orderId)
        {
            var entity = _ordersRepository.Find(orderId);
            if (entity.Completed != true)
            {
                //Pay
                foreach (var item in entity.OrderDetails)
                {
                    item.Completed = true;
                }
                entity.Completed = true;
                entity.PaymentTime = DateTime.Now;
                entity.PayeeId = Support.GetCurrentAdmin().Id;
                _ordersRepository.Save(entity);
            }
            return View(entity);
        }

        public ActionResult Pay(int orderId)
        {
            var entity = _ordersRepository.Find(orderId);

            //Pay
            foreach (var item in entity.OrderDetails)
            {
                item.Completed = true;
            }
            entity.Completed = true;
            entity.PaymentTime = DateTime.Now;
            entity.PayeeId = Support.GetCurrentAdmin().Id;
            _ordersRepository.Save(entity);

            return RedirectToAction("Index", "Orders");
        }


        //POST
        [HttpPost]
        public ActionResult OrderFood(OrderModel orderModel)
        {
            try
            {
                var message = new MessageModel
                {
                    Content = Resources.Resources.Message_Success_Insert,
                    Title = Resources.Resources.Content_Success,
                    Type = MessageTypeEnum.Success.ToString(),
                    ClosePopup = true
                };

                //Check table, time, food
                if (string.IsNullOrEmpty(orderModel.TableString) || String.IsNullOrEmpty(orderModel.TimeString) ||
                    orderModel.FoodList == null || orderModel.FoodList.Count < 1)
                {
                    message = new MessageModel
                    {
                        Content = Resources.Resources.Message_Error_Validate,
                        Title = Resources.Resources.Content_Error,
                        Type = MessageTypeEnum.Error.ToString()
                    };
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                var time = DateTime.ParseExact(orderModel.TimeString,"MM/dd/yyyy hh:mm tt",CultureInfo.InvariantCulture);


                var adminId = Support.GetCurrentAdmin().Id;

                var tableBll = new TableBll();

                //new order
                var newOrder = new Order
                {
                    Name = Resources.Resources.Content_Orders,
                    CreatorId = adminId,
                    CreationTime = DateTime.Now,
                    OrderTime = time,
                    OrderDetails = new List<OrderDetail>(),
                    SubTotal = 0,
                    Completed = false,
                    LastUpdate = DateTime.Now,
                    UpdateByAdminId = adminId,
                    DepositPrice = 0,
                    Tax = 0,
                };

                //Get List Table Id
                var listTableId = new List<int>();

                var listTableIdString = orderModel.TableString.Split(',').ToList();
                listTableIdString.RemoveAt(listTableIdString.Count - 1);
                listTableId.AddRange(listTableIdString.Select(x => Int32.Parse(x)));
                var numOfTables = listTableId.Count;

                //Add Orderdetails
                foreach (var f in orderModel.FoodList)
                {
                    //Food
                    var food = _foodRepository.Find(f.Id);

                    if (food == null)
                    {
                        message = new MessageModel
                        {
                            Content = Resources.Resources.Message_Error_Food_Null,
                            Title = Resources.Resources.Content_Error,
                            Type = MessageTypeEnum.Error.ToString()
                        };
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                    var quantity = f.Quantity / numOfTables;

                    var subtotal = food.Price * quantity;

                    //Check food enough for all tables
                    if (quantity < 1 || f.Quantity % numOfTables != 0 || food.Quantity < f.Quantity)
                    {
                        message = new MessageModel
                        {
                            Content = Resources.Resources.Message_Error_Food_Quantity,
                            Title = Resources.Resources.Content_Error,
                            Type = MessageTypeEnum.Error.ToString()
                        };
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }

                    foreach (var tableId in listTableId)
                    {
                        //Get Old order
                        var oldOrder = tableBll.GetOrderByTableIdAndTime(tableId, time);

                        if (oldOrder == null)
                        {
                            //Add to new order
                            var orderDetails = new OrderDetail
                            {
                                FoodId = food.Id,
                                FoodPrice = food.Price,
                                Quantity = quantity,
                                Subtotal = subtotal,
                                TableId = tableId,
                                CreatorId = adminId,
                                CreationTime = DateTime.Now,
                                Completed = false,
                                LastUpdate = DateTime.Now,
                                UpdateByAdminId = adminId,
                                OrderTime = time
                            };
                            newOrder.OrderDetails.Add(orderDetails);
                            newOrder.SubTotal += subtotal;
                        }
                        else
                        {
                            //Add to old order
                            var orderDetails = new OrderDetail
                            {
                                OrdersId = oldOrder.Id,
                                FoodId = food.Id,
                                FoodPrice = food.Price,
                                Quantity = quantity,
                                Subtotal = subtotal,
                                TableId = tableId,
                                CreatorId = adminId,
                                CreationTime = DateTime.Now,
                                Completed = false,
                                LastUpdate = DateTime.Now,
                                UpdateByAdminId = adminId,
                                OrderTime = time
                            };

                            if (!_ordersRepository.AddOrderDetais(orderDetails))
                            {
                                message = new MessageModel
                                {
                                    Content = Resources.Resources.Message_Error_System,
                                    Title = Resources.Resources.Content_Error,
                                    Type = MessageTypeEnum.Error.ToString()
                                };
                                return Json(message, JsonRequestBehavior.AllowGet);
                            }

                            food.Quantity -= quantity;

                            oldOrder.SubTotal += subtotal;
                            _ordersRepository.Save(oldOrder);
                        }
                    }
                    _foodRepository.Save(food);
                }
                //Check new order exist order details
                if (newOrder.OrderDetails != null && newOrder.OrderDetails.Count > 0)
                {
                    if (!_ordersRepository.Save(newOrder))
                    {
                        message = new MessageModel
                        {
                            Content = Resources.Resources.Message_Error_System,
                            Title = Resources.Resources.Content_Error,
                            Type = MessageTypeEnum.Error.ToString()
                        };
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                    foreach (var item in newOrder.OrderDetails)
                    {
                        var food = _foodRepository.Find(item.FoodId.Value);
                        food.Quantity -= item.Quantity.Value;
                        _foodRepository.Save(food);
                    }
                }

                return Json(message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                var message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_Validate,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateOrder(OrderModel orderModel)
        {
            if (orderModel.Orderdetails == null)
            {
                try
                {
                    var message = new MessageModel
                    {
                        Content = Resources.Resources.Message_Success_Insert,
                        Title = Resources.Resources.Content_Success,
                        Type = MessageTypeEnum.SuccessReload.ToString(),
                        ClosePopup = true
                    };

                    //Check table, time, food
                    if (orderModel.OrderId < 1 ||
                        orderModel.FoodList == null ||
                        orderModel.FoodList.Count < 1)
                    {
                        message = new MessageModel
                        {
                            Content = Resources.Resources.Message_Error_Validate,
                            Title = Resources.Resources.Content_Error,
                            Type = MessageTypeEnum.ErrorReload.ToString()
                        };
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }

                    var adminId = Support.GetCurrentAdmin().Id;

                    var order = _ordersRepository.Find(orderModel.OrderId);

                    if (order == null)
                    {
                        message = new MessageModel
                        {
                            Content = Resources.Resources.Message_Error_Order_Null,
                            Title = Resources.Resources.Content_Error,
                            Type = MessageTypeEnum.ErrorReload.ToString()
                        };
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }

                    if (order.Completed == true)
                    {
                        message = new MessageModel
                        {
                            Content = Resources.Resources.Message_Error_Order_Completed,
                            Title = Resources.Resources.Content_Error,
                            Type = MessageTypeEnum.ErrorReload.ToString()
                        };
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(orderModel.TableString))
                    {
                        if (!_ordersRepository.Delete(order))
                        {
                            message = new MessageModel
                            {
                                Content = Resources.Resources.Message_Error_System,
                                Title = Resources.Resources.Content_Error,
                                Type = MessageTypeEnum.ErrorReload.ToString()
                            };
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                    }

                    //Get List Table Id
                    var listTableId = new List<int>();
                    var listTableIdString = orderModel.TableString.Split(',').ToList();
                    listTableIdString.RemoveAt(listTableIdString.Count - 1);
                    listTableId.AddRange(listTableIdString.Select(x => Int32.Parse(x)));

                    foreach (var newDetails in orderModel.FoodList)
                    {
                        var oldDetails = order.OrderDetails.FirstOrDefault(x => x.Id == newDetails.Id);

                        if (oldDetails == null)
                        {
                            message = new MessageModel
                            {
                                Content = Resources.Resources.Message_Error_UpdateOrder,
                                Title = Resources.Resources.Content_Error,
                                Type = MessageTypeEnum.ErrorReload.ToString()
                            };
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }


                        if (oldDetails.Food != null)
                        {
                            var food = _foodRepository.Find(oldDetails.FoodId.Value);
                            var change = 0;
                            //old quantity > new quantity --
                            if (oldDetails.Quantity > newDetails.Quantity)
                            {
                                if (newDetails.Quantity < 1)
                                {
                                    _ordersRepository.DeleteOrderDetailsById(oldDetails.Id);
                                    order.OrderDetails.Remove(oldDetails);
                                }
                                else
                                {
                                    change = oldDetails.Quantity.Value - newDetails.Quantity;

                                    oldDetails.Quantity = newDetails.Quantity;

                                    oldDetails.Food.Quantity += change;

                                    oldDetails.Subtotal -= change * oldDetails.FoodPrice;
                                    oldDetails.Order.SubTotal -= change * oldDetails.FoodPrice.Value;
                                    oldDetails.UpdateByAdminId = adminId;
                                    oldDetails.LastUpdate = DateTime.Now;

                                    food.Quantity += change;
                                }
                            } //new quantity > old quantity ++
                            else if (oldDetails.Quantity < newDetails.Quantity)
                            {
                                change = newDetails.Quantity - oldDetails.Quantity.Value;

                                if (oldDetails.Food.Quantity >= change)
                                {
                                    oldDetails.Quantity = newDetails.Quantity;

                                    oldDetails.Food.Quantity -= change;

                                    oldDetails.Subtotal += change * oldDetails.Food.Price;
                                    oldDetails.Order.SubTotal += change * oldDetails.Food.Price;
                                    oldDetails.UpdateByAdminId = adminId;
                                    oldDetails.LastUpdate = DateTime.Now;

                                    food.Quantity -= change;
                                }
                            }
                            _foodRepository.Save(food);
                        }
                    }

                    if (order.OrderDetails != null && order.OrderDetails.Count > 0)
                    {
                        var tableOfOrder = order.OrderDetails.GroupBy(x => x.Table).OrderBy(x => x.Key.Id).ToList();
                        var numOfTables = listTableId.Count;

                        //Update table
                        if (numOfTables == tableOfOrder.Count)
                        {
                            var tableBll = new TableBll();

                            listTableId = listTableId.OrderBy(x => x).ToList();
                            var newTableId = 0;
                            var oldTableId = 0;
                            for (var i = 0; i < numOfTables; i++)
                            {
                                newTableId = listTableId.ElementAt(i);
                                oldTableId = tableOfOrder.ElementAt(i).Key.Id;
                                if (oldTableId != newTableId)
                                {
                                    var table1 = tableBll.GetOrderByTableIdAndTime(newTableId, order.OrderTime.Value);
                                    var table2 = tableBll.GetOrderByTableIdAndTime(newTableId, order.OrderTime.Value.AddHours(1));
                                    if (table1 == null && table2 == null)
                                    {
                                        foreach (var item in order.OrderDetails.Where(x => x.TableId == oldTableId))
                                        {
                                            item.TableId = newTableId;
                                        }
                                    }
                                    else
                                    {
                                        message = new MessageModel
                                        {
                                            Content = Resources.Resources.Message_Error_ChangeTable,
                                            Title = Resources.Resources.Content_Error,
                                            Type = MessageTypeEnum.ErrorReload.ToString()
                                        };
                                        return Json(message, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }
                        else
                        {
                            message = new MessageModel
                            {
                                Content = Resources.Resources.Message_Error_ChangeTable,
                                Title = Resources.Resources.Content_Error,
                                Type = MessageTypeEnum.ErrorReload.ToString()
                            };
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }

                        //Update order
                        order.SubTotal = order.OrderDetails.Sum(x => x.Quantity.Value * x.FoodPrice.Value);
                        order.UpdateByAdminId = adminId;
                        order.LastUpdate = DateTime.Now;

                        if (!_ordersRepository.Save(order))
                        {
                            message = new MessageModel
                            {
                                Content = Resources.Resources.Message_Error_System,
                                Title = Resources.Resources.Content_Error,
                                Type = MessageTypeEnum.ErrorReload.ToString()
                            };
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (!_ordersRepository.Delete(order))
                        {
                            message = new MessageModel
                            {
                                Content = Resources.Resources.Message_Error_System,
                                Title = Resources.Resources.Content_Error,
                                Type = MessageTypeEnum.ErrorReload.ToString()
                            };
                            return Json(message, JsonRequestBehavior.AllowGet);
                        }

                    }
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {

                    var message = new MessageModel
                    {
                        Content = Resources.Resources.Message_Error_Validate,
                        Title = Resources.Resources.Content_Error,
                        Type = MessageTypeEnum.ErrorReload.ToString()
                    };
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
            }else{

                var message = new MessageModel
                {
                    Content = Resources.Resources.Message_Success_Insert,
                    Title = Resources.Resources.Content_Success,
                    Type = MessageTypeEnum.SuccessReload.ToString(),
                    ClosePopup = true
                };
                //Check table Available
                TableBll tableBll = new TableBll();
                var distanceTime = WebSetting.GetTimeDistanceSetting();
                foreach (var item in orderModel.Orderdetails)
                {
                    var orderToCheck = tableBll.GetOrderByTableIdAndTime(item.TableId, item.OrderTime.Value);
                    var orderToCheck2 = tableBll.GetOrderByTableIdAndTime(item.TableId, item.OrderTime.Value.AddHours(distanceTime));
                    if (orderToCheck != null && orderToCheck.Id != orderModel.OrderId)
                    {
                        message = new MessageModel
                        {
                            Content = Resources.Resources.Message_Error_ChangeTable,
                            Title = Resources.Resources.Content_Error,
                            Type = MessageTypeEnum.ErrorReload.ToString()
                        };
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                    if (orderToCheck2 != null)
                    {
                        message = new MessageModel
                        {
                            Content = Resources.Resources.Message_Error_ChangeTable,
                            Title = Resources.Resources.Content_Error,
                            Type = MessageTypeEnum.ErrorReload.ToString()
                        };
                        return Json(message, JsonRequestBehavior.AllowGet);
                    }
                }
                var order = _ordersRepository.Find(orderModel.OrderId);
                foreach (var item in orderModel.Orderdetails)
                {
                    if(item.TableId != 0)
                    {
                        var oldTableId = order.OrderDetails.First(x => x.Id == item.Id).TableId;
                        foreach (var itemUpdate in order.OrderDetails.Where(x=>x.TableId == oldTableId))
                        {
                            itemUpdate.TableId = item.TableId;
                            itemUpdate.OrderTime = item.OrderTime;
                            itemUpdate.LastUpdate = DateTime.Now;
                            itemUpdate.UpdateByAdminId = Support.GetCurrentAdmin().Id;
                        }
                    }
                }
                _ordersRepository.Save(order);

                return Json(message, JsonRequestBehavior.AllowGet);

            }
        }
    }
}