using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Helpers;

namespace The_Store.Controllers
{
    public class AdminController : Controller
    {
        StoreDBDataContext st = new StoreDBDataContext();
        // GET: Admin
        public ActionResult Index(string searchString)
        {
            //var prod = from x in st.PRODUCTs
            //         select x;
            IEnumerable<PRODUCT> prod;
            
            if (!String.IsNullOrEmpty(searchString))
            {
                prod = st.PRODUCTs.Where(a => a.item.Contains(searchString));
            }
            else
            {
                prod = st.PRODUCTs;
            }

            
            return View(prod);
        }

        public ActionResult OrderList(string searchString)
        {
            var order = from x in st.ORDERs
                        select x;

            return View(order);
        }

        public void featPic()
        {
            
        }

        // GET: Admin/Details/5
        public ActionResult Details(int id)
        {
            var prod = (from x in st.PRODUCTs
                        where x.Id == id
                        select x).FirstOrDefault();
            ViewBag.pictures = prod.IMAGEs.AsEnumerable();
            return View(prod);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                PRODUCT newProduct = new PRODUCT();

                newProduct.item = collection["item"];
                newProduct.description = collection["description"];
                newProduct.specification = collection["specification"];
                newProduct.price = decimal.Parse(collection["price"]);
                newProduct.handling_charge = decimal.Parse(collection["handling_charge"]);
                newProduct.shipping_details = collection["shipping_details"];
                newProduct.quantity = int.Parse(collection["quantity"]);

                st.PRODUCTs.InsertOnSubmit(newProduct);
                st.SubmitChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int id)
        {
            var prod = (from x in st.PRODUCTs
                        where x.Id == id
                        select x).FirstOrDefault();

            ViewBag.pictures = prod.IMAGEs.AsEnumerable();
            return View(prod);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var prod = (from x in st.PRODUCTs
                        where x.Id == id
                        select x).FirstOrDefault();

            try
            {
                // TODO: Add update logic here
                prod.item = collection["item"];
                prod.description = collection["description"];
                prod.specification = collection["specification"];
                prod.price = decimal.Parse(collection["price"]);
                prod.handling_charge = decimal.Parse(collection["handling_charge"]);
                prod.shipping_details = collection["shipping_details"];
                prod.quantity = int.Parse(collection["quantity"]);

                st.SubmitChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View(prod);
            }
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int id)
        {
            var prod = (from x in st.PRODUCTs
                        where x.Id == id
                        select x).FirstOrDefault();

            return View(prod);
        }

        // POST: Admin/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var prod = (from x in st.PRODUCTs
                        where x.Id == id
                        select x).FirstOrDefault();

            var img = from x in st.IMAGEs
                      where x.product_id == prod.Id
                      select x;
            
            try
            {
                if(prod.feature_picture != null)
                {
                    prod.feature_picture = null;
                    st.SubmitChanges();
                }

                foreach(IMAGE pic in img)
                {
                    st.IMAGEs.DeleteOnSubmit(pic);
                    deletePicFile(pic);
                }
                // TODO: Add delete logic here
                st.SubmitChanges();
                st.PRODUCTs.DeleteOnSubmit(prod);
                st.SubmitChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View(prod);
            }
        }

        public ActionResult AddPictures(int id)
        {
            var prod = (from x in st.PRODUCTs
                        where x.Id == id
                        select x).FirstOrDefault();

            var pictures = prod.IMAGEs.AsEnumerable();
            ViewBag.pictures = pictures;

            return View(prod);
        }

        [HttpPost]
        public ActionResult AddPictures(int id, HttpPostedFileBase file, FormCollection collection)
        {
            try
            {
                string[] allowedTypes = { "image/jpeg", "image/png", "image/gif" };
                string type = file.ContentType;

                if (file != null && file.ContentLength > 0 && allowedTypes.Contains(type))
                {
                    Guid g = Guid.NewGuid();

                    var fileName = g.ToString() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                    file.SaveAs(path);

                    PRODUCT newfeatPic = new PRODUCT();
//                    newfeatPic.feature_picture = path;

                    IMAGE newPic = new IMAGE();
                    newPic.path = fileName;
                    newPic.product_id = id;
                    newPic.description = collection["Description"];
                    st.IMAGEs.InsertOnSubmit(newPic);
                    st.SubmitChanges();
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private void deletePicFile(IMAGE picture)
        {
            //delete the file...
            string aPath = "~/Images/" + picture.path;
            aPath = HttpContext.Server.MapPath(aPath);

            FileInfo file = new FileInfo(aPath);
            if (file.Exists)//check file exsit or not
            {
                file.Delete();
            }
        }

        public ActionResult DeletePicture(int id)
        {
            try
            {
                // TODO: Add delete logic here

                IMAGE picture = (from p in st.IMAGEs
                                 where p.Id == id
                                 select p).FirstOrDefault();

                var prod = from x in st.PRODUCTs
                           where x.Id == picture.Id
                           select x;

                st.IMAGEs.DeleteOnSubmit(picture);
                st.SubmitChanges();

                deletePicFile(picture);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //LOGIN

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login (FormCollection collection)
        {
            string user_name = collection["user_name"];
            string password = collection["password_hash"];

            string hash = (from h in st.USERs
                           where h.user_name == user_name
                           select h.password_hash).SingleOrDefault();

            if(Crypto.VerifyHashedPassword(hash, password))
            {
                Session["user"] = user_name;
            }

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult ConfirmLogin()
        {
            string user_name = (string)Session["user"];

            var user = (from u in st.USERs
                        where u.user_name == user_name
                        select u).SingleOrDefault();

            if(user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
        }

        public ActionResult LogOut()
        {
            Session["user"] = null;
            return RedirectToAction("Index", "Admin");
        }
        [HttpGet]
        public ActionResult Register()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();

            var items = from c in st.COUNTRies
                        select c;

            foreach (COUNTRY item in items)
            {
                listItems.Add(new SelectListItem() { Text = item.country_name, Value = item.Id.ToString() });
            }
            ViewBag.country = listItems;

            return View();
        }

        [HttpPost]
        public ActionResult Register(FormCollection collection)
        {
            USER newUser = new USER();
            newUser.user_name = collection["user_name"];
            newUser.password_hash = Crypto.HashPassword(collection["password_hash"]);
            newUser.country_id = int.Parse(collection["country_id"]);
            newUser.first_name = collection["first_name"];
            newUser.last_name = collection["last_name"];

            st.USERs.InsertOnSubmit(newUser);
            st.SubmitChanges();

            return RedirectToAction("Index", "Admin");
        }
    }
}
