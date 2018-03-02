using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace The_Store.Controllers
{
    public class ClientController : Controller
    {
        StoreDBDataContext st = new StoreDBDataContext();
        // GET: Client
        public ActionResult Index(string searchString)
        {
            var prod = from x in st.IMAGEs
                       select x;

            if (!String.IsNullOrEmpty(searchString))
            {
                prod = prod.Where(a => a.PRODUCT.item.Contains(searchString));
            }
            return View(prod);
        }

        // GET: Client/Details/5
        public ActionResult Details(int id)
        {
            var prod = (from x in st.PRODUCTs
                        where x.Id == id
                        select x).FirstOrDefault();
            ViewBag.pictures = prod.IMAGEs.AsEnumerable();

            return View(prod);
        }
        
        public ActionResult AddToCart(int id)
        {
            var prod = from x in st.PRODUCTs
                       where x.Id == id
                       select x;

            ORDER_DETAIL newOrder = new ORDER_DETAIL();
            newOrder.PRODUCT = prod.FirstOrDefault();
            newOrder.quantity = 1;

            if(Session["cart"] == null)
            {
                Session["cart"] = new List<ORDER_DETAIL>();
            }

            var item = (from x in (List<ORDER_DETAIL>)Session["cart"]
                        where x.product_id == id
                        select x).FirstOrDefault();

            if(item == null)
            {
                ((List<ORDER_DETAIL>)Session["cart"]).Add(newOrder);
            }
            else
            {
                item.quantity += 1;
            }
            

            return RedirectToAction("ViewCart");
        }

        public ActionResult ViewCart()
        {
            var cart = from x in ((List<ORDER_DETAIL>)Session["cart"])
                       select x;
            
            return View(cart);
        }

        public ActionResult DeleteFromCart(int id, FormCollection collection)
        {
            var item = (from x in ((List<ORDER_DETAIL>)Session["cart"])
                        where x.product_id == id
                        select x).FirstOrDefault();

            if(item.quantity >= 2)
            {
                item.quantity -= 1;
            } else
            {
                ((List<ORDER_DETAIL>)Session["cart"]).Remove(item);
            }

            return RedirectToAction("ViewCart");
        }

        public ActionResult CheckOut()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CheckOut(FormCollection collection)
        {
            try
            {
                ORDER newOrder = new ORDER();

                newOrder.first_name = collection["first_name"];
                newOrder.last_name = collection["last_name"];
                newOrder.street_address = collection["street_address"];
                newOrder.city = collection["city"];
                newOrder.province = collection["province"];
                newOrder.postal_code = collection["postal_code"];
                newOrder.country_id = int.Parse(collection["country_id"]);
                newOrder.status = "processing";

                st.ORDERs.InsertOnSubmit(newOrder);
                st.SubmitChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
