using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Entities;
using BookingTable.Entities.Models;
using BookingTable.Web.Helpers;

namespace BookingTable.Web.Business
{
    public class TableBll
    {
        public List<TableModel> ConvertTableToTableModelByTime(List<Table> data, DateTime time)
        {
            var distanceTime = WebSetting.GetTimeDistanceSetting();
            var culture = new CultureInfo("en-US");
            var tableSelectModels = new List<TableModel>();

            foreach (var table in data)
            {
                var tableSelectModel = new TableModel
                {
                    Id = table.Id,
                    Name = table.Name,
                    Seats = table.Seats,
                    TypeName = table.TableType.Name,
                    DepositPrice = table.TableType.DepositPrice.ToString(culture),
                    Time = time
                };

                if (table.OrderDetails == null)
                {
                    tableSelectModel.Status = "Available";

                }
                else
                {
                    var ordersDetailsOfTable = table.OrderDetails.Where(
                        x => x.Completed != true &&
                             x.OrderTime != null &&
                             x.OrderTime.Value.Date == time.Date).ToList();

                    if (!ordersDetailsOfTable.Any())
                    {
                        tableSelectModel.Status = "Available";
                    }
                    else
                    {
                        var orderBefore = ordersDetailsOfTable.FirstOrDefault(
                            x => x.Completed != true &&
                                 x.OrderTime != null &&
                                 x.OrderTime.Value <= time &&
                                 x.OrderTime.Value.AddHours(distanceTime) > time);
                        var orderAfter = ordersDetailsOfTable.Any(
                            x => x.Completed != true &&
                                 x.OrderTime != null &&
                                 x.OrderTime.Value > time &&
                                 x.OrderTime.Value < time.AddHours(distanceTime));

                        if (orderBefore == null && orderAfter == false)
                        {
                            tableSelectModel.Status = "Available";
                        }
                        else if (orderBefore != null)
                        {
                            tableSelectModel.Status = "Unavailable";
                            tableSelectModel.OrderId = orderBefore.Id;
                        }
                        else
                        {
                            tableSelectModel.Status = "Warning";
                        }

                    }
                }
                tableSelectModels.Add(tableSelectModel);
            }
            return tableSelectModels;
        }

        public Order GetOrderByTableIdAndTime(int tableId, DateTime time)
        {
            try
            {
                IOrdersRepository orderRepository = new OrdersRepository();

                var distanceTime = WebSetting.GetTimeDistanceSetting();

                var orders = orderRepository.GetOrdersByTableId(tableId);

                var order = orders.FirstOrDefault(
                            x => 
                            (x.Completed != true &&
                             x.OrderTime != null &&
                             x.OrderTime.Value <= time &&
                             x.OrderTime.Value.AddHours(distanceTime) > time)
                            || x.OrderDetails.Any(y=> y.Completed != true &&
                                                      y.OrderTime != null &&
                                                      y.OrderTime.Value <= time &&
                                                      y.OrderTime.Value.AddHours(distanceTime) > time));
                return order;
            }
            catch (Exception)
            {

                return null;
            }

        }

    }
}
