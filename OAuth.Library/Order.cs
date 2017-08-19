using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth.Library
{
    public class PaymentDetails
    {
        public string method_id { get; set; }
        public string method_title { get; set; }
        public bool paid { get; set; }
    }

    public class BillingAddress
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public class ShippingAddress
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
    }

    public class LineItem
    {
        public int id { get; set; }
        public string subtotal { get; set; }
        public string subtotal_tax { get; set; }
        public string total { get; set; }
        public string total_tax { get; set; }
        public string price { get; set; }
        public int quantity { get; set; }
        public string tax_class { get; set; }
        public string name { get; set; }
        public int product_id { get; set; }
        public string sku { get; set; }
        public List<object> meta { get; set; }
    }

    public class TaxLine
    {
        public int id { get; set; }
        public int rate_id { get; set; }
        public string code { get; set; }
        public string title { get; set; }
        public string total { get; set; }
        public bool compound { get; set; }
    }

    public class BillingAddress2
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public class ShippingAddress2
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
    }

    public class Customer
    {
        public int id { get; set; }
        public string created_at { get; set; }
        public string last_update { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string username { get; set; }
        public string role { get; set; }
        public int last_order_id { get; set; }
        public string last_order_date { get; set; }
        public int orders_count { get; set; }
        public string total_spent { get; set; }
        public string avatar_url { get; set; }
        public BillingAddress2 billing_address { get; set; }
        public ShippingAddress2 shipping_address { get; set; }
    }

    public class Order
    {
        public int id { get; set; }
        public string order_number { get; set; }
        public string order_key { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string completed_at { get; set; }
        public string status { get; set; }
        public string currency { get; set; }
        public string total { get; set; }
        public string subtotal { get; set; }
        public int total_line_items_quantity { get; set; }
        public string total_tax { get; set; }
        public string total_shipping { get; set; }
        public string cart_tax { get; set; }
        public string shipping_tax { get; set; }
        public string total_discount { get; set; }
        public string shipping_methods { get; set; }
        public PaymentDetails payment_details { get; set; }
        public BillingAddress billing_address { get; set; }
        public ShippingAddress shipping_address { get; set; }
        public string note { get; set; }
        public string customer_ip { get; set; }
        public string customer_user_agent { get; set; }
        public int customer_id { get; set; }
        public string view_order_url { get; set; }
        public List<LineItem> line_items { get; set; }
        public List<object> shipping_lines { get; set; }
        public List<TaxLine> tax_lines { get; set; }
        public List<object> fee_lines { get; set; }
        public List<object> coupon_lines { get; set; }
        public Customer customer { get; set; }
    }

    public class RootObject
    {
        public List<Order> orders { get; set; }
    }
}
