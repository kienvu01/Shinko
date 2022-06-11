using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Haravan.Haravan.DataAcess;
using Haravan.Haravan.Model;

namespace Haravan.Haravan.BLL
{
    public class CallFunc2
    {
        public static Object CallFuncbyTopic( string topic, Object data)
        {
            Orders orders = new Orders();
            Customers customers = new Customers();
            switch (topic)
            {
                case "shop/update":
                    break;
                case "user/create":
                    break;
                case "user/update":
                    break;
                case "user/delete":
                    break;
                case "orders/create":
                    return orders.orders_create(data);
                case "orders/updated":
                    return orders.orders_updated(data);
                case "orders/paid":
                    return orders.orders_paid(data);
                case "orders/cancelled":
                    return orders.orders_cancelled(data);
                case "orders/fulfilled":
                    break;
                case "orders/delete":
                    break;
                case "refunds/create":
                    break;
                case "carts/create":
                    break;
                case "carts/update":
                    break;
                case "checkouts/create":
                    break;
                case "checkouts/update":
                    break;
                case "customers/create":
                    return customers.customers_create(data);
                case "customers/update":
                    return customers.customers_update(data);
                case "customers/enable":
                    break;
                case "customers/disable":
                    break;
                case "customers/delete":
                    return customers.customers_delete(data);
                    break;
                case "products/create":
                    break;
                case "products/update":
                    break;
                case "products/deleted":
                    break;
                case "collections/create":
                    break;
                case "collections/update":
                    break;
                case "collections/delete":
                    break;
                case "inventoryadjustments/create":
                    break;
                case "inventoryadjustments/update":
                    break;
                case "inventorytransfers/create":
                    break;
                case "inventorytransfers/update":
                    break;
                case "locations/create":
                    break;
                case "locations/update":
                    break;
                case "locations/delete":
                    break;
                case "inventorytransaction/create":
                    break;
                case "inventorylocationbalances/create":
                    break;
                case "inventorylocationbalances/update":
                    break;
                case "inventorylocationbalances/delete":
                    break;
                case "discounts/create":
                    break;
                case "discounts/update":
                    break;
                case "discounts/delete":
                    break;
                case "promotions/create":
                    break;
                case "promotions/update":
                    break;
                case "promotions/delete":
                    break;
                case "app/uninstalled":
                    break;
                default:
                    return new { };
                    break;
            }
            ModelsApp.ResponseData re = new ModelsApp.ResponseData("ok", "", "");
            return re;
        }

    }
}
